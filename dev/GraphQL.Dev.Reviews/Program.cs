using GraphQL.Dev.Reviews;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.Extensions.ApolloFederation;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Server;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ReviewsReferenceResolvers>();
builder.Services.AddSingleton<ReviewsResolvers>();

// configure services
builder.AddTankaGraphQL3()
    .AddOptions("reviews", options =>
    {
        options.AddReviews();

        // add federation as last step
        options.Configure<ReviewsReferenceResolvers>((schema, referenceResolvers) =>
        {
            // federation should be added as last step so
            // that all entity types are correctly detected
            schema.AddSubgraph(new(referenceResolvers));
        });
    })
    .AddHttp()
    .AddWebSockets();


var app = builder.Build();

app.UseWebSockets();

// this uses the default pipeline
app.MapTankaGraphQL3("/graphql", "reviews");

app.Run();