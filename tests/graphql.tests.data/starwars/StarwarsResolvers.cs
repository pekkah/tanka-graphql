using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tanka.graphql.resolvers;
using tanka.graphql.type;
using static tanka.graphql.resolvers.Resolve;

namespace tanka.graphql.tests.data.starwars
{
    public class StarwarsResolvers
    {
        public static TypeMap BuildResolvers(Starwars starwars)
        {
            async ValueTask<IResolveResult> ResolveCharacter(ResolverContext context)
            {
                var id = (string) context.Arguments["id"];
                var character = await starwars.GetCharacter(id).ConfigureAwait(false);
                return As(context.Schema.GetNamedType<ObjectType>("Human"), character);
            }

            async ValueTask<IResolveResult> ResolveHuman(ResolverContext context)
            {
                var id = (string) context.Arguments["id"];

                var human = await starwars.GetHuman(id).ConfigureAwait(false);
                return As(human);
            }

            async ValueTask<IResolveResult> ResolveFriends(ResolverContext context)
            {
                var character = (Starwars.Character) context.ObjectValue;
                var friends = character.GetFriends();
                await Task.Delay(0).ConfigureAwait(false);
                return As(friends.Select(c => As(context.Schema.GetNamedType<ObjectType>("Human"), c)));
            }

            async ValueTask<IResolveResult> ResolveCharacters(ResolverContext context)
            {
                await Task.Delay(0).ConfigureAwait(false);
                return As(starwars.Characters.Select(c => As(context.Schema.GetNamedType<ObjectType>("Human"), c)));
            }

            async ValueTask<IResolveResult> AddHuman(ResolverContext context)
            {
                var humanInput = (IDictionary<string, object>) context.Arguments["human"];
                var human = starwars.AddHuman(humanInput["name"].ToString());

                await Task.Delay(0).ConfigureAwait(false);
                return As(human);
            }

            var resolverMap = new TypeMap
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