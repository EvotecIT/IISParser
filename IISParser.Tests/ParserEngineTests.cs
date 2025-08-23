using System;
using System.IO;
using System.Linq;
using System.Globalization;
using Xunit;

namespace IISParser.Tests;

public class ParserEngineTests {
    [Fact]
    public void ParseLog_ReturnsEvents() {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "sample.log");
        var engine = new ParserEngine(path);
        var eventsList = engine.ParseLog().ToList();
        Assert.Single(eventsList);
        var evt = eventsList[0];
        Assert.Equal("/index.html", evt.csUriStem);
        Assert.Equal(200, evt.scStatus);
        Assert.Equal("192.168.0.1", evt.Fields["X-Forwarded-For"]);
    }

    [Fact]
    public void ParseLog_RemovesKnownFieldsFromDictionary() {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "sample.log");
        var engine = new ParserEngine(path);
        var evt = engine.ParseLog().Single();
        Assert.False(evt.Fields.ContainsKey("cs-uri-stem"));
        Assert.False(evt.Fields.ContainsKey("date"));
    }

    [Fact]
    public void ParseLog_HandlesValuesAboveIntMax() {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "large_values.log");
        var engine = new ParserEngine(path);
        var evt = engine.ParseLog().Single();
        Assert.Equal(3000000000L, evt.scBytes);
        Assert.Equal(4000000000L, evt.csBytes);
        Assert.Equal(5000000000L, evt.timeTaken);
    }

    [Fact]
    public void ParseLog_ParsesDateTimeUnderDifferentCulture() {
        var originalCulture = CultureInfo.CurrentCulture;
        try {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var path = Path.Combine(AppContext.BaseDirectory, "TestData", "sample.log");
            var engine = new ParserEngine(path);
            var evt = engine.ParseLog().Single();
            Assert.Equal(new DateTime(2024, 1, 1, 0, 0, 0), evt.DateTimeEvent);
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
        Assert.Equal("/index0.html", enumerator.Current.csUriStem);
        Assert.Equal(1, engine.CurrentFileRecord);
    }
}