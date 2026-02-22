using System.Collections.Generic;
using Jint;
using Jint.Native.Object;

namespace Yilduz.DOM.DOMException;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/DOMException
/// </summary>
public sealed class DOMExceptionInstance : ObjectInstance
{
    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/DOMException/name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/DOMException/message
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/DOMException/code
    /// </summary>
    public int Code { get; }

    internal DOMExceptionInstance(Engine engine, string message = "", string name = "Error")
        : base(engine)
    {
        Name = name;
        Message = message;

#if NETSTANDARD
        Code = ErrorCodes.Codes.TryGetValue(name, out var code) ? code : 0;
#else
        Code = ErrorCodes.Codes.GetValueOrDefault(name);
#endif
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override string ToString()
    {
        return $"{Name}: {Message}";
    }
}
