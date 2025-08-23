using System;
using System.Collections.Generic;

namespace IISParser;

/// <summary>
/// Represents a single entry in an IIS log file with user-friendly property names.
/// </summary>
public class IISLogRecord {
    /// <summary>
    /// Gets or sets the timestamp of the log entry.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the site name reported by the server (<c>s-sitename</c>).
    /// </summary>
    public string? SiteName { get; set; }

    /// <summary>
    /// Gets or sets the computer name for the server (<c>s-computername</c>).
    /// </summary>
    public string? ComputerName { get; set; }

    /// <summary>
    /// Gets or sets the server IP address (<c>s-ip</c>).
    /// </summary>
    public string? ServerIp { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method used by the request (<c>cs-method</c>).
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    /// Gets or sets the requested resource path (<c>cs-uri-stem</c>).
    /// </summary>
    public string? UriPath { get; set; }

    /// <summary>
    /// Gets or sets the query portion of the requested URI (<c>cs-uri-query</c>).
    /// </summary>
    public string? UriQuery { get; set; }

    /// <summary>
    /// Gets or sets the server port (<c>s-port</c>).
    /// </summary>
    public int? ServerPort { get; set; }

    /// <summary>
    /// Gets or sets the authenticated user name (<c>cs-username</c>).
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the client IP address (<c>c-ip</c>).
    /// </summary>
    public string? ClientIp { get; set; }

    /// <summary>
    /// Gets or sets the protocol version (<c>cs-version</c>).
    /// </summary>
    public string? HttpVersion { get; set; }

    /// <summary>
    /// Gets or sets the user agent string (<c>cs(User-Agent)</c>).
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the HTTP cookie value (<c>cs(Cookie)</c>).
    /// </summary>
    public string? Cookie { get; set; }

    /// <summary>
    /// Gets or sets the referrer URL (<c>cs(Referer)</c>).
    /// </summary>
    public string? Referer { get; set; }

    /// <summary>
    /// Gets or sets the host header value (<c>cs-host</c>).
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code (<c>sc-status</c>).
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the substatus error code (<c>sc-substatus</c>).
    /// </summary>
    public int? SubStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the Windows status code (<c>sc-win32-status</c>).
    /// </summary>
    public long? Win32Status { get; set; }

    /// <summary>
    /// Gets or sets the number of bytes sent (<c>sc-bytes</c>).
    /// </summary>
    public long? BytesSent { get; set; }

    /// <summary>
    /// Gets or sets the number of bytes received (<c>cs-bytes</c>).
    /// </summary>
    public long? BytesReceived { get; set; }

    /// <summary>
    /// Gets or sets the time taken to service the request, in milliseconds (<c>time-taken</c>).
    /// </summary>
    public long? TimeTakenMs { get; set; }

    /// <summary>
    /// Gets a dictionary containing all fields from the log line keyed by their original names.
    /// </summary>
    public Dictionary<string, string?> Fields { get; } = new(StringComparer.OrdinalIgnoreCase);
}

