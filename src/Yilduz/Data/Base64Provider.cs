using System;
using System.Linq;
using System.Text;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Errors;
using Yilduz.Utils;

namespace Yilduz.Data;

internal sealed class Base64Provider(Engine engine)
{
    private readonly Engine _engine = engine;

    public JsValue Encode(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(_engine, 1, "Failed to execute 'btoa'");

        var input = arguments[0].ToString();
        CheckRange(input);

        var bytes = Encoding.ASCII.GetBytes(input);
        return Convert.ToBase64String(bytes);
    }

    public JsValue Decode(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(_engine, 1, "Failed to execute 'atob'");
        var input = arguments[0].ToString();

        try
        {
            return Encoding.ASCII.GetString(Convert.FromBase64String(input));
        }
        catch (FormatException)
        {
            throw new JavaScriptException(
                ErrorHelper.CreateError(
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
                    ErrorHelper.CreateError(
                        _engine,
                        "InvalidCharacterError",
                        "The string to be encoded contains characters outside of the Latin1 range."
                    )
                );
            }
        }
    }
}
