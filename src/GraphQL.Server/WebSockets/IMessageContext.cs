namespace Tanka.GraphQL.Server.WebSockets;

public interface IMessageContext
{
    Task Write<T>(T message) where T: MessageBase;
    
    Task Close(Exception? error = default);
    
    MessageBase Message { get; }
}