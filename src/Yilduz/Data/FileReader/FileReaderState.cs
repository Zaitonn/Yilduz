namespace Yilduz.Data.FileReader;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/readyState
/// </summary>
public enum FileReaderState
{
    /// <summary>
    /// Reader has been created, but none of the read methods have been called yet.
    /// </summary>
    EMPTY = 0,

    /// <summary>
    /// A read method has been called. A <see cref="File.FileInstance"/>  or <see cref="Blob.BlobInstance"/> is being read, and no error has occurred yet.
    /// </summary>
    LOADING = 1,

    /// <summary>
    /// The read operation is complete. This could mean that: the entire <see cref="File.FileInstance"/>  or <see cref="Blob.BlobInstance"/> has been read into memory, a file read error occurred, or <see cref="FileReaderInstance.Abort"/> was called and the read was cancelled.
    /// </summary>
    DONE = 2,
}
