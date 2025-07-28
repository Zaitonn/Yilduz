using System;
using Jint;
using Jint.Native;
using Yilduz.Files.Blob;

namespace Yilduz.Files.File;

public sealed class FileInstance : BlobInstance
{
    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/File/lastModified
    /// </summary>
    public long LastModified { get; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/File/name
    /// </summary>
    public string Name { get; }

    internal FileInstance(Engine engine, JsValue blobParts, JsValue fileName, JsValue options)
        : base(engine, blobParts, options)
    {
        Name = fileName.ToString();

        var lastModifiedOption = options.Get("lastModified");
        LastModified = lastModifiedOption.IsNumber()
            ? (long)lastModifiedOption.AsNumber()
            : DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}
