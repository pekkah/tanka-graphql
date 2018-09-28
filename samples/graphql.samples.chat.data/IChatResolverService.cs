using System.Threading.Tasks;
using fugu.graphql.resolvers;
using fugu.graphql.samples.chat.data.domain;
using static fugu.graphql.resolvers.Resolve;

namespace fugu.graphql.samples.chat.data
{
    public interface IChatResolverService
    {
        Task<IResolveResult> GetMessagesAsync(ResolverContext context);

        Task<IResolveResult> AddMessageAsync(ResolverContext context);

        Task<IResolveResult> EditMessageAsync(ResolverContext context);
    }

    public class ChatResolverService : IChatResolverService
    {
        private readonly IChat _chat;

        public ChatResolverService(IChat chat)
        {
            _chat = chat;
        }

        public async Task<IResolveResult> GetMessagesAsync(ResolverContext context)
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

        public async Task<IResolveResult> EditMessageAsync(ResolverContext context)
        {
            var id = context.GetArgument<string>("id");
            var input = context.GetArgument<InputMessage>("message");

            var message = await _chat.EditMessageAsync(
                id,
                input.Content);

            return As(message);
        }
    }
}