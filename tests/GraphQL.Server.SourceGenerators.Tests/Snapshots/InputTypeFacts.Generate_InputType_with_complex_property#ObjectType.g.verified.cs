//HintName: ObjectType.g.cs
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Tanka.GraphQL.Executable;

namespace Tanka.GraphQL.Server;

public static class SourceGeneratedExecutableSchemaExtensions
{
    public static OptionsBuilder<SchemaOptions> AddGeneratedTypes(
        this OptionsBuilder<SchemaOptions> builder,
        Action<SourceGeneratedTypesBuilder> configureTypes)
    {
        var typesBuilder = new SourceGeneratedTypesBuilder(builder);
        configureTypes(typesBuilder);
        return builder;
    }
}

public class SourceGeneratedTypesBuilder
{
    public OptionsBuilder<SchemaOptions> Builder { get; }

    public SourceGeneratedTypesBuilder(OptionsBuilder<SchemaOptions> builder)
    {
        Builder = builder;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ObjectTypeAttribute: Attribute
{
}

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class FromArgumentsAttribute: Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
public class GraphQLNameAttribute: Attribute
{
}