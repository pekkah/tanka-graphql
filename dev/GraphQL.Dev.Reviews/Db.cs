using System.Collections.Generic;

using GraphQL.Dev.Reviews.Domain;

namespace GraphQL.Dev.Reviews;

public static class Db
{
    public static Dictionary<string, Review> Reviews { get; } = new()
    {
        ["1"] = new Review
        {
            ID = "1",
            AuthorID = "1",
            Product = new Product { Upc = "1" },
            Body = "Love it!"
        },
        ["2"] = new Review
        {
            ID = "2",
            AuthorID = "1",
            Product = new Product { Upc = "2" },
            Body = "Too expensive!"
        },
        ["3"] = new Review
        {
            ID = "3",
            AuthorID = "2",
            Product = new Product { Upc = "3" },
            Body = "Could be better"
        },
        ["4"] = new Review
        {
            ID = "4",
            AuthorID = "2",
            Product = new Product { Upc = "1" },
            Body = "Prefer something else"
        }
    };

    public static Dictionary<string, string> Usernames { get; } = new()
    {
        ["1"] = "@ada",
        ["2"] = "@complete"
    };
}