using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Authentication;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Server;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// add required services
builder.AddTankaGraphQL()
    // add http transport
    .AddHttp()
    // add websocket transport for subscriptions
    .AddWebSockets()
    // add named schema
    .AddSchema("System", schema =>
    {
        // add Query root
        schema.Add(
            "Query",
            new FieldsWithResolvers
            {
                // Simple string field with hard coded resolved value
                { "hello: String!", () => "Hello World" }
            });

        // add Subscription root
        schema.Add(
            "Subscription",
            new FieldsWithResolvers
            {
                // this will resolve the actual resolved value from the produced values
                { "hello: String!", (string objectValue) => objectValue }
            },
            new FieldsWithSubscribers
            {
                // this is our subscription producer
                {
                    "hello: String!", (CancellationToken unsubscribe) =>
                    {
                        return Hello(unsubscribe);

                        static async IAsyncEnumerable<string> Hello(
                            [EnumeratorCancellation] CancellationToken unsubscribe)
                        {
                            yield return "Hello";
                            await Task.Delay(TimeSpan.FromSeconds(1), unsubscribe);
                            yield return "World";
                        }
                    }
                }
            });
    });

// add cookie authentication
builder.Services.AddAuthentication().AddCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/login";
});

builder.Services.AddAuthorization();

WebApplication app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
// this is required by the websocket transport
app.UseWebSockets();

// this uses the default pipeline
// you can access GraphiQL at "/graphql/ui"
app.MapTankaGraphQL("/graphql", "System")
    // we require a user name User
    .RequireAuthorization(policy => policy.RequireUserName("User"));

// map login (required by the cookie authentication)
app.MapGet("/login", async (HttpContext http, string returnUrl) =>
{
    // login as hardcoded user
    await http.SignInAsync(new ClaimsPrincipal(new GenericIdentity("User")));
    return Results.Redirect(returnUrl);
});


app.Run();