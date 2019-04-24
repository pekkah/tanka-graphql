using System.Threading.Tasks;

namespace tanka.graphql.server.webSockets
{
    public interface IProtocolHandler
    {
        ValueTask Handle(MessageContext context);
    }
}