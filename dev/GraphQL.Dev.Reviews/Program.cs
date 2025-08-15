using GraphQL.Dev.Reviews;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Tanka.GraphQL.Dev.Reviews;
using Tanka.GraphQL.Extensions.ApolloFederation;
using Tanka.GraphQL.Server;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ReviewsReferenceResolvers>();
builder.Services.AddSingleton<ReviewsResolvers>();

// configure services
builder.AddTankaGraphQL()
    .AddSchemaOptions("reviews", optionsBuilder =>
    {
        optionsBuilder.AddReviews();

        // add federation as last step
        optionsBuilder.Configure<ReviewsReferenceResolvers>((options, referenceResolvers) =>
        {
            options.ConfigureBuild(build =>
            {
                build.UseFederation(new SubgraphOptions(referenceResolvers));
            });
        });

    })
    .AddHttp()
    .AddWebSockets();


var app = builder.Build();

app.UseWebSockets();

// this uses the default pipeline
app.MapTankaGraphQL("/graphql", "reviews");

app.Run();