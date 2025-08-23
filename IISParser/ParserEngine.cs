using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace IISParser;

/// <summary>
/// Parses IIS log files and exposes the entries as <see cref="IISLogEvent"/> objects.
/// </summary>
public class ParserEngine : IDisposable {
    private string[]? _headerFields;
    private readonly Dictionary<string, string?> _dataStruct = new(StringComparer.OrdinalIgnoreCase);
    private readonly int _mbSize;

    private static readonly HashSet<string> KnownFields = new(StringComparer.OrdinalIgnoreCase) {
        "date",
        "time",
        "s-sitename",
        "s-computername",
        "s-ip",
        "cs-method",
        "cs-uri-stem",
        "cs-uri-query",
        "s-port",
        "cs-username",
        "c-ip",
        "cs-version",
        "cs(User-Agent)",
        "cs(Cookie)",
        "cs(Referer)",
        "cs-host",
        "sc-status",
        "sc-substatus",
        "sc-win32-status",
        "sc-bytes",
        "cs-bytes",
        "time-taken"
    };

    /// <summary>
    /// Gets the path to the log file being processed.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets a value indicating whether the parser could not read all records from the file.
    /// </summary>
    public bool MissingRecords { get; private set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of records to read before stopping.
    /// </summary>
    public int MaxFileRecord2Read { get; set; } = 1000000;

    /// <summary>
    /// Gets the number of records processed so far.
    /// </summary>
    public int CurrentFileRecord { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParserEngine"/> class for the specified file.
    /// </summary>
    /// <param name="filePath">The path to the IIS log file.</param>
    /// <exception cref="Exception">Thrown when the file does not exist.</exception>
    public ParserEngine(string filePath) {
        if (!File.Exists(filePath))
            throw new Exception("Could not find File " + filePath);
        FilePath = filePath;
        _mbSize = (int)(new FileInfo(filePath).Length / 1024 / 1024);
    }

    /// <summary>
    /// Parses the log file and returns the entries as an enumerable sequence.
    /// </summary>
    /// <returns>A sequence of <see cref="IISLogEvent"/> instances.</returns>
    public IEnumerable<IISLogEvent> ParseLog() => _mbSize < 50 ? QuickProcess() : LongProcess();

    private IEnumerable<IISLogEvent> QuickProcess() {
        MissingRecords = false;
        foreach (var line in Utils.ReadAllLines(FilePath)) {
            var evt = ProcessLine(line);
            if (evt != null)
                yield return evt;
        }
    }

    private IEnumerable<IISLogEvent> LongProcess() {
        MissingRecords = false;
        using var fileStream = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(fileStream);
        while (reader.Peek() > -1) {
            var evt = ProcessLine(reader.ReadLine() ?? string.Empty);
            if (evt != null) {
                if (CurrentFileRecord % MaxFileRecord2Read == 0 && reader.Peek() > -1) {
                    MissingRecords = true;
                    yield return evt;
                    yield break;
                }
                yield return evt;
            }
        }
    }

    private IISLogEvent? ProcessLine(string line) {
        if (line.StartsWith("#Fields:", StringComparison.OrdinalIgnoreCase))
            _headerFields = line.Replace("#Fields: ", string.Empty).Split(' ');
        if (line.StartsWith("#", StringComparison.OrdinalIgnoreCase) || _headerFields == null)
            return null;
        FillDataStruct(line.Split(' '), _headerFields);
        CurrentFileRecord++;
        return NewEventObj();
    }

    private IISLogEvent NewEventObj() {
        var evt = new IISLogEvent();
        foreach (var kv in _dataStruct) {
            if (!KnownFields.Contains(kv.Key))
                evt.Fields[kv.Key] = kv.Value;
        }
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
        evt.scBytes = GetLong("sc-bytes");
        evt.csBytes = GetLong("cs-bytes");
        evt.timeTaken = GetLong("time-taken");
        return evt;
    }

    private DateTime GetEventDateTime()
        => DateTime.ParseExact($"{GetValue("date")} {GetValue("time")}", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

    private void FillDataStruct(string[] fieldsData, string[] header) {
        _dataStruct.Clear();
        for (int i = 0; i < header.Length; i++)
            _dataStruct[header[i]] = fieldsData[i] == "-" ? null : fieldsData[i];
    }

    private string? GetValue(string key) => _dataStruct.TryGetValue(key, out var v) ? v : null;
    private int? GetInt(string key) => int.TryParse(GetValue(key), out var v) ? v : null;
    private long? GetLong(string key) => long.TryParse(GetValue(key), out var v) ? v : null;

    /// <inheritdoc />
    public void Dispose() { }
}