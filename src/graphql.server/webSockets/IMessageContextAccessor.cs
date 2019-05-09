namespace tanka.graphql.server.webSockets
{
    public interface IMessageContextAccessor
    {
        MessageContext Context { get; set; }
    }

    public class MessageContextAccessor : IMessageContextAccessor
    {
        public MessageContext Context { get; set; }
    }
}