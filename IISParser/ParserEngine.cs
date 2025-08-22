using System;
using System.Collections.Generic;
using System.IO;

namespace IISParser;

public class ParserEngine : IDisposable {
    private string[]? _headerFields;
    private readonly Dictionary<string, string?> _dataStruct = new(StringComparer.OrdinalIgnoreCase);
    private readonly int _mbSize;

    public string FilePath { get; }
    public bool MissingRecords { get; private set; } = true;
    public int MaxFileRecord2Read { get; set; } = 1000000;
    public int CurrentFileRecord { get; private set; }

    public ParserEngine(string filePath) {
        if (!File.Exists(filePath))
            throw new Exception("Could not find File " + filePath);
        FilePath = filePath;
        _mbSize = (int)(new FileInfo(filePath).Length / 1024 / 1024);
    }

    public IEnumerable<IISLogEvent> ParseLog() => _mbSize < 50 ? QuickProcess() : LongProcess();

    private IEnumerable<IISLogEvent> QuickProcess() {
        var events = new List<IISLogEvent>();
        foreach (var line in Utils.ReadAllLines(FilePath))
            ProcessLine(line, events);
        MissingRecords = false;
        return events;
    }

    private IEnumerable<IISLogEvent> LongProcess() {
        var events = new List<IISLogEvent>();
        MissingRecords = false;
        using var fileStream = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(fileStream);
        while (reader.Peek() > -1) {
            ProcessLine(reader.ReadLine() ?? string.Empty, events);
            if (events.Count > 0 && events.Count % MaxFileRecord2Read == 0) {
                MissingRecords = true;
                break;
            }
        }
        return events;
    }

    private void ProcessLine(string line, List<IISLogEvent> events) {
        if (line.StartsWith("#Fields:", StringComparison.OrdinalIgnoreCase))
            _headerFields = line.Replace("#Fields: ", string.Empty).Split(' ');
        if (line.StartsWith("#", StringComparison.OrdinalIgnoreCase) || _headerFields == null)
            return;
        FillDataStruct(line.Split(' '), _headerFields);
        events.Add(NewEventObj());
        CurrentFileRecord++;
    }

    private IISLogEvent NewEventObj() {
        var evt = new IISLogEvent();
        foreach (var kv in _dataStruct)
            evt.Fields[kv.Key] = kv.Value;
        evt.DateTimeEvent = GetEventDateTime();
        evt.sSitename = GetValue("s-sitename");
        evt.sComputername = GetValue("s-computername");
        evt.sIp = GetValue("s-ip");
        evt.csMethod = GetValue("cs-method");
        evt.csUriStem = GetValue("cs-uri-stem");
        evt.csUriQuery = GetValue("cs-uri-query");
        evt.sPort = GetInt("s-port");
        evt.csUsername = GetValue("cs-username");
        evt.cIp = GetValue("c-ip");
        evt.csVersion = GetValue("cs-version");
        evt.csUserAgent = GetValue("cs(User-Agent)");
        evt.csCookie = GetValue("cs(Cookie)");
        evt.csReferer = GetValue("cs(Referer)");
        evt.csHost = GetValue("cs-host");
        evt.scStatus = GetInt("sc-status");
        evt.scSubstatus = GetInt("sc-substatus");
        evt.scWin32Status = GetLong("sc-win32-status");
        evt.scBytes = GetInt("sc-bytes");
        evt.csBytes = GetInt("cs-bytes");
        evt.timeTaken = GetInt("time-taken");
        return evt;
    }

    private DateTime GetEventDateTime()
        => DateTime.Parse($"{GetValue("date")} {GetValue("time")}");

    private void FillDataStruct(string[] fieldsData, string[] header) {
        _dataStruct.Clear();
        for (int i = 0; i < header.Length; i++)
            _dataStruct[header[i]] = fieldsData[i] == "-" ? null : fieldsData[i];
    }

    private string? GetValue(string key) => _dataStruct.TryGetValue(key, out var v) ? v : null;
    private int? GetInt(string key) => int.TryParse(GetValue(key), out var v) ? v : null;
    private long? GetLong(string key) => long.TryParse(GetValue(key), out var v) ? v : null;

    public void Dispose() { }
}