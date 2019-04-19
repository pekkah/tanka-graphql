using System.Threading.Tasks;

namespace tanka.graphql.server.webSockets
{
    public interface ITextProtocolApplication
    {
        ValueTask OnMessage(string message);
    }
}