using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tanka.graphql.graph;
using tanka.graphql.resolvers;
using tanka.graphql.type;
using static tanka.graphql.resolvers.Resolve;

namespace tanka.graphql.tests.data.starwars
{
    public class StarwarsResolvers
    {
        public static ResolverMap BuildResolvers(Starwars starwars, ISchema schema)
        {
            async Task<IResolveResult> ResolveCharacter(ResolverContext context)
            {
                var id = (string) context.Arguments["id"];
                var character = await starwars.GetCharacter(id).ConfigureAwait(false);
                return As(schema.GetNamedType<ObjectType>("Human"), character);
            }

            async Task<IResolveResult> ResolveHuman(ResolverContext context)
            {
                var id = (string) context.Arguments["id"];

                var human = await starwars.GetHuman(id).ConfigureAwait(false);
                return As(human);
            }

            async Task<IResolveResult> ResolveFriends(ResolverContext context)
            {
                var character = (Starwars.Character) context.ObjectValue;
                var friends = character.GetFriends();
                await Task.Delay(0).ConfigureAwait(false);
                return As(friends.Select(c => As(schema.GetNamedType<ObjectType>("Human"), c)));
            }

            async Task<IResolveResult> ResolveCharacters(ResolverContext context)
            {
                await Task.Delay(0).ConfigureAwait(false);
                return As(starwars.Characters.Select(c => As(schema.GetNamedType<ObjectType>("Human"), c)));
            }

            async Task<IResolveResult> AddHuman(ResolverContext context)
            {
                var humanInput = (IDictionary<string, object>) context.Arguments["human"];
                var human = starwars.AddHuman(humanInput["name"].ToString());

                await Task.Delay(0).ConfigureAwait(false);
                return As(human);
            }

            var resolverMap = new ResolverMap
            {
                // Root query
                ["Query"] = new FieldResolverMap
                {
                    {"human", new Resolver(ResolveHuman)},
                    {"character", new Resolver(ResolveCharacter)},
                    {"characters", new Resolver(ResolveCharacters)}
                },

                // Root mutation
                ["Mutation"] = new FieldResolverMap
                {
                    {"addHuman", new Resolver(AddHuman)}
                },

                // ObjectType
                ["Human"] = new FieldResolverMap
                {
                    {"id", PropertyOf<Starwars.Human>(c => c.Id)},
                    {"name", PropertyOf<Starwars.Human>(c => c.Name)},
                    {"homePlanet", PropertyOf<Starwars.Human>(c => c.HomePlanet)},
                    {"friends", new Resolver(ResolveFriends)},
                    {"appearsIn", PropertyOf<Starwars.Human>(h => h.AppearsIn)}
                }
            };

            return resolverMap;
        }
    }
}