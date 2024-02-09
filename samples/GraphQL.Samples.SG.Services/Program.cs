using Microsoft.AspNetCore.Mvc;
using Tanka.GraphQL.Server;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add HttpContext accessor as service
builder.Services.AddHttpContextAccessor();

// Add tanka graphql
builder.AddTankaGraphQL()
    .AddHttp()
    .AddWebSockets()
    .AddSchemaOptions("Default", options =>
    {
        options.AddGeneratedTypes(types =>
        {
            // add types from current namespace
            types.AddGlobalTypes();
        });
    });

WebApplication app = builder.Build();
app.UseWebSockets();

app.MapTankaGraphQL("/graphql", "Default");
app.MapGraphiQL("/graphql/ui");
app.Run();


[ObjectType]
public static partial class Query
{
    public static HttpContextResponse? HttpContext(
        // Use [FromServices] to inject service from DI to the resolver;
        // without the attribute the generated code would first check if there's an
        // GraphQL argument with the same name and use it instead and if there's
        // no argument then it would try to resolve the service from the DI.
        // Services can be registered as scoped, singleton or transient.
        [FromServices]IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor.HttpContext == null)
            return null;

        return new HttpContextResponse(httpContextAccessor.HttpContext);
    }
}

[ObjectType]
public partial class HttpContextResponse(HttpContext httpContext)
{
    public string Path => httpContext.Request.Path;
    
    public string Method => httpContext.Request.Method;

    public string Protocol => httpContext.Request.Protocol;
}