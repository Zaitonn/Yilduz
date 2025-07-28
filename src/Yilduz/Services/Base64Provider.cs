using System;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Extensions;
using Yilduz.Utils;
using SystemEncoding = System.Text.Encoding;

namespace Yilduz.Services;

internal sealed class Base64Provider(Engine engine)
{
    private readonly Engine _engine = engine;

    public JsValue Encode(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(_engine, 1, "Failed to execute 'btoa'");

        var input = arguments[0].ToString();
        CheckRange(input);

        var bytes = SystemEncoding.ASCII.GetBytes(input);
        return Convert.ToBase64String(bytes);
    }

    public JsValue Decode(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(_engine, 1, "Failed to execute 'atob'");
        var input = arguments[0].ToString();

        try
        {
            return SystemEncoding.ASCII.GetString(Convert.FromBase64String(input));
        }
        catch (FormatException)
        {
            throw new JavaScriptException(
                ErrorHelper.Create(
                    _engine,
                    "InvalidCharacterError",
                    "The string to be decoded is not correctly encoded."
                )
            );
        }
    }

    private void CheckRange(string s)
    {
        foreach (var c in s)
        {
            if (c > (char)0xff)
            {
                throw new JavaScriptException(
                    ErrorHelper.Create(
                        _engine,
                        "InvalidCharacterError",
                        "The string to be encoded contains characters outside of the Latin1 range."
                    )
                );
            }
        }
    }
}
