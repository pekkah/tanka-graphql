using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tanka.graphql.tests.data.starwars
{
    public class Starwars
    {
        public Starwars()
        {
            var hanSolo = new Human(this)
            {
                Id = "humans/han",
                Name = "Han",
                HomePlanet = null,
                AppearsIn = { "JEDI", "EMPIRE", "NEWHOPE"}
            };
            var luke = new Human(this)
            {
                Id = "humans/luke",
                Name = "Luke",
                HomePlanet = "Tatooine",
                Friends =
                {
                    hanSolo.Id
                },
                AppearsIn = { "JEDI", "EMPIRE", "NEWHOPE"}
            };

            hanSolo.Friends.Add(luke.Id);

            Characters.Add(hanSolo);
            Characters.Add(luke);
        }

        public List<Character> Characters { get; } = new List<Character>();

        public async Task<Human> GetHuman(string id)
        {
            await Task.Delay(0);
            return Characters.OfType<Human>().SingleOrDefault(h => h.Id == id);
        }

        public async Task<Character> GetCharacter(string id)
        {
            await Task.Delay(0);
            return Characters.SingleOrDefault(h => h.Id == id);
        }

        private IEnumerable<Character> GetFriendsOf(Human human)
        {
            return human.Friends.Select(f => Characters.Single(c => c.Id == f));
        }

        public abstract class Character
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public abstract IEnumerable<Character> GetFriends();
        }

        public class Human : Character
        {
            private readonly Starwars _starwars;

            public Human(Starwars starwars)
            {
                _starwars = starwars;
            }

            public string HomePlanet { get; set; }

            public List<string> Friends { get; } = new List<string>();

            public List<string> AppearsIn { get; } = new List<string>();


            public override IEnumerable<Character> GetFriends()
            {
                return _starwars.GetFriendsOf(this);
            }
        }

        public Human AddHuman(string name)
        {
            var human = new Human(this)
            {
                Id = $"humans/{name.ToLowerInvariant()}",
                Name = name
            };

            Characters.Add(human);

            return human;
        }
    }
}