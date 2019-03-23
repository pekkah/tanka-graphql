using System.Threading;
using System.Threading.Tasks;
using tanka.graphql.resolvers;
using tanka.graphql.samples.chat.data.domain;

namespace tanka.graphql.samples.chat.data
{
    public class ChatResolverService : IChatResolverService
    {
        private readonly IChat _chat;

        public ChatResolverService(IChat chat)
        {
            _chat = chat;
        }

        public async ValueTask<IResolveResult> GetMessagesAsync(ResolverContext context)
        {
            var messages = await _chat.GetMessagesAsync(100);
            return Resolve.As(messages);
        }

        public async ValueTask<IResolveResult> AddMessageAsync(ResolverContext context)
        {
            var input = context.GetArgument<InputMessage>("message");
            var message = await _chat.AddMessageAsync(
                "1",
                input.Content);

            return Resolve.As(message);
        }

        public async ValueTask<IResolveResult> EditMessageAsync(ResolverContext context)
        {
            var id = context.GetArgument<string>("id");
            var input = context.GetArgument<InputMessage>("message");

            var message = await _chat.EditMessageAsync(
                id,
                input.Content);

            return Resolve.As(message);
        }

        public ValueTask<ISubscribeResult> StreamMessagesAsync(ResolverContext context, CancellationToken unsubscribe)
        {
            return _chat.JoinAsync(unsubscribe);
        }

        public ValueTask<IResolveResult> ResolveMessageAsync(ResolverContext context)
        {
            return ResolveSync.As(context.ObjectValue);
        }
    }
}