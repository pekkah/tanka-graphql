using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using static Tanka.GraphQL.ValueResolution.Resolve;

namespace Tanka.GraphQL.Tests.Data.Starwars;

public class StarwarsResolvers
{
    public static IResolverMap BuildResolvers(Starwars starwars)
    {
        async ValueTask<IResolverResult> ResolveCharacter(IResolverContext context)
        {
            var id = (string)context.Arguments["id"];
            var character = await starwars.GetCharacter(id).ConfigureAwait(false);
            return As(context.ExecutionContext.Schema.GetRequiredNamedType<ObjectDefinition>("Human"), character);
        }

        async ValueTask<IResolverResult> ResolveHuman(IResolverContext context)
        {
            var id = (string)context.Arguments["id"];

            var human = await starwars.GetHuman(id).ConfigureAwait(false);
            return As(human);
        }

        async ValueTask<IResolverResult> ResolveFriends(IResolverContext context)
        {
            var character = (Starwars.Character)context.ObjectValue;
            var friends = character.GetFriends();
            await Task.Delay(0).ConfigureAwait(false);
            return As(friends, (_, friend) => CharacterIsTypeOf(friend, context));
        }

        ObjectDefinition CharacterIsTypeOf(object character, IResolverContext context)
        {
            return character switch
            {
                Starwars.Human human => context.ExecutionContext.Schema.GetRequiredNamedType<ObjectDefinition>("Human"),
                _ => throw new ArgumentOutOfRangeException(nameof(character))
            };
        }

        async ValueTask<IResolverResult> ResolveCharacters(IResolverContext context)
        {
            await Task.Delay(0).ConfigureAwait(false);
            return As(starwars.Characters, (_, character) => CharacterIsTypeOf(character, context));
        }

        async ValueTask<IResolverResult> AddHuman(IResolverContext context)
        {
            var humanInput = (IDictionary<string, object>)context.Arguments["human"];
            var human = starwars.AddHuman(humanInput["name"].ToString());

            await Task.Delay(0).ConfigureAwait(false);
            return As(human);
        }

        var resolverMap = new ResolversMap
        {
            // Root query
            ["Query"] = new()
            {
                { "human", ResolveHuman },
                { "character", ResolveCharacter },
                { "characters", ResolveCharacters }
            },

            // Root mutation
            ["Mutation"] = new()
            {
                { "addHuman", AddHuman }
            },

            // ObjectType
            ["Human"] = new()
            {
                { "id", PropertyOf<Starwars.Human>(c => c.Id) },
                { "name", PropertyOf<Starwars.Human>(c => c.Name) },
                { "homePlanet", PropertyOf<Starwars.Human>(c => c.HomePlanet) },
                { "friends", ResolveFriends },
                { "appearsIn", PropertyOf<Starwars.Human>(h => h.AppearsIn) }
            }
        };

        return resolverMap;
    }
}