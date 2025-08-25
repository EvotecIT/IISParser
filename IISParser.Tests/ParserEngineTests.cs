using System;
using System.IO;
using System.Linq;
using System.Globalization;
using Xunit;

namespace IISParser.Tests;

public class ParserEngineTests {
    [Fact]
    public void ParseLog_ReturnsRecords() {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "sample.log");
        var engine = new ParserEngine(path);
        var recordsList = engine.ParseLog().ToList();
        Assert.Single(recordsList);
        var record = recordsList[0];
        Assert.Equal("/index.html", record.UriPath);
        Assert.Equal(200, record.StatusCode);
        Assert.Equal("192.168.0.1", record.Fields["X-Forwarded-For"]);
    }

    [Fact]
    public void ParseLog_SupportsRelativePath() {
        var baseDir = Path.Combine(AppContext.BaseDirectory, "TestData");
        var previous = Environment.CurrentDirectory;
        try {
            Environment.CurrentDirectory = baseDir;
            var engine = new ParserEngine("./sample.log");
            var record = engine.ParseLog().Single();
            Assert.Equal("/index.html", record.UriPath);
        } finally {
            Environment.CurrentDirectory = previous;
        }
    }

    [Fact]
    public void ParseLog_RemovesKnownFieldsFromDictionary() {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "sample.log");
        var engine = new ParserEngine(path);
        var record = engine.ParseLog().Single();
        Assert.False(record.Fields.ContainsKey("cs-uri-stem"));
        Assert.False(record.Fields.ContainsKey("date"));
    }

    [Fact]
    public void ParseLog_HandlesValuesAboveIntMax() {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "large_values.log");
        var engine = new ParserEngine(path);
        var record = engine.ParseLog().Single();
        Assert.Equal(3000000000L, record.BytesSent);
        Assert.Equal(4000000000L, record.BytesReceived);
        Assert.Equal(5000000000L, record.TimeTakenMs);
    }

    [Fact]
    public void ParseLog_ParsesDateTimeUnderDifferentCulture() {
        var originalCulture = CultureInfo.CurrentCulture;
        try {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var path = Path.Combine(AppContext.BaseDirectory, "TestData", "sample.log");
            var engine = new ParserEngine(path);
            var record = engine.ParseLog().Single();
            Assert.Equal(new DateTime(2024, 1, 1, 0, 0, 0), record.Timestamp);
        } finally {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void ParseLog_YieldsEventsLazily() {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "multi.log");
        var engine = new ParserEngine(path);
        using var enumerator = engine.ParseLog().GetEnumerator();
        Assert.True(enumerator.MoveNext());
        Assert.Equal("/index0.html", enumerator.Current.UriPath);
        Assert.Equal(1, engine.CurrentFileRecord);
    }
    
    [Fact]
    public void ParseLog_HandlesShortLogLineGracefully() {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "short_line.log");
        var engine = new ParserEngine(path);
        var record = engine.ParseLog().Single();
        Assert.True(record.Fields.ContainsKey("X-Forwarded-For"));
        Assert.Null(record.Fields["X-Forwarded-For"]);
    }

    [Fact]
    public void ParseLog_MalformedDateTime_ReturnsMinValue() {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "malformed_datetime.log");
        var engine = new ParserEngine(path);
        var record = engine.ParseLog().Single();
        Assert.Equal(DateTime.MinValue, record.Timestamp);
        Assert.Equal("/index.html", record.UriPath);
    }

    [Fact]
    public void ParseLog_MissingDateTime_ReturnsMinValue() {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "missing_datetime.log");
        var engine = new ParserEngine(path);
        var record = engine.ParseLog().Single();
        Assert.Equal(DateTime.MinValue, record.Timestamp);
        Assert.Equal("/index.html", record.UriPath);
    }

    [Fact]
    public void ParseLogLegacy_ReturnsEvents() {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "sample.log");
        var engine = new ParserEngine(path);
        var eventsList = engine.ParseLogLegacy().ToList();
        Assert.Single(eventsList);
        var evt = eventsList[0];
        Assert.Equal("/index.html", evt.csUriStem);
        Assert.Equal(200, evt.scStatus);
    }
}