namespace Yilduz.Data.FileReader;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/readyState
/// </summary>
public enum FileReaderReadyState
{
    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/readyState#filereader.empty
    /// </summary>
    EMPTY = 0,

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/readyState#filereader.loading
    /// </summary>
    LOADING = 1,

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FileReader/readyState#filereader.done
    /// </summary>
    DONE = 2,
}
