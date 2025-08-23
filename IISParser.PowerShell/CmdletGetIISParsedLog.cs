using IISParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;

namespace IISParser.PowerShell;

/// <summary>Parses entries from an IIS log file.</summary>
/// <para>Reads the specified log and converts each record into a PowerShell object for further processing.</para>
/// <para>Entries are streamed lazily to minimize memory usage.</para>
/// <para>Use filtering parameters to limit the number of events returned.</para>
/// <example>
/// <summary>Parse an entire log file.</summary>
/// <prefix>PS&gt; </prefix>
/// <code>Get-IISParsedLog -FilePath "C:\\Logs\\u_ex230101.log"</code>
/// <para>Outputs all entries from the specified log.</para>
/// </example>
/// <example>
/// <summary>Retrieve a subset of entries.</summary>
/// <prefix>PS&gt; </prefix>
/// <code>Get-IISParsedLog -FilePath "C:\\Logs\\u_ex230101.log" -Skip 10 -First 5</code>
/// <para>Skips the first ten lines and returns the next five.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/iis/configuration/system.webserver/httplogging" />
/// <seealso href="https://github.com/EvotecIT/IISParser" />
[Cmdlet(VerbsCommon.Get, "IISParsedLog", DefaultParameterSetName = "Default")]
public class CmdletGetIISParsedLog : AsyncPSCmdlet {
    private ParserEngine? _parser;

    /// <summary>Path to the IIS log file.</summary>
    [Parameter(Mandatory = true, ParameterSetName = "Default")]
    [Parameter(Mandatory = true, ParameterSetName = "FirstLastSkip")]
    [Parameter(Mandatory = true, ParameterSetName = "SkipLast")]
    [Alias("LogPath")]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Selects the first number of log entries to return.</summary>
    [Parameter(ParameterSetName = "FirstLastSkip")]
    public int? First { get; set; }

    /// <summary>Returns only the last number of log entries.</summary>
    [Parameter(ParameterSetName = "FirstLastSkip")]
    public int? Last { get; set; }

    /// <summary>Skips a specified number of entries from the start.</summary>
    [Parameter(ParameterSetName = "FirstLastSkip")]
    public int? Skip { get; set; }

    /// <summary>Omits a specified number of entries from the end.</summary>
    [Parameter(ParameterSetName = "SkipLast")]
    public int? SkipLast { get; set; }

    /// <summary>Expands fields into top-level properties.</summary>
    [Parameter]
    public SwitchParameter Expand { get; set; }

    /// <inheritdoc />
    protected override Task BeginProcessingAsync() {
        ActionPreference pref = GetErrorActionPreference();
        if (EnsureFileExists(FilePath, pref)) {
            _parser = new ParserEngine(FilePath);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override Task ProcessRecordAsync() {
        if (_parser == null) {
            return Task.CompletedTask;
        }

        IEnumerable<IISLogEvent> events = _parser.ParseLog();

        if (ParameterSetName == "FirstLastSkip") {
            if (Skip.HasValue && Skip.Value > 0) {
                events = events.Skip(Skip.Value);
            }

            if (First.HasValue) {
                events = events.Take(First.Value);
            }

            if (Last.HasValue && Last.Value > 0) {
                events = TakeLastLazy(events, Last.Value);
            }
        } else if (ParameterSetName == "SkipLast" && SkipLast.HasValue && SkipLast.Value > 0) {
            events = SkipLastLazy(events, SkipLast.Value);
        }

        foreach (var evt in events) {
            WriteEvent(evt);
        }

        return Task.CompletedTask;
    }

    private static IEnumerable<T> TakeLastLazy<T>(IEnumerable<T> source, int count) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (count <= 0) {
            yield break;
        }

        var queue = new Queue<T>(count);
        foreach (var item in source) {
            if (queue.Count == count) {
                queue.Dequeue();
            }
            queue.Enqueue(item);
        }

        foreach (var item in queue) {
            yield return item;
        }
    }

    private static IEnumerable<T> SkipLastLazy<T>(IEnumerable<T> source, int count) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (count <= 0) {
            foreach (var item in source) {
                yield return item;
            }
            yield break;
        }

        var queue = new Queue<T>(count + 1);
        foreach (var item in source) {
            queue.Enqueue(item);
            if (queue.Count > count) {
                yield return queue.Dequeue();
            }
        }
    }

    private void WriteEvent(IISLogEvent evt) {
        if (Expand) {
            var psObj = new PSObject();
            foreach (var prop in PSObject.AsPSObject(evt).Properties) {
                if (!prop.Name.Equals("Fields", StringComparison.OrdinalIgnoreCase)) {
                    psObj.Properties.Add(new PSNoteProperty(prop.Name, prop.Value));
                }
            }
            foreach (var kv in evt.Fields)
                psObj.Properties.Add(new PSNoteProperty(kv.Key, kv.Value));
            WriteObject(psObj);
        } else {
            WriteObject(evt);
        }
    }

    /// <inheritdoc />
    protected override Task EndProcessingAsync() {
        _parser?.Dispose();
        return Task.CompletedTask;
    }
}