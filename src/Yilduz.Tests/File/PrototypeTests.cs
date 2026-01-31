using Jint;
using Xunit;

namespace Yilduz.Tests.File;

public sealed class PrototypeTests : TestBase
{
    [Fact]
    public void ShouldHaveToStringTag()
    {
        Assert.Equal(
            "File",
            Engine
                .Evaluate("Object.prototype.toString.call(new File([], 'test.txt'))")
                .AsString()
                .Replace("[object ", "")
                .Replace("]", "")
        );
    }

    [Fact]
    public void ShouldHaveCorrectPrototypeChain()
    {
        Execute(
            """
            const file = new File([], 'test.txt');
            const filePrototype = Object.getPrototypeOf(file);
            const blobPrototype = Object.getPrototypeOf(filePrototype);
            const objectPrototype = Object.getPrototypeOf(blobPrototype);
            """
        );

        Assert.Equal("File", Evaluate("filePrototype.constructor.name").AsString());
        Assert.Equal("Blob", Evaluate("blobPrototype.constructor.name").AsString());
        Assert.Equal("Object", Evaluate("objectPrototype.constructor.name").AsString());
    }

    [Fact]
    public void ShouldHaveLastModifiedGetter()
    {
        Execute(
            """
            const file = new File([], 'test.txt', { lastModified: 12345 });
            const descriptor = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(file), 'lastModified');
            """
        );

        Assert.True(Evaluate("typeof descriptor.get === 'function'").AsBoolean());
        Assert.True(Evaluate("descriptor.get.name === 'get lastModified'").AsBoolean());
        Assert.True(Evaluate("descriptor.set === undefined").AsBoolean());
        Assert.Equal(12345, Evaluate("file.lastModified").AsNumber());
    }

    [Fact]
    public void ShouldHaveNameGetter()
    {
        Execute(
            """
            const file = new File([], 'test.txt');
            const descriptor = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(file), 'name');
            """
        );

        Assert.True(Evaluate("typeof descriptor.get === 'function'").AsBoolean());
        Assert.True(Evaluate("descriptor.get.name === 'get name'").AsBoolean());
        Assert.True(Evaluate("descriptor.set === undefined").AsBoolean());
        Assert.Equal("test.txt", Evaluate("file.name").AsString());
    }
}
