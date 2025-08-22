using System;
using System.Collections.Generic;

namespace IISParser;

/// <summary>
/// Represents a single entry in an IIS log file.
/// </summary>
public class IISLogEvent {
    /// <summary>
    /// Gets or sets the timestamp of the log entry.
    /// </summary>
    public DateTime DateTimeEvent { get; set; }

    /// <summary>
    /// Gets or sets the site name reported by the server (<c>s-sitename</c>).
    /// </summary>
    public string? sSitename { get; set; }

    /// <summary>
    /// Gets or sets the computer name for the server (<c>s-computername</c>).
    /// </summary>
    public string? sComputername { get; set; }

    /// <summary>
    /// Gets or sets the server IP address (<c>s-ip</c>).
    /// </summary>
    public string? sIp { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method used by the request (<c>cs-method</c>).
    /// </summary>
    public string? csMethod { get; set; }

    /// <summary>
    /// Gets or sets the requested resource path (<c>cs-uri-stem</c>).
    /// </summary>
    public string? csUriStem { get; set; }

    /// <summary>
    /// Gets or sets the query portion of the requested URI (<c>cs-uri-query</c>).
    /// </summary>
    public string? csUriQuery { get; set; }

    /// <summary>
    /// Gets or sets the server port (<c>s-port</c>).
    /// </summary>
    public int? sPort { get; set; }

    /// <summary>
    /// Gets or sets the authenticated user name (<c>cs-username</c>).
    /// </summary>
    public string? csUsername { get; set; }

    /// <summary>
    /// Gets or sets the client IP address (<c>c-ip</c>).
    /// </summary>
    public string? cIp { get; set; }

    /// <summary>
    /// Gets or sets the protocol version (<c>cs-version</c>).
    /// </summary>
    public string? csVersion { get; set; }

    /// <summary>
    /// Gets or sets the user agent string (<c>cs(User-Agent)</c>).
    /// </summary>
    public string? csUserAgent { get; set; }

    /// <summary>
    /// Gets or sets the HTTP cookie value (<c>cs(Cookie)</c>).
    /// </summary>
    public string? csCookie { get; set; }

    /// <summary>
    /// Gets or sets the referrer URL (<c>cs(Referer)</c>).
    /// </summary>
    public string? csReferer { get; set; }

    /// <summary>
    /// Gets or sets the host header value (<c>cs-host</c>).
    /// </summary>
    public string? csHost { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code (<c>sc-status</c>).
    /// </summary>
    public int? scStatus { get; set; }

    /// <summary>
    /// Gets or sets the substatus error code (<c>sc-substatus</c>).
    /// </summary>
    public int? scSubstatus { get; set; }

    /// <summary>
    /// Gets or sets the Windows status code (<c>sc-win32-status</c>).
    /// </summary>
    public long? scWin32Status { get; set; }

    /// <summary>
    /// Gets or sets the number of bytes sent (<c>sc-bytes</c>).
    /// </summary>
    public long? scBytes { get; set; }

    /// <summary>
    /// Gets or sets the number of bytes received (<c>cs-bytes</c>).
    /// </summary>
    public long? csBytes { get; set; }

    /// <summary>
    /// Gets or sets the time taken to service the request, in milliseconds (<c>time-taken</c>).
    /// </summary>
    public long? timeTaken { get; set; }

    /// <summary>
    /// Gets a dictionary containing all fields from the log line keyed by their original names.
    /// </summary>
    public Dictionary<string, string?> Fields { get; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
}