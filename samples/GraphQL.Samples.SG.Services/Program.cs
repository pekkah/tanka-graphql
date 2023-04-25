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
            // add object type controllers from <global> namespace
            types.AddGlobalControllers();
        });
    });

WebApplication app = builder.Build();
app.UseWebSockets();

app.MapTankaGraphQL("/graphql", "Default");
app.Run();


[ObjectType]
public static class Query
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
public class HttpContextResponse
{
    private readonly HttpContext _httpContext;

    public HttpContextResponse(HttpContext httpContext)
    {
        _httpContext = httpContext;
    }

    public string Path => _httpContext.Request.Path;
    
    public string Method => _httpContext.Request.Method;

    public string Protocol => _httpContext.Request.Protocol;
}