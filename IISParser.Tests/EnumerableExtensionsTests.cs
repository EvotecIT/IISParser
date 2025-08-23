using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace IISParser.Tests;

public class EnumerableExtensionsTests {
    [Fact]
    public void TakeLastLazy_ReturnsLastElements() {
        IEnumerable<int> source = Enumerable.Range(0, 100);
        var result = source.TakeLastLazy(5).ToArray();
        Assert.Equal(new[] {95, 96, 97, 98, 99}, result);
    }

    [Fact]
    public void SkipLastLazy_SkipsTailElements() {
        IEnumerable<int> source = Enumerable.Range(0, 100);
        var result = source.SkipLastLazy(10).ToArray();
        Assert.Equal(90, result.Length);
        Assert.Equal(89, result[result.Length - 1]);
    }
}

