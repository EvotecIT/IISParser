using System;
using System.Collections.Concurrent;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace IISParser.PowerShell;

/// <summary>Provides asynchronous execution support for cmdlets.</summary>
/// <para>Derived classes override asynchronous lifecycle methods to interact with the PowerShell pipeline without blocking.</para>
/// <list type="alertSet">
/// <item>
/// <term>Note</term>
/// <description>Override <c>BeginProcessingAsync</c>, <c>ProcessRecordAsync</c>, and <c>EndProcessingAsync</c> instead of their synchronous counterparts.</description>
/// </item>
/// </list>
/// <seealso href="https://learn.microsoft.com/powershell/developer/cmdlet/cmdlet-overview" />
/// <seealso href="https://github.com/EvotecIT/IISParser" />
public abstract class AsyncPSCmdlet : PSCmdlet, IDisposable {
    private readonly CancellationTokenSource _cancelSource = new();

    private BlockingCollection<(object?, PipelineType)>? _currentOutPipe;
    private BlockingCollection<object?>? _currentReplyPipe;

    /// <summary>
    /// Gets a cancellation token that is triggered when the cmdlet is stopped.
    /// </summary>
    protected internal CancellationToken CancelToken => _cancelSource.Token;

    /// <inheritdoc />
    public void Dispose() {
        _cancelSource?.Dispose();
    }

    /// <inheritdoc />
    protected override void BeginProcessing() {
        RunBlockInAsync(BeginProcessingAsync);
    }

    /// <summary>
    /// Asynchronously performs initialization before record processing begins.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task BeginProcessingAsync() {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        RunBlockInAsync(ProcessRecordAsync);
    }

    /// <summary>
    /// Asynchronously processes each record.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task ProcessRecordAsync() {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override void EndProcessing() {
        RunBlockInAsync(EndProcessingAsync);
    }

    /// <summary>
    /// Asynchronously performs cleanup after processing is complete.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task EndProcessingAsync() {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override void StopProcessing() {
        _cancelSource?.Cancel();
    }

    private void RunBlockInAsync(Func<Task> task) {
        using BlockingCollection<(object?, PipelineType)> outPipe = new();
        using BlockingCollection<object?> replyPipe = new();
        Task blockTask = Task.Run(async () => {
            try {
                _currentOutPipe = outPipe;
                _currentReplyPipe = replyPipe;
                await task();
            } finally {
                _currentOutPipe = null;
                _currentReplyPipe = null;
                outPipe.CompleteAdding();
                replyPipe.CompleteAdding();
            }
        });

        foreach ((object? data, PipelineType pipelineType) in outPipe.GetConsumingEnumerable()) {
            switch (pipelineType) {
                case PipelineType.Output:
                    base.WriteObject(data);
                    break;
                case PipelineType.OutputEnumerate:
                    base.WriteObject(data, true);
                    break;
                case PipelineType.Error:
                    base.WriteError((ErrorRecord)data!);
                    break;
                case PipelineType.Warning:
                    base.WriteWarning((string)data!);
                    break;
                case PipelineType.Verbose:
                    base.WriteVerbose((string)data!);
                    break;
                case PipelineType.Debug:
                    base.WriteDebug((string)data!);
                    break;
                case PipelineType.Information:
                    base.WriteInformation((InformationRecord)data!);
                    break;
                case PipelineType.Progress:
                    base.WriteProgress((ProgressRecord)data!);
                    break;
                case PipelineType.ShouldProcess:
                    (string target, string action) = (ValueTuple<string, string>)data!;
                    bool res = base.ShouldProcess(target, action);
                    replyPipe.Add(res);
                    break;
            }
        }

        blockTask.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Determines whether the cmdlet should continue processing.
    /// </summary>
    /// <param name="target">The target of the operation.</param>
    /// <param name="action">The action to be performed.</param>
    /// <returns><c>true</c> if processing should continue; otherwise, <c>false</c>.</returns>
    public new bool ShouldProcess(string target, string action) {
        ThrowIfStopped();
        _currentOutPipe?.Add(((target, action), PipelineType.ShouldProcess));
        return (bool)_currentReplyPipe?.Take(CancelToken)!;
    }

    /// <summary>
    /// Writes an object to the output pipeline.
    /// </summary>
    /// <param name="sendToPipeline">The object to write.</param>
    public new void WriteObject(object? sendToPipeline) {
        WriteObject(sendToPipeline, false);
    }

    /// <summary>
    /// Writes an object to the output pipeline with optional enumeration of collections.
    /// </summary>
    /// <param name="sendToPipeline">The object to write.</param>
    /// <param name="enumerateCollection">Whether to enumerate the object if it is a collection.</param>
    public new void WriteObject(object? sendToPipeline, bool enumerateCollection) {
        ThrowIfStopped();
        _currentOutPipe?.Add((sendToPipeline, enumerateCollection ? PipelineType.OutputEnumerate : PipelineType.Output));
    }

    /// <summary>
    /// Writes an error record to the error pipeline.
    /// </summary>
    /// <param name="errorRecord">The error to write.</param>
    public new void WriteError(ErrorRecord errorRecord) {
        ThrowIfStopped();
        _currentOutPipe?.Add((errorRecord, PipelineType.Error));
    }

    /// <summary>
    /// Writes a warning message to the warning pipeline.
    /// </summary>
    /// <param name="message">The warning message.</param>
    public new void WriteWarning(string message) {
        ThrowIfStopped();
        _currentOutPipe?.Add((message, PipelineType.Warning));
    }

    /// <summary>
    /// Writes a verbose message to the verbose pipeline.
    /// </summary>
    /// <param name="message">The verbose message.</param>
    public new void WriteVerbose(string message) {
        ThrowIfStopped();
        _currentOutPipe?.Add((message, PipelineType.Verbose));
    }

    /// <summary>
    /// Writes a debug message to the debug pipeline.
    /// </summary>
    /// <param name="message">The debug message.</param>
    public new void WriteDebug(string message) {
        ThrowIfStopped();
        _currentOutPipe?.Add((message, PipelineType.Debug));
    }

    /// <summary>
    /// Writes information to the information pipeline.
    /// </summary>
    /// <param name="informationRecord">The information record.</param>
    public new void WriteInformation(InformationRecord informationRecord) {
        ThrowIfStopped();
        _currentOutPipe?.Add((informationRecord, PipelineType.Information));
    }

    /// <summary>
    /// Writes a progress record to the progress pipeline or directly if asynchronous output is not active.
    /// </summary>
    /// <param name="progressRecord">The progress record.</param>
    public new void WriteProgress(ProgressRecord progressRecord) {
        ThrowIfStopped();
        if (_currentOutPipe != null) {
            _currentOutPipe.Add((progressRecord, PipelineType.Progress));
        } else {
            base.WriteProgress(progressRecord);
        }
    }

    internal void ThrowIfStopped() {
        if (_cancelSource.IsCancellationRequested) {
            throw new PipelineStoppedException();
        }
    }

    /// <summary>
    /// Retrieves the effective <see cref="ActionPreference"/> for error handling.
    /// </summary>
    /// <returns>The resolved <see cref="ActionPreference"/>.</returns>
    protected ActionPreference GetErrorActionPreference() {
        if (MyInvocation.BoundParameters.ContainsKey("ErrorAction")) {
            string? errorActionString = MyInvocation.BoundParameters["ErrorAction"]?.ToString();
            if (!string.IsNullOrWhiteSpace(errorActionString) && Enum.TryParse(errorActionString, true, out ActionPreference parsed)) {
                return parsed;
            }
        }

        object? preference = GetVariableValue("ErrorActionPreference");
        if (preference is ActionPreference actionPreference) {
            return actionPreference;
        }

        if (preference is string preferenceString && Enum.TryParse(preferenceString, true, out ActionPreference parsedPreference)) {
            return parsedPreference;
        }

        return ActionPreference.Continue;
    }

    /// <summary>
    /// Ensures that the specified file exists, writing a warning or terminating error as appropriate.
    /// </summary>
    /// <param name="path">The file path to check.</param>
    /// <param name="errorAction">The action preference determining error handling.</param>
    /// <param name="resolvedPath">The resolved provider path.</param>
    /// <returns><c>true</c> if the file exists; otherwise, <c>false</c>.</returns>
    protected bool EnsureFileExists(string path, ActionPreference errorAction, out string resolvedPath) {
        try {
            resolvedPath = GetUnresolvedProviderPathFromPSPath(path);
        } catch (ItemNotFoundException) {
            resolvedPath = path;
        }

        if (File.Exists(resolvedPath)) {
            return true;
        }

        string message = $"{MyInvocation.InvocationName} - The specified file does not exist: {resolvedPath}";
        if (errorAction == ActionPreference.Stop) {
            FileNotFoundException ex = new("The specified file does not exist.", resolvedPath);
            ThrowTerminatingError(new ErrorRecord(ex, "FileNotFound", ErrorCategory.ObjectNotFound, resolvedPath));
        } else {
            LoggingMessages.Logger.WriteWarning(message);
        }

        return false;
    }

    private enum PipelineType {
        Output,
        OutputEnumerate,
        Error,
        Warning,
        Verbose,
        Debug,
        Information,
        Progress,
        ShouldProcess
    }
}