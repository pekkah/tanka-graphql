using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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

        public async Task<ISubscribeResult> StreamMessagesAsync(ResolverContext context, CancellationToken cancellationToken)
        {
            var target = new BufferBlock<Message>();
            var subscription = await _chat.JoinAsync(target);
            cancellationToken.Register(() => subscription.Dispose());

            return Resolve.Stream(target);
        }

        public async Task<IResolveResult> ResolveMessageAsync(ResolverContext context)
        {
            await Task.Delay(0);
            return Resolve.As(context.ObjectValue);
        }
    }
}