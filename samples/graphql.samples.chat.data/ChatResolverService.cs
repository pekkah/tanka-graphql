using System.Threading;
using System.Threading.Tasks;
using fugu.graphql.resolvers;
using fugu.graphql.samples.chat.data.domain;

namespace fugu.graphql.samples.chat.data
{
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
            return Resolve.As(messages);
        }

        public async Task<IResolveResult> AddMessageAsync(ResolverContext context)
        {
            var input = context.GetArgument<InputMessage>("message");
            var message = await _chat.AddMessageAsync(
                "1",
                input.Content);

            return Resolve.As(message);
        }

        public async Task<IResolveResult> EditMessageAsync(ResolverContext context)
        {
            var id = context.GetArgument<string>("id");
            var input = context.GetArgument<InputMessage>("message");

            var message = await _chat.EditMessageAsync(
                id,
                input.Content);

            return Resolve.As(message);
        }

        public async Task<ISubscribeResult> StreamMessagesAsync(ResolverContext context,
            CancellationToken cancellationToken)
        {
            var reader = await _chat.JoinAsync(cancellationToken);
            return Resolve.Stream(reader);
        }

        public async Task<IResolveResult> ResolveMessageAsync(ResolverContext context)
        {
            await Task.Delay(0);
            return Resolve.As(context.ObjectValue);
        }
    }
}