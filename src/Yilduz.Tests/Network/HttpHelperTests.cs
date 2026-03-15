using System;
using Xunit;
using Yilduz.Network;

namespace Yilduz.Tests.Network;

public class HttpHelperTests
{
    [Theory]
    [InlineData("bytes=0-", 0, null)]
    [InlineData("bytes=-500", null, 500)]
    [InlineData("bytes=0-499", 0, 499)]
    [InlineData("bytes=500-999", 500, 999)]
    [InlineData("bytes= -500", null, 500)]
    [InlineData("bytes= 0 -", 0, null)]
    [InlineData("bytes = 0-", 0, null)]
    public void ParseSingleRangeHeaderValue_ShouldParseValidRanges(
        string input,
        int? expectedStart,
        int? expectedEnd
    )
    {
        var (start, end) = HttpHelper.ParseSingleRangeHeaderValue(input, true);

        Assert.Equal(expectedStart, start);
        Assert.Equal(expectedEnd, end);
    }

    [Theory]
    [InlineData("byte=0-499")]
    [InlineData("bytes=0")]
    [InlineData("bytes=0499")]
    [InlineData("bytes=-")]
    [InlineData("bytes=500-100")]
    [InlineData("bytes=1a-2b")]
    public void ParseSingleRangeHeaderValue_ShouldThrowOnInvalidRanges(string input)
    {
        Assert.Throws<InvalidOperationException>(
            () => HttpHelper.ParseSingleRangeHeaderValue(input, true)
        );
    }

    [Theory]
    [InlineData("accept", "text/plain", false)]
    [InlineData("accept-language", "en-US", false)]
    [InlineData("content-language", "en-US", false)]
    [InlineData("content-type", "application/json", false)]
    [InlineData("range", "bytes=0-499", true)]
    [InlineData("range", "bytes=0-", true)]
    [InlineData("range", "bytes=-500", false)]
    [InlineData("range", "invalid-range", false)]
    [InlineData("range", "bytes= 0-", false)]
    [InlineData("authorization", "Bearer token", false)]
    [InlineData("x-custom-header", "value", false)]
    public void IsNoCORSUnsafeRequestHeader_ShouldReturnExpectedResult(
        string name,
        string value,
        bool expected
    )
    {
        var result = HttpHelper.IsNoCORSUnsafeRequestHeader(name, value);
        Assert.Equal(expected, result);
    }
}
