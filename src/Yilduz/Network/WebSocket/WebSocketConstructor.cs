using System;
using System.Collections.Generic;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Extensions;
using Yilduz.Utils;

namespace Yilduz.Network.WebSocket;

/// <summary>
/// https://websockets.spec.whatwg.org/#dom-websocket
/// </summary>
internal sealed class WebSocketConstructor : Constructor
{
    private readonly WebApiIntrinsics _webApiIntrinsics;

    public WebSocketConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(WebSocket))
    {
        _webApiIntrinsics = webApiIntrinsics;
        PrototypeObject = new(engine, this)
        {
            Prototype = webApiIntrinsics.EventTarget.PrototypeObject,
        };

        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));

        // Static ready state constants: https://websockets.spec.whatwg.org/#dom-websocket-connecting
        SetOwnProperty(
            nameof(WebSocketReadyState.Connecting).ToUpperInvariant(),
            new(JsNumber.Create((int)WebSocketReadyState.Connecting), false, false, false)
        );
        SetOwnProperty(
            nameof(WebSocketReadyState.Open).ToUpperInvariant(),
            new(JsNumber.Create((int)WebSocketReadyState.Open), false, false, false)
        );
        SetOwnProperty(
            nameof(WebSocketReadyState.Closing).ToUpperInvariant(),
            new(JsNumber.Create((int)WebSocketReadyState.Closing), false, false, false)
        );
        SetOwnProperty(
            nameof(WebSocketReadyState.Closed).ToUpperInvariant(),
            new(JsNumber.Create((int)WebSocketReadyState.Closed), false, false, false)
        );
    }

    public WebSocketPrototype PrototypeObject { get; }

    /// <summary>
    /// https://websockets.spec.whatwg.org/#dom-websocket-websocket
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCount(Engine, 1, "Failed to construct 'WebSocket'");

        var urlArg = arguments.At(0);
        var protocolsArg = arguments.At(1);

        // Let baseURL be this’s relevant settings object’s API base URL.
        // Let urlRecord be the result of applying the URL parser to url with baseURL.
        var urlRecord = _webApiIntrinsics.URL.TryParse(
            urlArg.ToString(),
            _webApiIntrinsics.Options.BaseUrl
        );

        if (urlRecord == null)
        {
            DOMExceptionHelper
                .CreateSyntaxError(
                    Engine,
                    $"Failed to construct 'WebSocket': The URL '{urlArg}' is not a valid URL."
                )
                .Throw();
        }

        var scheme = urlRecord.Protocol.TrimEnd(':');

        // If urlRecord’s scheme is "http", then set urlRecord’s scheme to "ws".
        // Otherwise, if urlRecord’s scheme is "https", set urlRecord’s scheme to "wss".
        if (
            scheme.Equals("http", StringComparison.OrdinalIgnoreCase)
            || scheme.Equals("https", StringComparison.OrdinalIgnoreCase)
        )
        {
            scheme = scheme.Equals("http", StringComparison.OrdinalIgnoreCase) ? "ws" : "wss";
            urlRecord.Protocol = scheme + ":";
        }

        // If urlRecord’s scheme is not "ws" or "wss", then throw a "SyntaxError" DOMException.
        if (
            !scheme.Equals("ws", StringComparison.OrdinalIgnoreCase)
            && !scheme.Equals("wss", StringComparison.OrdinalIgnoreCase)
        )
        {
            DOMExceptionHelper
                .CreateSyntaxError(
                    Engine,
                    $"Failed to construct 'WebSocket': The URL scheme '{scheme}' is not supported. WebSocket URLs must use the 'ws' or 'wss' scheme."
                )
                .Throw();
        }

        // If urlRecord’s fragment is non-null, then throw a "SyntaxError" DOMException.
        if (!string.IsNullOrEmpty(urlRecord.Hash))
        {
            DOMExceptionHelper
                .CreateSyntaxError(
                    Engine,
                    "Failed to construct 'WebSocket': The URL contains a fragment identifier ('#'). Fragment identifiers are not allowed in WebSocket URLs."
                )
                .Throw();
        }

        // If protocols is a string, set protocols to a sequence consisting of just that string.
        string[] protocols;
        if (protocolsArg.IsUndefined() || protocolsArg.IsNull())
        {
            protocols = [];
        }
        else if (protocolsArg.IsString())
        {
            protocols = [protocolsArg.AsString()];
        }
        else if (protocolsArg.IsArray())
        {
            var array = protocolsArg.AsArray();
            protocols = new string[(int)array.Length];
            for (uint i = 0; i < array.Length; i++)
            {
                protocols[i] = array[i].ToString();
            }
        }
        else
        {
            DOMExceptionHelper
                .CreateSyntaxError(
                    Engine,
                    "Failed to construct 'WebSocket': The 'protocols' argument must be a string or an array of strings."
                )
                .Throw();
            return null;
        }

        // If any of the values in protocols occur more than once or otherwise fail to match the requirements for elements that comprise the value of `Sec-WebSocket-Protocol` fields as defined by The WebSocket protocol, then throw a "SyntaxError" DOMException. [WSP]
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var protocol in protocols)
        {
            if (!seen.Add(protocol))
            {
                DOMExceptionHelper
                    .CreateSyntaxError(
                        Engine,
                        $"Failed to construct 'WebSocket': The subprotocol '{protocol}' is duplicated."
                    )
                    .Throw();
            }
        }

        var serializedUrl = urlRecord.Href;

        // Set this’s url to urlRecord.
        // Let client be this’s relevant settings object.
        // Run this step in parallel:
        //  Establish a WebSocket connection given urlRecord, protocols, and client. [FETCH]

        return new WebSocketInstance(Engine, serializedUrl, protocols, _webApiIntrinsics)
        {
            Prototype = PrototypeObject,
        };
    }
}
