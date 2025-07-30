using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GraphQL.Dev.Reviews.Domain;

using Tanka.GraphQL.Extensions.ApolloFederation;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace GraphQL.Dev.Reviews;

public class ReviewsReferenceResolvers : DictionaryReferenceResolversMap
{
    public ReviewsReferenceResolvers()
    {
        this["User"] = UserReference;
        this["Product"] = ProductReference;
    }

    private static async ValueTask<ResolveReferenceResult> ProductReference(
        ResolverContext context, TypeDefinition type, IReadOnlyDictionary<string, object> representation)
    {
        await Task.Delay(0);

        var upc = representation["upc"].ToString();
        var product = new Product
        {
            Upc = upc
        };

        return new(type, product);
    }

    private static async ValueTask<ResolveReferenceResult> UserReference(
        ResolverContext context,
        TypeDefinition type,
        IReadOnlyDictionary<string, object> representation)
    {
        await Task.Delay(0);

        if (!representation.TryGetValue("id", out var idObj))
            throw new ArgumentOutOfRangeException("id", "Representation is missing the required 'id' value");

        var userId = idObj.ToString();

        if (!Db.Usernames.TryGetValue(userId, out var username))
            throw new ArgumentOutOfRangeException("id", $"User '{userId} not found");

        var user = new User
        {
            ID = userId,
            Username = username
        };

        return new(type, user);
    }
}