using System;
using Jint;
using Xunit;
using Yilduz.Utils;

namespace Yilduz.Tests.TextDecoder;

public sealed class EncodingHelperTests : TestBase
{
    [Theory]
    // UTF-8 variants
    [InlineData("utf-8")]
    [InlineData("utf8")]
    [InlineData("unicode-1-1-utf-8")]
    [InlineData("UTF-8")]
    [InlineData("UTF8")]
    // IBM866 variants
    [InlineData("866")]
    [InlineData("cp866")]
    [InlineData("csibm866")]
    [InlineData("ibm866")]
    [InlineData("IBM866")]
    [InlineData("CP866")]
    // ISO-8859-2 variants
    [InlineData("iso-8859-2")]
    [InlineData("iso8859-2")]
    [InlineData("iso88592")]
    [InlineData("iso_8859-2")]
    [InlineData("iso_8859-2:1987")]
    [InlineData("latin2")]
    [InlineData("l2")]
    [InlineData("csisolatin2")]
    [InlineData("iso-ir-101")]
    // Windows-1252 variants
    [InlineData("windows-1252")]
    [InlineData("cp1252")]
    [InlineData("x-cp1252")]
    [InlineData("ansi_x3.4-1968")]
    [InlineData("ascii")]
    [InlineData("cp819")]
    [InlineData("csisolatin1")]
    [InlineData("ibm819")]
    [InlineData("iso-8859-1")]
    [InlineData("iso-ir-100")]
    [InlineData("iso8859-1")]
    [InlineData("iso88591")]
    [InlineData("iso_8859-1")]
    [InlineData("iso_8859-1:1987")]
    [InlineData("l1")]
    [InlineData("latin1")]
    [InlineData("us-ascii")]
    // UTF-16LE variants
    [InlineData("utf-16")]
    [InlineData("utf-16le")]
    [InlineData("UTF-16")]
    [InlineData("UTF-16LE")]
    // UTF-16BE variants
    [InlineData("utf-16be")]
    [InlineData("UTF-16BE")]
    // Shift-JIS variants
    [InlineData("shift-jis")]
    [InlineData("shift_jis")]
    [InlineData("sjis")]
    [InlineData("csshiftjis")]
    [InlineData("ms_kanji")]
    [InlineData("windows-31j")]
    [InlineData("x-sjis")]
    // EUC-JP variants
    [InlineData("euc-jp")]
    [InlineData("cseucpkdfmtjapanese")]
    [InlineData("x-euc-jp")]
    // EUC-KR variants
    [InlineData("euc-kr")]
    [InlineData("cseuckr")]
    [InlineData("csksc56011987")]
    [InlineData("iso-ir-149")]
    [InlineData("korean")]
    [InlineData("ks_c_5601-1987")]
    [InlineData("ks_c_5601-1989")]
    [InlineData("ksc5601")]
    [InlineData("ksc_5601")]
    [InlineData("windows-949")]
    // GBK variants
    [InlineData("gbk")]
    [InlineData("chinese")]
    [InlineData("csgb2312")]
    [InlineData("csiso58gb231280")]
    [InlineData("gb2312")]
    [InlineData("gb_2312")]
    [InlineData("gb_2312-80")]
    [InlineData("iso-ir-58")]
    [InlineData("x-gbk")]
    // Big5 variants
    [InlineData("big5")]
    [InlineData("big5-hkscs")]
    [InlineData("cn-big5")]
    [InlineData("csbig5")]
    [InlineData("x-x-big5")]
    // Additional encodings
    [InlineData("gb18030")]
    [InlineData("hz-gb-2312")]
    [InlineData("iso-2022-jp")]
    [InlineData("csiso2022jp")]
    [InlineData("iso-2022-kr")]
    [InlineData("csiso2022kr")]
    public void ShouldNormalizeEncodingName(string input)
    {
        // Arrange & Act
        var normalizedName = EncodingHelper.NormalizeEncodingName(input);

        // Assert - Create TextDecoder with the input and verify the encoding matches our normalization
        Execute($"const decoder = new TextDecoder('{input}');");
        var actualEncoding = Evaluate("decoder.encoding").AsString();

        Assert.Equal(normalizedName, actualEncoding);
    }

    [Fact]
    public void ShouldThrowOnInvalidEncoding()
    {
        // Arrange
        const string invalidEncoding = "invalid-encoding-name";

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => EncodingHelper.NormalizeEncodingName(invalidEncoding)
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ShouldThrowOnEmptyOrWhitespaceInput(string input)
    {
        // Act & Assert
        Assert.ThrowsAny<Exception>(() => EncodingHelper.NormalizeEncodingName(input));
    }
}
