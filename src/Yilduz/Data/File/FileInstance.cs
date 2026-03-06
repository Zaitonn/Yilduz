using System;
using Jint;
using Jint.Native;
using Yilduz.Data.Blob;

namespace Yilduz.Data.File;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/File
/// </summary>
public sealed class FileInstance : BlobInstance
{
    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/File/lastModified
    /// </summary>
    public long LastModified { get; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/File/lastModifiedDate
    /// </summary>
    public DateTime LastModifiedDate =>
        DateTimeOffset.FromUnixTimeMilliseconds(LastModified).DateTime;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/File/name
    /// </summary>
    public string Name { get; }

    internal FileInstance(
        Engine engine,
        WebApiIntrinsics webApiIntrinsics,
        JsValue blobParts,
        JsValue fileName,
        JsValue options
    )
        : base(engine, webApiIntrinsics, blobParts, options)
    {
        Name = fileName.ToString();

        var lastModifiedOption = options.Get("lastModified");
        LastModified = lastModifiedOption.IsNumber()
            ? (long)lastModifiedOption.AsNumber()
            : DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}
