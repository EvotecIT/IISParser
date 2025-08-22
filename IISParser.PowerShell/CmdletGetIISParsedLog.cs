using IISParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;

namespace IISParser.PowerShell;

/// <summary>Parses entries from an IIS log file.</summary>
/// <para>Reads the specified log and converts each record into a PowerShell object for further processing.</para>
/// <para>Use filtering parameters to limit the number of events returned.</para>
/// <list type="alertSet">
/// <item>
/// <term>Note</term>
/// <description>The cmdlet loads the entire log into memory before applying filters, which may impact large files.</description>
/// </item>
/// </list>
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
        var list = events.ToList();
        if (ParameterSetName == "FirstLastSkip") {
            if (Skip.HasValue) list = list.Skip(Skip.Value).ToList();
            if (First.HasValue) list = list.Take(First.Value).ToList();
            if (Last.HasValue) list = list.Skip(Math.Max(0, list.Count - Last.Value)).ToList();
        } else if (ParameterSetName == "SkipLast" && SkipLast.HasValue) {
            list = list.Take(Math.Max(0, list.Count - SkipLast.Value)).ToList();
        }

        var output = new List<PSObject>();
        foreach (var evt in list) {
            var psObj = PSObject.AsPSObject(evt);
            foreach (var kv in evt.Fields) {
                if (psObj.Properties[kv.Key] == null) {
                    psObj.Properties.Add(new PSNoteProperty(kv.Key, kv.Value));
                }
            }
            output.Add(psObj);
        }
        WriteObject(output, true);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override Task EndProcessingAsync() {
        _parser?.Dispose();
        return Task.CompletedTask;
    }
}