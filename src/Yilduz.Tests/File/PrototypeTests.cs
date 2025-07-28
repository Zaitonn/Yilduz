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
        Engine.Execute(
            """
            const file = new File([], 'test.txt');
            const filePrototype = Object.getPrototypeOf(file);
            const blobPrototype = Object.getPrototypeOf(filePrototype);
            const objectPrototype = Object.getPrototypeOf(blobPrototype);
            """
        );

        Assert.Equal("File", Engine.Evaluate("filePrototype.constructor.name").AsString());
        Assert.Equal("Blob", Engine.Evaluate("blobPrototype.constructor.name").AsString());
        Assert.Equal("Object", Engine.Evaluate("objectPrototype.constructor.name").AsString());
    }

    [Fact]
    public void ShouldHaveLastModifiedGetter()
    {
        Engine.Execute(
            """
            const file = new File([], 'test.txt', { lastModified: 12345 });
            const descriptor = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(file), 'lastModified');
            """
        );

        Assert.True(Engine.Evaluate("typeof descriptor.get === 'function'").AsBoolean());
        Assert.True(Engine.Evaluate("descriptor.get.name === 'get lastModified'").AsBoolean());
        Assert.True(Engine.Evaluate("descriptor.set === undefined").AsBoolean());
        Assert.Equal(12345, Engine.Evaluate("file.lastModified").AsNumber());
    }

    [Fact]
    public void ShouldHaveNameGetter()
    {
        Engine.Execute(
            """
            const file = new File([], 'test.txt');
            const descriptor = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(file), 'name');
            """
        );

        Assert.True(Engine.Evaluate("typeof descriptor.get === 'function'").AsBoolean());
        Assert.True(Engine.Evaluate("descriptor.get.name === 'get name'").AsBoolean());
        Assert.True(Engine.Evaluate("descriptor.set === undefined").AsBoolean());
        Assert.Equal("test.txt", Engine.Evaluate("file.name").AsString());
    }
}
