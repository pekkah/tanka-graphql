using System.Linq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Tanka.GraphQL.Server;

public static class SignalRBuilderExtensions
{
    public static ISignalRBuilder AddTankaGraphQL(this ISignalRBuilder builder)
    {
        builder.AddJsonProtocol(options =>
        {
            /*if (!options.PayloadSerializerOptions.Converters.Any(converter =>
                    converter is ObjectDictionaryConverter))
                options.PayloadSerializerOptions.Converters
                    .Add(new ObjectDictionaryConverter());*/
        });

        return builder;
    }
}