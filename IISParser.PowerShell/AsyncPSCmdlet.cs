using System;
using System.Collections.Concurrent;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace ADPlayground.PowerShell;

public abstract class AsyncPSCmdlet : PSCmdlet, IDisposable {
    private readonly CancellationTokenSource _cancelSource = new();

    private BlockingCollection<(object?, PipelineType)>? _currentOutPipe;
    private BlockingCollection<object?>? _currentReplyPipe;

    protected internal CancellationToken CancelToken => _cancelSource.Token;

    public void Dispose() {
        _cancelSource?.Dispose();
    }

    protected override void BeginProcessing() {
        RunBlockInAsync(BeginProcessingAsync);
    }

    protected virtual Task BeginProcessingAsync() {
        return Task.CompletedTask;
    }

    protected override void ProcessRecord() {
        RunBlockInAsync(ProcessRecordAsync);
    }

    protected virtual Task ProcessRecordAsync() {
        return Task.CompletedTask;
    }

    protected override void EndProcessing() {
        RunBlockInAsync(EndProcessingAsync);
    }

    protected virtual Task EndProcessingAsync() {
        return Task.CompletedTask;
    }

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

    public new bool ShouldProcess(string target, string action) {
        ThrowIfStopped();
        _currentOutPipe?.Add(((target, action), PipelineType.ShouldProcess));
        return (bool)_currentReplyPipe?.Take(CancelToken)!;
    }

    public new void WriteObject(object? sendToPipeline) {
        WriteObject(sendToPipeline, false);
    }

    public new void WriteObject(object? sendToPipeline, bool enumerateCollection) {
        ThrowIfStopped();
        _currentOutPipe?.Add((sendToPipeline, enumerateCollection ? PipelineType.OutputEnumerate : PipelineType.Output));
    }

    public new void WriteError(ErrorRecord errorRecord) {
        ThrowIfStopped();
        _currentOutPipe?.Add((errorRecord, PipelineType.Error));
    }

    public new void WriteWarning(string message) {
        ThrowIfStopped();
        _currentOutPipe?.Add((message, PipelineType.Warning));
    }

    public new void WriteVerbose(string message) {
        ThrowIfStopped();
        _currentOutPipe?.Add((message, PipelineType.Verbose));
    }

    public new void WriteDebug(string message) {
        ThrowIfStopped();
        _currentOutPipe?.Add((message, PipelineType.Debug));
    }

    public new void WriteInformation(InformationRecord informationRecord) {
        ThrowIfStopped();
        _currentOutPipe?.Add((informationRecord, PipelineType.Information));
    }

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

    protected bool EnsureFileExists(string path, ActionPreference errorAction) {
        if (File.Exists(path)) {
            return true;
        }

        string message = $"{MyInvocation.InvocationName} - The specified file does not exist: {path}";
        if (errorAction == ActionPreference.Stop) {
            FileNotFoundException ex = new("The specified file does not exist.", path);
            ThrowTerminatingError(new ErrorRecord(ex, "FileNotFound", ErrorCategory.ObjectNotFound, path));
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