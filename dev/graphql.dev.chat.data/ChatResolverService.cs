using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.Samples.Chat.Data.Domain;
using Tanka.GraphQL.Server;

namespace Tanka.GraphQL.Samples.Chat.Data
{
    public class ChatResolverService : IChatResolverService
    {
        public async ValueTask<IResolveResult> GetMessagesAsync(ResolverContext context)
        {
            var messages = await context.Use<IChat>().GetMessagesAsync(100);
            return Resolve.As(messages);
        }

        public async ValueTask<IResolveResult> AddMessageAsync(ResolverContext context)
        {
            var input = context.GetObjectArgument<InputMessage>("message");
            var message = await context.Use<IChat>().AddMessageAsync(
                "1",
                input.Content);

            return Resolve.As(message);
        }

        public async ValueTask<IResolveResult> EditMessageAsync(ResolverContext context)
        {
            var id = context.GetArgument<string>("id");
            var input = context.GetObjectArgument<InputMessage>("message");

            var message = await context.Use<IChat>().EditMessageAsync(
                id,
                input.Content);

            return Resolve.As(message);
        }

        public ValueTask<ISubscribeResult> StreamMessagesAsync(ResolverContext context, CancellationToken unsubscribe)
        {
            return context.Use<IChat>().JoinAsync(unsubscribe);
        }

        public ValueTask<IResolveResult> ResolveMessageAsync(ResolverContext context)
        {
            return ResolveSync.As(context.ObjectValue);
        }
    }
}