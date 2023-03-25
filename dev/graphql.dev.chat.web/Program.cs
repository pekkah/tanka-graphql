using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.Samples.Chat.Data;
using Tanka.GraphQL.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(json =>
{
    json.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
// configure services
builder.AddTankaGraphQL3()
    .AddOptions("chat", options => { options.Configure(schema => schema.AddChat()); })
    .AddHttp()
    .AddWebSockets();

builder.Services.AddSingleton<IChatResolverService, ChatResolverService>();
builder.Services.AddSingleton<IChat, Chat>();

var app = builder.Build();

app.UseWebSockets();

// this uses the default pipeline
app.MapTankaGraphQL3("/graphql", "chat");

app.Run();