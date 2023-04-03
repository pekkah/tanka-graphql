using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Samples.Chat.Data.Domain;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Samples.Chat.Data;

public class ChatResolverService : IChatResolverService
{
    public async ValueTask GetMessagesAsync(ResolverContext context)
    {
        var messages = await context.GetRequiredService<IChat>().GetMessagesAsync(100);
        context.ResolvedValue = messages;
    }

    public async ValueTask AddMessageAsync(ResolverContext context)
    {
        var input = context.BindInputObject<InputMessage>("message");
        var message = await context.GetRequiredService<IChat>().AddMessageAsync(
            "1",
            input.Content);

        context.ResolvedValue = message;
    }

    public async ValueTask EditMessageAsync(ResolverContext context)
    {
        var id = context.GetArgument<string>("id");
        var input = context.BindInputObject<InputMessage>("message");

        var message = await context.GetRequiredService<IChat>().EditMessageAsync(
            id,
            input.Content);

        context.ResolvedValue = message;
    }

    public async ValueTask StreamMessagesAsync(SubscriberContext context, CancellationToken unsubscribe)
    {
        context.ResolvedValue = context.GetRequiredService<IChat>().JoinAsync(unsubscribe);
    }

    public ValueTask ResolveMessageAsync(ResolverContext context) => context.ResolveAs(context.ObjectValue);
}