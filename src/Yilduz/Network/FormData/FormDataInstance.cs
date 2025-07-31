using System.Collections.Generic;
using System.Linq;
using Jint;
using Jint.Native.Object;
using Yilduz.Files.Blob;
using Entry = (string Name, Jint.Native.JsValue Value, string? FileName);

namespace Yilduz.Network.FormData;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/FormData
/// </summary>
public sealed class FormDataInstance : ObjectInstance
{
    public List<Entry> EntryList { get; } = [];

    internal FormDataInstance(Engine engine)
        : base(engine) { }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FormData/append
    /// </summary>
    public void Append(string name, string value)
    {
        EntryList.Add((name, value, null));
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FormData/append
    /// </summary>
    public void Append(string name, BlobInstance blobValue, string fileName = "")
    {
        EntryList.Add((name, blobValue, fileName));
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FormData/get
    /// </summary>
    public Entry? Get(string name)
    {
        foreach (var entry in EntryList)
        {
            if (entry.Name == name)
            {
                return entry;
            }
        }

        return null;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FormData/getAll
    /// </summary>
    public IEnumerable<Entry> GetAll(string name)
    {
        return EntryList.Where((entry) => entry.Name == name);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FormData/has
    /// </summary>
    public bool Has(string name)
    {
        return EntryList.Any((entry) => entry.Name == name);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FormData/delete
    /// </summary>
    public void Delete(string name)
    {
        EntryList.RemoveAll((entry) => entry.Name == name);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FormData/set
    /// </summary>
    public void Set(string name, string value)
    {
        Delete(name);
        Append(name, value);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/FormData/set
    /// </summary>
    public void Set(string name, BlobInstance blobValue, string fileName = "")
    {
        Delete(name);
        Append(name, blobValue, fileName);
    }
}
