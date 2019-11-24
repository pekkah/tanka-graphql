using System.Threading.Tasks;

namespace Tanka.GraphQL.Server.WebSockets
{
    public interface IProtocolHandler
    {
        ValueTask Handle(MessageContext context);
    }
}