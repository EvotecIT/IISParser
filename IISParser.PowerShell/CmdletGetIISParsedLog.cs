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
/// <example>
/// <summary>Expand fields into PowerShell-friendly properties.</summary>
/// <prefix>PS&gt; </prefix>
/// <code>Get-IISParsedLog -FilePath "C:\\Logs\\u_ex230101.log" -Expand</code>
/// <para>Field names such as <c>X-Forwarded-For</c> become <c>X_Forwarded_For</c>.</para>
/// </example>
/// <example>
/// <summary>Return legacy property names.</summary>
/// <prefix>PS&gt; </prefix>
/// <code>Get-IISParsedLog -FilePath "C:\\Logs\\u_ex230101.log" -Legacy</code>
/// <para>Outputs entries using the original property names.</para>
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

    /// <summary>
    /// Expands fields into top-level properties.
    /// Field names are transformed into PowerShell-friendly identifiers by
    /// replacing <c>-</c> with <c>_</c> and removing parentheses.
    /// </summary>
    [Parameter]
    public SwitchParameter Expand { get; set; }

    /// <summary>Outputs objects with legacy property names.</summary>
    [Parameter]
    public SwitchParameter Legacy { get; set; }

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

        if (Legacy) {
            IEnumerable<IISLogEvent> events = _parser.ParseLogLegacy();
            if (ParameterSetName == "FirstLastSkip") {
                if (Skip.HasValue && Skip.Value > 0) {
                    events = events.Skip(Skip.Value);
                }

                if (First.HasValue) {
                    events = events.Take(First.Value);
                }

                if (Last.HasValue && Last.Value > 0) {
                    events = events.TakeLastLazy(Last.Value);
                }
            } else if (ParameterSetName == "SkipLast" && SkipLast.HasValue && SkipLast.Value > 0) {
                events = events.SkipLastLazy(SkipLast.Value);
            }

            foreach (var evt in events) {
                WriteEvent(evt);
            }
        } else {
            IEnumerable<IISLogRecord> records = _parser.ParseLog();
            if (ParameterSetName == "FirstLastSkip") {
                if (Skip.HasValue && Skip.Value > 0) {
                    records = records.Skip(Skip.Value);
                }

                if (First.HasValue) {
                    records = records.Take(First.Value);
                }

                if (Last.HasValue && Last.Value > 0) {
                    records = records.TakeLastLazy(Last.Value);
                }
            } else if (ParameterSetName == "SkipLast" && SkipLast.HasValue && SkipLast.Value > 0) {
                records = records.SkipLastLazy(SkipLast.Value);
            }

            foreach (var record in records) {
                WriteRecord(record);
            }
        }
        return Task.CompletedTask;
    }

    private static string ToPsIdentifier(string key) {
        var chars = key.ToCharArray();
        for (int i = 0; i < chars.Length; i++) {
            chars[i] = chars[i] switch {
                '-' => '_',
                '(' or ')' => '\0',
                _ => chars[i]
            };
        }

        return new string(chars.Where(c => c != '\0').ToArray());
    }

    private void WriteEvent(IISLogEvent evt) {
        if (Expand) {
            var psObj = new PSObject();
            foreach (var prop in PSObject.AsPSObject(evt).Properties) {
                if (!prop.Name.Equals("Fields", StringComparison.OrdinalIgnoreCase)) {
                    psObj.Properties.Add(new PSNoteProperty(prop.Name, prop.Value));
                }
            }

            foreach (var kv in evt.Fields) {
                var key = ToPsIdentifier(kv.Key);
                psObj.Properties.Add(new PSNoteProperty(key, kv.Value));
            }

            WriteObject(psObj);
        } else {
            WriteObject(evt);
        }
    }

    private void WriteRecord(IISLogRecord record) {
        if (Expand) {
            var psObj = new PSObject();
            foreach (var prop in PSObject.AsPSObject(record).Properties) {
                if (!prop.Name.Equals("Fields", StringComparison.OrdinalIgnoreCase)) {
                    psObj.Properties.Add(new PSNoteProperty(prop.Name, prop.Value));
                }
            }

            foreach (var kv in record.Fields) {
                var key = ToPsIdentifier(kv.Key);
                psObj.Properties.Add(new PSNoteProperty(key, kv.Value));
            }

            WriteObject(psObj);
        } else {
            WriteObject(record);
        }
    }

    /// <inheritdoc />
    protected override Task EndProcessingAsync() {
        _parser?.Dispose();
        return Task.CompletedTask;
    }
}