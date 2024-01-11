using System.Threading.Channels;

namespace Tanka.GraphQL.Server.WebSockets;

public class ClientMethods(ChannelWriter<MessageBase> writer)
{
    protected ChannelWriter<MessageBase> Writer { get; } = writer;

    public async Task ConnectionAck(ConnectionAck connectionAck, CancellationToken cancellationToken)
    {
        await Writer.WriteAsync(connectionAck, cancellationToken);
    }

    public async Task Next(Next next, CancellationToken cancellationToken)
    {
        await Writer.WriteAsync(next, cancellationToken);
    }

    public async Task Error(Error error, CancellationToken cancellationToken)
    {
        await Writer.WriteAsync(error, cancellationToken);
    }

    public async Task Complete(Complete complete, CancellationToken cancellationToken)
    {
        await Writer.WriteAsync(complete, cancellationToken);
    }
}