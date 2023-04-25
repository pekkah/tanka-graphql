using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Tanka.GraphQL.Server;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Simple in memory db for messages
builder.Services.AddSingleton<Db>();

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

            // Add generated input type to schema
            //todo: make a namespace add all method for this
            types.AddInputMessageInputType();
        });
    });

WebApplication app = builder.Build();
app.UseWebSockets();

app.MapTankaGraphQL("/graphql", "Default");
app.Run();


[ObjectType]
public static class Query
{
    /// <summary>
    ///     Simple query with one dependency resolved from DI
    /// </summary>
    /// <remarks>
    ///     [FromServices] is provided by Microsoft.AspNetCore.Mvc
    /// </remarks>
    /// <param name="db"></param>
    /// <returns></returns>
    public static Message[] Messages([FromServices]Db db)
    {
        return db.Messages.ToArray();
    }
}

[ObjectType]
public static class Mutation
{
    /// <summary>
    ///     Simple mutation with one InputObject argument and one dependency resolved from DI
    /// </summary>
    /// <remarks>
    ///     [FromArguments} is provided by Tanka.GraphQL.Server
    /// </remarks>
    /// <param name="input"></param>
    /// <param name="db"></param>
    /// <returns></returns>
    public static Message Post([FromArguments]InputMessage input, [FromServices]Db db)
    {
        var message = new Message
        {
            Id = Guid.NewGuid().ToString(),
            Text = input.Text
        };

        db.Messages.Add(message);
        return message;
    }
}

[ObjectType]
public class Message
{
    public required string Id { get; set; }

    public required string Text { get; set; }
}

[InputType]
public class InputMessage
{
    public string Text { get; set; } = string.Empty;
}

public class Db
{
    public ConcurrentBag<Message> Messages { get; } = new();
}