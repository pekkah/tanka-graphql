namespace Tanka.GraphQL.Server.WebSockets;

public class MessageContext(
    WebSocketChannel channel,
    MessageBase contextMessage,
    GraphQLRequestDelegate requestPipeline) : IMessageContext
{
    public async Task Write<T>(T message) where T : MessageBase
    {
        await channel.Writer.WriteAsync(message);
    }

    public Task Close(Exception? error = default)
    {
        channel.Complete(error);
        return Task.CompletedTask;
    }

    public MessageBase Message => contextMessage;

    public GraphQLRequestDelegate RequestPipeline => requestPipeline;

}