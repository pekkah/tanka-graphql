using System.Threading.Tasks;
using fugu.graphql.resolvers;
using fugu.graphql.samples.chat.data.domain;
using static fugu.graphql.resolvers.Resolve;

namespace fugu.graphql.samples.chat.data
{
    public interface IMessageResolverService
    {
        Task<IResolveResult> GetMessagesAsync();

        Task<IResolveResult> AddMessageAsync(ResolverContext context);
    }

    public class MessageResolverService : IMessageResolverService
    {
        private readonly IChat _chat;

        public MessageResolverService(IChat chat)
        {
            _chat = chat;
        }

        public async Task<IResolveResult> GetMessagesAsync()
        {
            var messages = await _chat.GetMessagesAsync(100);
            return As(messages);
        }

        public async Task<IResolveResult> AddMessageAsync(ResolverContext context)
        {
            var input = context.GetArgument<InputMessage>("message");
            var message = await _chat.AddMessageAsync(
                "1",
                input.Content);

            return As(message);
        }
    }
}