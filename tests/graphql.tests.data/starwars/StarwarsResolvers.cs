using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.TypeSystem;
using static Tanka.GraphQL.ValueResolution.Resolve;

namespace Tanka.GraphQL.Tests.Data.Starwars
{
    public class StarwarsResolvers
    {
        public static ObjectTypeMap BuildResolvers(Starwars starwars)
        {
            async ValueTask<IResolverResult> ResolveCharacter(IResolverContext context)
            {
                var id = (string) context.Arguments["id"];
                var character = await starwars.GetCharacter(id).ConfigureAwait(false);
                return As(context.ExecutionContext.Schema.GetNamedType<ObjectType>("Human"), character);
            }

            async ValueTask<IResolverResult> ResolveHuman(IResolverContext context)
            {
                var id = (string) context.Arguments["id"];

                var human = await starwars.GetHuman(id).ConfigureAwait(false);
                return As(human);
            }

            async ValueTask<IResolverResult> ResolveFriends(IResolverContext context)
            {
                var character = (Starwars.Character) context.ObjectValue;
                var friends = character.GetFriends();
                await Task.Delay(0).ConfigureAwait(false);
                return As(friends, friend => CharacterIsTypeOf(friend, context));
            }

            IType CharacterIsTypeOf(object character, IResolverContext context)
            {
                return character switch
                {
                    Starwars.Human human => context.ExecutionContext.Schema.GetNamedType<ObjectType>("Human"),
                    _ => throw new ArgumentOutOfRangeException(nameof(character))
                };
            }

            async ValueTask<IResolverResult> ResolveCharacters(IResolverContext context)
            {
                await Task.Delay(0).ConfigureAwait(false);
                return As(starwars.Characters, character => CharacterIsTypeOf(character, context));
            }

            async ValueTask<IResolverResult> AddHuman(IResolverContext context)
            {
                var humanInput = (IDictionary<string, object>) context.Arguments["human"];
                var human = starwars.AddHuman(humanInput["name"].ToString());

                await Task.Delay(0).ConfigureAwait(false);
                return As(human);
            }

            var resolverMap = new ObjectTypeMap
            {
                // Root query
                ["Query"] = new FieldResolversMap
                {
                    {"human", ResolveHuman},
                    {"character", ResolveCharacter},
                    {"characters", ResolveCharacters}
                },

                // Root mutation
                ["Mutation"] = new FieldResolversMap
                {
                    {"addHuman", AddHuman}
                },

                // ObjectType
                ["Human"] = new FieldResolversMap
                {
                    {"id", PropertyOf<Starwars.Human>(c => c.Id)},
                    {"name", PropertyOf<Starwars.Human>(c => c.Name)},
                    {"homePlanet", PropertyOf<Starwars.Human>(c => c.HomePlanet)},
                    {"friends", ResolveFriends},
                    {"appearsIn", PropertyOf<Starwars.Human>(h => h.AppearsIn)}
                }
            };

            return resolverMap;
        }
    }
}