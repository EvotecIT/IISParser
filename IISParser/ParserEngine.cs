using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace IISParser;

/// <summary>
/// Parses IIS log files and exposes the entries as <see cref="IISLogRecord"/> objects.
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
    /// The default value (<see cref="int.MaxValue"/>) means no limit is applied.
    /// </summary>
    public int MaxFileRecord2Read { get; set; } = int.MaxValue;

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
    /// Parses the log file and returns the entries as an enumerable sequence of <see cref="IISLogRecord"/>.
    /// </summary>
    /// <returns>A sequence of <see cref="IISLogRecord"/> instances.</returns>
    public IEnumerable<IISLogRecord> ParseLog() => _mbSize < 50 ? QuickProcess(NewRecordObj) : LongProcess(NewRecordObj);

    /// <summary>
    /// Parses the log file and returns legacy <see cref="IISLogEvent"/> instances.
    /// </summary>
    /// <returns>A sequence of <see cref="IISLogEvent"/> instances.</returns>
    public IEnumerable<IISLogEvent> ParseLogLegacy() => _mbSize < 50 ? QuickProcess(NewEventObj) : LongProcess(NewEventObj);

    private IEnumerable<T> QuickProcess<T>(Func<T> factory) {
        MissingRecords = false;
        var lines = Utils.ReadAllLines(FilePath);
        var maxRecords = MaxFileRecord2Read <= 0 ? int.MaxValue : MaxFileRecord2Read;

        for (var i = 0; i < lines.Count; i++) {
            var obj = ProcessLine(lines[i], factory);
            if (obj != null) {
                if (CurrentFileRecord % maxRecords == 0 && i < lines.Count - 1) {
                    MissingRecords = true;
                    yield return obj;
                    yield break;
                }

                yield return obj;
            }
        }
    }

    private IEnumerable<T> LongProcess<T>(Func<T> factory) {
        MissingRecords = false;
        using var fileStream = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(fileStream);
        var maxRecords = MaxFileRecord2Read <= 0 ? int.MaxValue : MaxFileRecord2Read;
        while (reader.Peek() > -1) {
            var obj = ProcessLine(reader.ReadLine() ?? string.Empty, factory);
            if (obj != null) {
                if (CurrentFileRecord % maxRecords == 0 && reader.Peek() > -1) {
                    MissingRecords = true;
                    yield return obj;
                    yield break;
                }
                yield return obj;
            }
        }
    }

    private T? ProcessLine<T>(string line, Func<T> factory) {
        if (line.StartsWith("#Fields:", StringComparison.OrdinalIgnoreCase))
            _headerFields = line.Replace("#Fields: ", string.Empty).Split(' ');
        if (line.StartsWith("#", StringComparison.OrdinalIgnoreCase) || _headerFields == null)
            return default;
        FillDataStruct(line.Split(' '), _headerFields);
        CurrentFileRecord++;
        return factory();
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

    private IISLogRecord NewRecordObj() {
        var record = new IISLogRecord();
        foreach (var kv in _dataStruct) {
            if (!KnownFields.Contains(kv.Key))
                record.Fields[kv.Key] = kv.Value;
        }
        record.Timestamp = GetEventDateTime();
        record.SiteName = GetValue("s-sitename");
        record.ComputerName = GetValue("s-computername");
        record.ServerIp = GetValue("s-ip");
        record.HttpMethod = GetValue("cs-method");
        record.UriPath = GetValue("cs-uri-stem");
        record.UriQuery = GetValue("cs-uri-query");
        record.ServerPort = GetInt("s-port");
        record.Username = GetValue("cs-username");
        record.ClientIp = GetValue("c-ip");
        record.HttpVersion = GetValue("cs-version");
        record.UserAgent = GetValue("cs(User-Agent)");
        record.Cookie = GetValue("cs(Cookie)");
        record.Referer = GetValue("cs(Referer)");
        record.Host = GetValue("cs-host");
        record.StatusCode = GetInt("sc-status");
        record.SubStatusCode = GetInt("sc-substatus");
        record.Win32Status = GetLong("sc-win32-status");
        record.BytesSent = GetLong("sc-bytes");
        record.BytesReceived = GetLong("cs-bytes");
        record.TimeTakenMs = GetLong("time-taken");
        return record;
    }

    private DateTime GetEventDateTime() {
        var date = GetValue("date");
        var time = GetValue("time");
        if (date == null || time == null)
            return DateTime.MinValue;
        return DateTime.TryParseExact($"{date} {time}", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
            ? result
            : DateTime.MinValue;
    }

    private void FillDataStruct(string[] fieldsData, string[] header) {
        _dataStruct.Clear();
        for (int i = 0; i < header.Length; i++) {
            if (i < fieldsData.Length) {
                _dataStruct[header[i]] = fieldsData[i] == "-" ? null : fieldsData[i];
            } else {
                _dataStruct[header[i]] = null;
            }
        }
    }

    private string? GetValue(string key) => _dataStruct.TryGetValue(key, out var v) ? v : null;
    private int? GetInt(string key) => int.TryParse(GetValue(key), out var v) ? v : null;
    private long? GetLong(string key) => long.TryParse(GetValue(key), out var v) ? v : null;

    /// <inheritdoc />
    public void Dispose() { }
}