using IISParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using ADPlayground.PowerShell;

namespace IISParser.PowerShell;

[Cmdlet(VerbsCommon.Get, "IISParsedLog", DefaultParameterSetName = "Default")]
public class GetIISParsedLogCommand : AsyncPSCmdlet
{
    private ParserEngine? _parser;

    [Parameter(Mandatory = true, ParameterSetName = "Default")]
    [Parameter(Mandatory = true, ParameterSetName = "FirstLastSkip")]
    [Parameter(Mandatory = true, ParameterSetName = "SkipLast")]
    [Alias("LogPath")]
    public string FilePath { get; set; } = string.Empty;

    [Parameter(ParameterSetName = "FirstLastSkip")]
    public int? First { get; set; }

    [Parameter(ParameterSetName = "FirstLastSkip")]
    public int? Last { get; set; }

    [Parameter(ParameterSetName = "FirstLastSkip")]
    public int? Skip { get; set; }

    [Parameter(ParameterSetName = "SkipLast")]
    public int? SkipLast { get; set; }

    protected override Task BeginProcessingAsync()
    {
        ActionPreference pref = GetErrorActionPreference();
        if (EnsureFileExists(FilePath, pref))
        {
            _parser = new ParserEngine(FilePath);
        }
        return Task.CompletedTask;
    }

    protected override Task ProcessRecordAsync()
    {
        if (_parser == null)
        {
            return Task.CompletedTask;
        }

        IEnumerable<IISLogEvent> events = _parser.ParseLog();
        var list = events.ToList();
        if (ParameterSetName == "FirstLastSkip")
        {
            if (Skip.HasValue) list = list.Skip(Skip.Value).ToList();
            if (First.HasValue) list = list.Take(First.Value).ToList();
            if (Last.HasValue) list = list.Skip(Math.Max(0, list.Count - Last.Value)).ToList();
        }
        else if (ParameterSetName == "SkipLast" && SkipLast.HasValue)
        {
            list = list.Take(Math.Max(0, list.Count - SkipLast.Value)).ToList();
        }

        var output = new List<PSObject>();
        foreach (var evt in list)
        {
            var psObj = PSObject.AsPSObject(evt);
            foreach (var kv in evt.Fields)
            {
                if (psObj.Properties[kv.Key] == null)
                {
                    psObj.Properties.Add(new PSNoteProperty(kv.Key, kv.Value));
                }
            }
            output.Add(psObj);
        }
        WriteObject(output, true);
        return Task.CompletedTask;
    }

    protected override Task EndProcessingAsync()
    {
        _parser?.Dispose();
        return Task.CompletedTask;
    }
}
