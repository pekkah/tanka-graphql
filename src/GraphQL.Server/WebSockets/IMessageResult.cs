namespace Tanka.GraphQL.Server.WebSockets;

public interface IMessageResult
{
    Task Execute(IMessageContext context);
}