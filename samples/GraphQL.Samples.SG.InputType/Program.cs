using Microsoft.AspNetCore.Mvc;

using Tanka.GraphQL;
using Tanka.GraphQL.Extensions.Experimental.OneOf;
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
            // add types from current namespace
            types.AddGlobalTypes();
        });

        options.PostConfigure(configure =>
        {
            // add oneOf input type
            configure.Builder.Schema.AddOneOf();
            configure.Builder.Add($$"""
                                  extend input {{nameof(OneOfInput)}} @oneOf
                                  """);
        });
    });

// add validation rule for @oneOf directive
builder.Services.AddDefaultValidatorRule(OneOfDirective.OneOfValidationRule());

WebApplication app = builder.Build();
app.UseWebSockets();

app.MapTankaGraphQL("/graphql", "Default");
app.MapGraphiQL("/graphql/ui");
app.Run();


[ObjectType]
public static partial class Query
{
    /// <summary>
    ///     Simple query with one dependency resolved from DI
    /// </summary>
    /// <remarks>
    ///     [FromServices] is provided by Microsoft.AspNetCore.Mvc
    /// </remarks>
    /// <param name="db"></param>
    /// <returns></returns>
    public static Message[] Messages([FromServices] Db db)
    {
        return db.Messages.ToArray();
    }
}

[ObjectType]
public static partial class Mutation
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
    public static Message Post([FromArguments] InputMessage input, [FromServices] Db db)
    {
        var message = new Message
        {
            Id = Guid.NewGuid().ToString(),
            Text = input.Text
        };

        db.Messages.Add(message);
        return message;
    }

    /// <summary>
    ///     A command pattern like mutation with @oneOf input type
    /// </summary>
    /// <remarks>
    ///     @oneOf - directive is provided by Tanka.GraphQL.Extensions.Experimental
    ///     Spec PR: https://github.com/graphql/graphql-spec/pull/825
    /// </remarks>
    /// <param name="input"></param>
    /// <param name="db"></param>
    /// <returns></returns>
    public static Result? Execute([FromArguments] OneOfInput input, [FromServices] Db db)
    {
        if (input.Add is not null)
        {
            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                Text = input.Add.Text
            };

            db.Messages.Add(message);
            return new Result()
            {
                Id = message.Id
            };
        }

        if (input.Remove is null)
            throw new ArgumentNullException(nameof(input.Remove), "This should not happen as the validation rule should ensure one of these are set");

        db.Messages.RemoveAll(m => m.Id == input.Remove.Id);
        return new Result()
        {
            Id = input.Remove.Id
        };
    }
}

[ObjectType]
public partial class Message
{
    public required string Id { get; set; }

    public required string Text { get; set; }
}

[ObjectType]
public partial class Result
{
    public string Id { get; set; }
}

[InputType]
public partial class InputMessage
{
    public string Text { get; set; } = string.Empty;
}

[InputType]
public partial class OneOfInput
{
    public AddInput? Add { get; set; }

    public RemoveInput? Remove { get; set; }
}

[InputType]
public partial class AddInput
{
    public string Text { get; set; }
}

[InputType]
public partial class RemoveInput
{
    public string Id { get; set; }
}

public class Db
{
    public List<Message> Messages { get; } = new();
}