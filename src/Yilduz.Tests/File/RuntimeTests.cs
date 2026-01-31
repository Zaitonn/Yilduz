using System;
using Jint;
using Xunit;

namespace Yilduz.Tests.File;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldBeGlobalConstructor()
    {
        Assert.True(Evaluate("typeof File === 'function'").AsBoolean());
        Assert.True(Evaluate("File.prototype").IsObject());
    }

    [Fact]
    public void ShouldCreateFileWithCorrectProperties()
    {
        Execute(
            """
            const file = new File(['Hello, World!'], 'test.txt', { type: 'text/plain' });
            """
        );

        Assert.True(Evaluate("file instanceof File").AsBoolean());
        Assert.True(Evaluate("file instanceof Blob").AsBoolean());
        Assert.Equal("test.txt", Evaluate("file.name").AsString());
        Assert.Equal("text/plain", Evaluate("file.type").AsString());
        Assert.Equal(13, Evaluate("file.size").AsNumber());
    }

    [Fact]
    public void ShouldSetLastModifiedFromOptions()
    {
        var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        Execute(
            $$"""
            const timestamp = {{timestamp}};
            const file = new File(['Content'], 'file.txt', { lastModified: timestamp });
            """
        );

        Assert.Equal(timestamp, Evaluate("file.lastModified").AsNumber());
    }

    [Fact]
    public void ShouldSetDefaultLastModifiedWhenNotProvided()
    {
        Execute(
            """
            const now = Date.now();
            const file = new File(['Content'], 'file.txt');
            const isRecent = Math.abs(file.lastModified - now) < 1000; // within 1 second
            """
        );

        Assert.True(Evaluate("isRecent").AsBoolean());
    }

    [Fact]
    public void ShouldHaveReadOnlyProperties()
    {
        Execute(
            """
            const file = new File(['test'], 'test.txt', { type: 'text/plain', lastModified: 12345 });
            const originalName = file.name;
            const originalLastModified = file.lastModified;

            file.name = 'changed.txt';
            file.lastModified = 67890;
            """
        );

        Assert.Equal("test.txt", Evaluate("file.name").AsString());
        Assert.Equal(12345, Evaluate("file.lastModified").AsNumber());
    }

    [Fact]
    public void ShouldInheritBlobMethods()
    {
        Execute(
            """
            const file = new File(['Hello, World!'], 'test.txt');
            const slice = file.slice(0, 5);
            """
        );

        Assert.True(Evaluate("slice instanceof Blob").AsBoolean());
        Assert.Equal(5, Evaluate("slice.size").AsNumber());
    }
}
