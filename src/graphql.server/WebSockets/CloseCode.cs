using System.Net.WebSockets;

namespace Tanka.GraphQL.Server.WebSockets;

/// <summary>
///     Custom close codes from
///     https://github.com/enisdenjo/graphql-ws/blob/2e6eb138b47bf30220c8048f1ba10f0782ded589/src/common.ts#L29
/// </summary>
public static class CloseCode
{
    public const WebSocketCloseStatus InternalServerError = (WebSocketCloseStatus)4500;
    public const WebSocketCloseStatus InternalClientError = (WebSocketCloseStatus)4005;
    public const WebSocketCloseStatus BadRequest = (WebSocketCloseStatus)4400;
    public const WebSocketCloseStatus BadResponse = (WebSocketCloseStatus)4004;

    /** Tried subscribing before connect ack */
    public const WebSocketCloseStatus Unauthorized = (WebSocketCloseStatus)4401;
    public const WebSocketCloseStatus Forbidden = (WebSocketCloseStatus)4403;
    public const WebSocketCloseStatus SubprotocolNotAcceptable = (WebSocketCloseStatus)4406;
    public const WebSocketCloseStatus ConnectionInitialisationTimeout = (WebSocketCloseStatus)4408;
    public const WebSocketCloseStatus ConnectionAcknowledgementTimeout = (WebSocketCloseStatus)4504;

    /** Subscriber distinction is very important */
    public const WebSocketCloseStatus SubscriberAlreadyExists = (WebSocketCloseStatus)4409;
    public const WebSocketCloseStatus TooManyInitialisationRequests = (WebSocketCloseStatus)4429;
}