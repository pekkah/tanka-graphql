using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using static Tanka.GraphQL.Tests.Data.Starwars.Starwars;

namespace Tanka.GraphQL.Tests.Data.Starwars;

public class StarwarsResolvers
{
    public static IResolverMap BuildResolvers(Starwars starwars)
    {
        async ValueTask ResolveCharacter(ResolverContext context)
        {
            var id = (string)context.Arguments["id"];
            var character = await starwars.GetCharacter(id).ConfigureAwait(false);
            context.ResolvedValue = character;
            context.ResolveAbstractType = (definition, o) => context.Schema.GetRequiredNamedType<ObjectDefinition>("Human");
        }

        async ValueTask ResolveHuman(ResolverContext context)
        {
            var id = (string)context.Arguments["id"];

            var human = await starwars.GetHuman(id).ConfigureAwait(false);
            context.ResolvedValue = human;
        }

        async ValueTask ResolveFriends(ResolverContext context)
        {
            var character = (Starwars.Character)context.ObjectValue;
            var friends = character.GetFriends();
            await Task.Delay(0).ConfigureAwait(false);
            context.ResolvedValue = friends;
            context.ResolveAbstractType = (definition, o) => CharacterIsTypeOf(o, context);
        }

        ObjectDefinition CharacterIsTypeOf(object character, ResolverContext context)
        {
            return character switch
            {
                Starwars.Human human => context.Schema.GetRequiredNamedType<ObjectDefinition>("Human"),
                _ => throw new ArgumentOutOfRangeException(nameof(character))
            };
        }

        async ValueTask ResolveCharacters(ResolverContext context)
        {
            await Task.Delay(0).ConfigureAwait(false);
            context.ResolvedValue = starwars.Characters;
            context.ResolveAbstractType = (definition, o) => CharacterIsTypeOf(o, context);
        }

        async ValueTask AddHuman(ResolverContext context)
        {
            var humanInput = (IDictionary<string, object>)context.Arguments["human"];
            var human = starwars.AddHuman(humanInput["name"].ToString());

            await Task.Delay(0).ConfigureAwait(false);
            context.ResolvedValue = human;
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
                { "id", context => context.ResolveAsPropertyOf<Starwars.Human>(c => c.Id) },
                { "name", context => context.ResolveAsPropertyOf<Starwars.Human>(c => c.Name) },
                { "homePlanet", context => context.ResolveAsPropertyOf<Starwars.Human>(c => c.HomePlanet) },
                { "friends", ResolveFriends },
                { "appearsIn", context => context.ResolveAsPropertyOf<Starwars.Human>(h => h.AppearsIn) }
            }
        };

        return resolverMap;
    }
}