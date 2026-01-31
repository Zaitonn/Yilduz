using Jint;
using Xunit;

namespace Yilduz.Tests.URLSearchParams;

public sealed class IteratorTests : TestBase
{
    [Fact]
    public void EntriesMethodShouldReturnIterator()
    {
        Execute(
            """
            const url = new URL('https://example.com?a=1&b=2&c=3');
            const entries = url.searchParams.entries();
            const firstEntry = entries.next();
            const secondEntry = entries.next();
            const thirdEntry = entries.next();
            const fourthEntry = entries.next();
            """
        );

        var firstEntryDone = Evaluate("firstEntry.done").AsBoolean();
        var firstEntryKey = Evaluate("firstEntry.value[0]").AsString();
        var firstEntryValue = Evaluate("firstEntry.value[1]").AsString();

        var secondEntryDone = Evaluate("secondEntry.done").AsBoolean();
        var secondEntryKey = Evaluate("secondEntry.value[0]").AsString();
        var secondEntryValue = Evaluate("secondEntry.value[1]").AsString();

        var thirdEntryDone = Evaluate("thirdEntry.done").AsBoolean();
        var thirdEntryKey = Evaluate("thirdEntry.value[0]").AsString();
        var thirdEntryValue = Evaluate("thirdEntry.value[1]").AsString();

        var fourthEntryDone = Evaluate("fourthEntry.done").AsBoolean();

        Assert.False(firstEntryDone);
        Assert.Equal("a", firstEntryKey);
        Assert.Equal("1", firstEntryValue);

        Assert.False(secondEntryDone);
        Assert.Equal("b", secondEntryKey);
        Assert.Equal("2", secondEntryValue);

        Assert.False(thirdEntryDone);
        Assert.Equal("c", thirdEntryKey);
        Assert.Equal("3", thirdEntryValue);

        Assert.True(fourthEntryDone);
    }

    [Fact]
    public void KeysMethodShouldReturnIterator()
    {
        Execute(
            """
            const url = new URL('https://example.com?a=1&b=2&c=3');
            const keys = url.searchParams.keys();
            const firstKey = keys.next();
            const secondKey = keys.next();
            const thirdKey = keys.next();
            const fourthKey = keys.next();
            """
        );

        var firstKeyDone = Evaluate("firstKey.done").AsBoolean();
        var firstKeyValue = Evaluate("firstKey.value").AsString();

        var secondKeyDone = Evaluate("secondKey.done").AsBoolean();
        var secondKeyValue = Evaluate("secondKey.value").AsString();

        var thirdKeyDone = Evaluate("thirdKey.done").AsBoolean();
        var thirdKeyValue = Evaluate("thirdKey.value").AsString();

        var fourthKeyDone = Evaluate("fourthKey.done").AsBoolean();

        Assert.False(firstKeyDone);
        Assert.Equal("a", firstKeyValue);

        Assert.False(secondKeyDone);
        Assert.Equal("b", secondKeyValue);

        Assert.False(thirdKeyDone);
        Assert.Equal("c", thirdKeyValue);

        Assert.True(fourthKeyDone);
    }

    [Fact]
    public void ValuesMethodShouldReturnIterator()
    {
        Execute(
            """
            const url = new URL('https://example.com?a=1&b=2&c=3');
            const values = url.searchParams.values();
            const firstValue = values.next();
            const secondValue = values.next();
            const thirdValue = values.next();
            const fourthValue = values.next();
            """
        );

        var firstValueDone = Evaluate("firstValue.done").AsBoolean();
        var firstValueValue = Evaluate("firstValue.value").AsString();

        var secondValueDone = Evaluate("secondValue.done").AsBoolean();
        var secondValueValue = Evaluate("secondValue.value").AsString();

        var thirdValueDone = Evaluate("thirdValue.done").AsBoolean();
        var thirdValueValue = Evaluate("thirdValue.value").AsString();

        var fourthValueDone = Evaluate("fourthValue.done").AsBoolean();

        Assert.False(firstValueDone);
        Assert.Equal("1", firstValueValue);

        Assert.False(secondValueDone);
        Assert.Equal("2", secondValueValue);

        Assert.False(thirdValueDone);
        Assert.Equal("3", thirdValueValue);

        Assert.True(fourthValueDone);
    }

    [Fact]
    public void ForOfLoopShouldWorkWithEntries()
    {
        Execute(
            """
            const url = new URL('https://example.com?a=1&b=2&c=3');
            const results = [];

            for (const entry of url.searchParams.entries()) {
                results.push({ key: entry[0], value: entry[1] });
            }
            """
        );

        var resultsLength = Evaluate("results.length").AsNumber();
        var firstEntryKey = Evaluate("results[0].key").AsString();
        var firstEntryValue = Evaluate("results[0].value").AsString();
        var secondEntryKey = Evaluate("results[1].key").AsString();
        var secondEntryValue = Evaluate("results[1].value").AsString();
        var thirdEntryKey = Evaluate("results[2].key").AsString();
        var thirdEntryValue = Evaluate("results[2].value").AsString();

        Assert.Equal(3, resultsLength);
        Assert.Equal("a", firstEntryKey);
        Assert.Equal("1", firstEntryValue);
        Assert.Equal("b", secondEntryKey);
        Assert.Equal("2", secondEntryValue);
        Assert.Equal("c", thirdEntryKey);
        Assert.Equal("3", thirdEntryValue);
    }

    [Fact]
    public void ForOfLoopShouldWorkWithKeys()
    {
        Execute(
            """
            const url = new URL('https://example.com?a=1&b=2&c=3');
            const keys = [];

            for (const key of url.searchParams.keys()) {
                keys.push(key);
            }
            """
        );

        var keysLength = Evaluate("keys.length").AsNumber();
        var firstKey = Evaluate("keys[0]").AsString();
        var secondKey = Evaluate("keys[1]").AsString();
        var thirdKey = Evaluate("keys[2]").AsString();

        Assert.Equal(3, keysLength);
        Assert.Equal("a", firstKey);
        Assert.Equal("b", secondKey);
        Assert.Equal("c", thirdKey);
    }

    [Fact]
    public void ForOfLoopShouldWorkWithValues()
    {
        Execute(
            """
            const url = new URL('https://example.com?a=1&b=2&c=3');
            const values = [];

            for (const value of url.searchParams.values()) {
                values.push(value);
            }
            """
        );

        var valuesLength = Evaluate("values.length").AsNumber();
        var firstValue = Evaluate("values[0]").AsString();
        var secondValue = Evaluate("values[1]").AsString();
        var thirdValue = Evaluate("values[2]").AsString();

        Assert.Equal(3, valuesLength);
        Assert.Equal("1", firstValue);
        Assert.Equal("2", secondValue);
        Assert.Equal("3", thirdValue);
    }

    [Fact]
    public void DefaultIteratorShouldBehaveAsEntries()
    {
        Execute(
            """
            const url = new URL('https://example.com?a=1&b=2&c=3');
            const pairs = [];

            for (const pair of url.searchParams) {
                pairs.push({ key: pair[0], value: pair[1] });
            }
            """
        );

        var pairsLength = Evaluate("pairs.length").AsNumber();
        var firstPairKey = Evaluate("pairs[0].key").AsString();
        var firstPairValue = Evaluate("pairs[0].value").AsString();
        var secondPairKey = Evaluate("pairs[1].key").AsString();
        var secondPairValue = Evaluate("pairs[1].value").AsString();
        var thirdPairKey = Evaluate("pairs[2].key").AsString();
        var thirdPairValue = Evaluate("pairs[2].value").AsString();

        Assert.Equal(3, pairsLength);
        Assert.Equal("a", firstPairKey);
        Assert.Equal("1", firstPairValue);
        Assert.Equal("b", secondPairKey);
        Assert.Equal("2", secondPairValue);
        Assert.Equal("c", thirdPairKey);
        Assert.Equal("3", thirdPairValue);
    }

    [Fact]
    public void SymbolIteratorShouldReturnSelf()
    {
        Execute(
            """
            const url = new URL('https://example.com?a=1&b=2&c=3');
            const entries = url.searchParams.entries();

            entries.next();

            const resetEntries = entries[Symbol.iterator]();
            const firstEntry = resetEntries.next();
            """
        );

        var firstEntryDone = Evaluate("firstEntry.done").AsBoolean();
        var firstEntryKey = Evaluate("firstEntry.value[0]").AsString();
        var firstEntryValue = Evaluate("firstEntry.value[1]").AsString();

        Assert.False(firstEntryDone);
        Assert.Equal("b", firstEntryKey);
        Assert.Equal("2", firstEntryValue);
    }

    [Fact]
    public void SpreadSyntaxShouldWorkWithEntries()
    {
        Execute(
            """
            const url = new URL('https://example.com?a=1&b=2&c=3');
            const entries = [...url.searchParams.entries()];
            """
        );

        var entriesLength = Evaluate("entries.length").AsNumber();
        var firstEntryKey = Evaluate("entries[0][0]").AsString();
        var firstEntryValue = Evaluate("entries[0][1]").AsString();
        var lastEntryKey = Evaluate("entries[2][0]").AsString();
        var lastEntryValue = Evaluate("entries[2][1]").AsString();

        Assert.Equal(3, entriesLength);
        Assert.Equal("a", firstEntryKey);
        Assert.Equal("1", firstEntryValue);
        Assert.Equal("c", lastEntryKey);
        Assert.Equal("3", lastEntryValue);
    }

    [Fact]
    public void SpreadSyntaxShouldWorkWithKeysAndValues()
    {
        Execute(
            """
            const url = new URL('https://example.com?a=1&b=2&c=3');
            const keys = [...url.searchParams.keys()];
            const values = [...url.searchParams.values()];
            """
        );

        var keysLength = Evaluate("keys.length").AsNumber();
        var valuesLength = Evaluate("values.length").AsNumber();
        var firstKey = Evaluate("keys[0]").AsString();
        var firstValue = Evaluate("values[0]").AsString();
        var lastKey = Evaluate("keys[2]").AsString();
        var lastValue = Evaluate("values[2]").AsString();

        Assert.Equal(3, keysLength);
        Assert.Equal(3, valuesLength);
        Assert.Equal("a", firstKey);
        Assert.Equal("1", firstValue);
        Assert.Equal("c", lastKey);
        Assert.Equal("3", lastValue);
    }

    [Fact]
    public void ModifyingCollectionDuringIteration()
    {
        Execute(
            """
            const url = new URL('https://example.com?a=1&b=2&c=3');
            const entries = url.searchParams.entries();

            const firstEntry = entries.next();

            url.searchParams.delete('b');
            url.searchParams.append('d', '4');

            const secondEntry = entries.next();
            const thirdEntry = entries.next();
            const fourthEntry = entries.next();
            """
        );

        var firstEntryKey = Evaluate("firstEntry.value[0]").AsString();
        var secondEntryKey = Evaluate("secondEntry.value[0]").AsString();
        var thirdEntryDone = Evaluate("thirdEntry.done").AsBoolean();
        var fourthEntryDone = Evaluate("fourthEntry.done").AsBoolean();

        Assert.Equal("a", firstEntryKey);
        Assert.Equal("c", secondEntryKey);
        Assert.False(thirdEntryDone);
        Assert.True(fourthEntryDone);
    }
}
