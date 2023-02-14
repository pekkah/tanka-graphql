using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Tests.Data.Starwars;

public class StarwarsSchema
{
    public static SchemaBuilder Create()
    {
        var builder = new SchemaBuilder()
            .Add(@"
enum Episode {
    NEWHOPE
    EMPIRE
    JEDI
}

""""""Character in the movie""""""
interface Character {
    id: String!
    name: String!
    friends: [Character!]!
    appearsIn: [Episode!]!
}

""""""Human character""""""
type Human implements Character {
    id: String!
    name: String!
    friends: [Character!]!
    appearsIn: [Episode!]! 
    homePlanet: String
}

input HumanInput {
    name: String!
}

type Query {
    human(id: String!): Human
    character(id: String!): Character
    characters: [Character!]!
}

type Mutation {
    addHuman(human: HumanInput!): Human
}
");

        return builder;
    }
}