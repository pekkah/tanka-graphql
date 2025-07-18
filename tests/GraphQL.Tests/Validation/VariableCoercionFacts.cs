using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueSerialization;
using Xunit;

namespace Tanka.GraphQL.Tests.Validation;

public class VariableCoercionFacts
{
    [Fact]
    public async Task CoerceArgumentValue_ShouldHandleStringValue()
    {
        // Given
        var schema = await CreateTestSchema();
        var argumentDefinition = CreateInputValueDefinition("arg", "String!");
        var argument = CreateArgument("arg", "test value");
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValue(
            schema, null, "arg", argumentDefinition, argument);
        
        // Then
        Assert.Equal("test value", result);
    }

    [Fact]
    public async Task CoerceArgumentValue_ShouldHandleIntValue()
    {
        // Given
        var schema = await CreateTestSchema();
        var argumentDefinition = CreateInputValueDefinition("arg", "Int!");
        var argument = CreateArgument("arg", 42);
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValue(
            schema, null, "arg", argumentDefinition, argument);
        
        // Then
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task CoerceArgumentValue_ShouldHandleFloatValue()
    {
        // Given
        var schema = await CreateTestSchema();
        var argumentDefinition = CreateInputValueDefinition("arg", "Float!");
        var argument = CreateArgument("arg", 3.14);
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValue(
            schema, null, "arg", argumentDefinition, argument);
        
        // Then
        Assert.Equal(3.14, result);
    }

    [Fact]
    public async Task CoerceArgumentValue_ShouldHandleBooleanValue()
    {
        // Given
        var schema = await CreateTestSchema();
        var argumentDefinition = CreateInputValueDefinition("arg", "Boolean!");
        var argument = CreateArgument("arg", true);
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValue(
            schema, null, "arg", argumentDefinition, argument);
        
        // Then
        Assert.Equal(true, result);
    }

    [Fact]
    public async Task CoerceArgumentValue_ShouldHandleNullValue()
    {
        // Given
        var schema = await CreateTestSchema();
        var argumentDefinition = CreateInputValueDefinition("arg", "String");
        var argument = CreateArgument("arg", null);
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValue(
            schema, null, "arg", argumentDefinition, argument);
        
        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task CoerceArgumentValue_ShouldThrowForNullOnNonNullType()
    {
        // Given
        var schema = await CreateTestSchema();
        var argumentDefinition = CreateInputValueDefinition("arg", "String!");
        var argument = CreateArgument("arg", null);
        
        // When & Then
        var exception = Assert.Throws<ValueCoercionException>(() =>
            ArgumentCoercion.CoerceArgumentValue(schema, null, "arg", argumentDefinition, argument));
        
        Assert.Contains("Argument 'arg' is non-null but no value could be coerced", exception.Message);
    }

    [Fact]
    public async Task CoerceArgumentValue_ShouldHandleVariable()
    {
        // Given
        var schema = await CreateTestSchema();
        var argumentDefinition = CreateInputValueDefinition("arg", "String!");
        var argument = CreateArgumentWithVariable("arg", "varName");
        var variableValues = new Dictionary<string, object?> { { "varName", "variable value" } };
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValue(
            schema, variableValues, "arg", argumentDefinition, argument);
        
        // Then
        Assert.Equal("variable value", result);
    }

    [Fact]
    public async Task CoerceArgumentValue_ShouldHandleUndefinedVariable()
    {
        // Given
        var schema = await CreateTestSchema();
        var argumentDefinition = CreateInputValueDefinition("arg", "String");
        var argument = CreateArgumentWithVariable("arg", "varName");
        var variableValues = new Dictionary<string, object?>();
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValue(
            schema, variableValues, "arg", argumentDefinition, argument);
        
        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task CoerceArgumentValue_ShouldThrowForUndefinedVariableOnNonNullType()
    {
        // Given
        var schema = await CreateTestSchema();
        var argumentDefinition = CreateInputValueDefinition("arg", "String!");
        var argument = CreateArgumentWithVariable("arg", "varName");
        var variableValues = new Dictionary<string, object?>();
        
        // When & Then
        var exception = Assert.Throws<ValueCoercionException>(() =>
            ArgumentCoercion.CoerceArgumentValue(schema, variableValues, "arg", argumentDefinition, argument));
        
        Assert.Contains("Argument 'arg' is non-null but no value could be coerced", exception.Message);
    }

    [Fact]
    public async Task CoerceArgumentValue_ShouldHandleVariableWithNullValue()
    {
        // Given
        var schema = await CreateTestSchema();
        var argumentDefinition = CreateInputValueDefinition("arg", "String");
        var argument = CreateArgumentWithVariable("arg", "varName");
        var variableValues = new Dictionary<string, object?> { { "varName", null } };
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValue(
            schema, variableValues, "arg", argumentDefinition, argument);
        
        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task CoerceArgumentValue_ShouldThrowForVariableWithNullValueOnNonNullType()
    {
        // Given
        var schema = await CreateTestSchema();
        var argumentDefinition = CreateInputValueDefinition("arg", "String!");
        var argument = CreateArgumentWithVariable("arg", "varName");
        var variableValues = new Dictionary<string, object?> { { "varName", null } };
        
        // When & Then
        var exception = Assert.Throws<ValueCoercionException>(() =>
            ArgumentCoercion.CoerceArgumentValue(schema, variableValues, "arg", argumentDefinition, argument));
        
        Assert.Contains("Argument 'arg' is non-null but no value could be coerced", exception.Message);
    }

    [Fact]
    public async Task CoerceArgumentValue_ShouldHandleDefaultValue()
    {
        // Given
        var schema = await CreateTestSchema();
        var argumentDefinition = CreateInputValueDefinitionWithDefault("arg", "String", "default value");
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValue(
            schema, null, "arg", argumentDefinition, null);
        
        // Then
        Assert.Equal("default value", result);
    }

    [Fact]
    public async Task CoerceArgumentValue_ShouldHandleNullDefaultValue()
    {
        // Given
        var schema = await CreateTestSchema();
        var argumentDefinition = CreateInputValueDefinitionWithDefault("arg", "String", null);
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValue(
            schema, null, "arg", argumentDefinition, null);
        
        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task CoerceArgumentValue_ShouldHandleListValue()
    {
        // Given
        var schema = await CreateTestSchema();
        var argumentDefinition = CreateInputValueDefinition("arg", "[String!]!");
        var argument = CreateArgument("arg", new[] { "item1", "item2", "item3" });
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValue(
            schema, null, "arg", argumentDefinition, argument);
        
        // Then
        Assert.IsType<object[]>(result);
        var resultArray = (object[])result;
        Assert.Equal(3, resultArray.Length);
        Assert.Equal("item1", resultArray[0]);
        Assert.Equal("item2", resultArray[1]);
        Assert.Equal("item3", resultArray[2]);
    }

    [Fact]
    public async Task CoerceArgumentValue_ShouldHandleEmptyListValue()
    {
        // Given
        var schema = await CreateTestSchema();
        var argumentDefinition = CreateInputValueDefinition("arg", "[String!]!");
        var argument = CreateArgument("arg", new string[0]);
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValue(
            schema, null, "arg", argumentDefinition, argument);
        
        // Then
        Assert.IsType<object[]>(result);
        var resultArray = (object[])result;
        Assert.Empty(resultArray);
    }

    [Fact]
    public async Task CoerceArgumentValue_ShouldHandleInputObjectValue()
    {
        // Given
        var schema = await CreateSchemaWithInputObject();
        var argumentDefinition = CreateInputValueDefinition("arg", "UserInput!");
        var inputObject = new Dictionary<string, object>
        {
            { "name", "John" },
            { "age", 30 }
        };
        var argument = CreateArgument("arg", inputObject);
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValue(
            schema, null, "arg", argumentDefinition, argument);
        
        // Then
        Assert.IsType<Dictionary<string, object>>(result);
        var resultDict = (Dictionary<string, object>)result;
        Assert.Equal("John", resultDict["name"]);
        Assert.Equal(30, resultDict["age"]);
    }

    [Fact]
    public async Task CoerceArgumentValue_ShouldHandleEnumValue()
    {
        // Given
        var schema = await CreateSchemaWithEnum();
        var argumentDefinition = CreateInputValueDefinition("arg", "Status!");
        var argument = CreateArgument("arg", "ACTIVE");
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValue(
            schema, null, "arg", argumentDefinition, argument);
        
        // Then
        Assert.Equal("ACTIVE", result);
    }

    [Fact]
    public async Task CoerceArgumentValue_ShouldHandleInvalidEnumValue()
    {
        // Given
        var schema = await CreateSchemaWithEnum();
        var argumentDefinition = CreateInputValueDefinition("arg", "Status!");
        var argument = CreateArgument("arg", "INVALID");
        
        // When & Then
        Assert.Throws<ValueCoercionException>(() =>
            ArgumentCoercion.CoerceArgumentValue(schema, null, "arg", argumentDefinition, argument));
    }

    [Fact]
    public async Task CoerceArgumentValue_ShouldHandleCustomScalarValue()
    {
        // Given
        var schema = await CreateSchemaWithCustomScalar();
        var argumentDefinition = CreateInputValueDefinition("arg", "DateTime!");
        var dateTime = DateTime.Now;
        var argument = CreateArgument("arg", dateTime);
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValue(
            schema, null, "arg", argumentDefinition, argument);
        
        // Then
        Assert.Equal(dateTime, result);
    }

    [Fact]
    public async Task CoerceArgumentValue_ShouldHandleIntToFloatCoercion()
    {
        // Given
        var schema = await CreateTestSchema();
        var argumentDefinition = CreateInputValueDefinition("arg", "Float!");
        var argument = CreateArgument("arg", 42);
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValue(
            schema, null, "arg", argumentDefinition, argument);
        
        // Then
        Assert.Equal(42.0, result);
    }

    [Fact]
    public async Task CoerceArgumentValue_ShouldHandleStringToIntCoercionFailure()
    {
        // Given
        var schema = await CreateTestSchema();
        var argumentDefinition = CreateInputValueDefinition("arg", "Int!");
        var argument = CreateArgument("arg", "not a number");
        
        // When & Then
        Assert.Throws<ValueCoercionException>(() =>
            ArgumentCoercion.CoerceArgumentValue(schema, null, "arg", argumentDefinition, argument));
    }

    [Fact]
    public async Task CoerceArgumentValues_ShouldCoerceAllArguments()
    {
        // Given
        var schema = await CreateTestSchema();
        var objectDefinition = schema.GetRequiredNamedType<ObjectDefinition>("Query");
        var field = CreateFieldSelection("testField", 
            ("stringArg", "test"),
            ("intArg", 42),
            ("boolArg", true));
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValues(
            schema, objectDefinition, field, null);
        
        // Then
        Assert.Equal(3, result.Count);
        Assert.Equal("test", result["stringArg"]);
        Assert.Equal(42, result["intArg"]);
        Assert.Equal(true, result["boolArg"]);
    }

    [Fact]
    public async Task CoerceArgumentValues_ShouldHandleFieldWithoutArguments()
    {
        // Given
        var schema = await CreateTestSchema();
        var objectDefinition = schema.GetRequiredNamedType<ObjectDefinition>("Query");
        var field = CreateFieldSelection("simpleField");
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValues(
            schema, objectDefinition, field, null);
        
        // Then
        Assert.Empty(result);
    }

    [Fact]
    public async Task CoerceArgumentValues_ShouldHandleFieldWithVariables()
    {
        // Given
        var schema = await CreateTestSchema();
        var objectDefinition = schema.GetRequiredNamedType<ObjectDefinition>("Query");
        var field = CreateFieldSelectionWithVariable("testField", "stringArg", "varName");
        var variableValues = new Dictionary<string, object?> { { "varName", "variable value" } };
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValues(
            schema, objectDefinition, field, variableValues);
        
        // Then
        Assert.Single(result);
        Assert.Equal("variable value", result["stringArg"]);
    }

    [Fact]
    public async Task CoerceArgumentValues_ShouldHandleDefaultValues()
    {
        // Given
        var schema = await CreateSchemaWithDefaultValues();
        var objectDefinition = schema.GetRequiredNamedType<ObjectDefinition>("Query");
        var field = CreateFieldSelection("fieldWithDefaults");
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValues(
            schema, objectDefinition, field, null);
        
        // Then
        Assert.Equal(2, result.Count);
        Assert.Equal("default", result["stringArg"]);
        Assert.Equal(10, result["intArg"]);
    }

    [Fact]
    public async Task CoerceArgumentValues_ShouldOverrideDefaultValues()
    {
        // Given
        var schema = await CreateSchemaWithDefaultValues();
        var objectDefinition = schema.GetRequiredNamedType<ObjectDefinition>("Query");
        var field = CreateFieldSelection("fieldWithDefaults",
            ("stringArg", "override"));
        
        // When
        var result = ArgumentCoercion.CoerceArgumentValues(
            schema, objectDefinition, field, null);
        
        // Then
        Assert.Equal(2, result.Count);
        Assert.Equal("override", result["stringArg"]);
        Assert.Equal(10, result["intArg"]);
    }

    // Helper methods
    private static async Task<ISchema> CreateTestSchema()
    {
        return await new SchemaBuilder()
            .Add(@"
                type Query {
                    testField(stringArg: String!, intArg: Int!, boolArg: Boolean!): String
                    simpleField: String
                }
            ")
            .Build(new SchemaBuildOptions());
    }

    private static async Task<ISchema> CreateSchemaWithInputObject()
    {
        return await new SchemaBuilder()
            .Add(@"
                input UserInput {
                    name: String!
                    age: Int!
                }
                
                type Query {
                    createUser(input: UserInput!): String
                }
            ")
            .Build(new SchemaBuildOptions());
    }

    private static async Task<ISchema> CreateSchemaWithEnum()
    {
        return await new SchemaBuilder()
            .Add(@"
                enum Status {
                    ACTIVE
                    INACTIVE
                    PENDING
                }
                
                type Query {
                    getByStatus(status: Status!): String
                }
            ")
            .Build(new SchemaBuildOptions());
    }

    private static async Task<ISchema> CreateSchemaWithCustomScalar()
    {
        return await new SchemaBuilder()
            .Add(@"
                scalar DateTime
                
                type Query {
                    getByDate(date: DateTime!): String
                }
            ")
            .Build(new SchemaBuildOptions());
    }

    private static async Task<ISchema> CreateSchemaWithDefaultValues()
    {
        return await new SchemaBuilder()
            .Add(@"
                type Query {
                    fieldWithDefaults(stringArg: String = ""default"", intArg: Int = 10): String
                }
            ")
            .Build(new SchemaBuildOptions());
    }

    private static InputValueDefinition CreateInputValueDefinition(string name, string type)
    {
        return new InputValueDefinition
        {
            Name = name,
            Type = ParseType(type)
        };
    }

    private static InputValueDefinition CreateInputValueDefinitionWithDefault(string name, string type, object? defaultValue)
    {
        return new InputValueDefinition
        {
            Name = name,
            Type = ParseType(type),
            DefaultValue = defaultValue != null ? new DefaultValue { Value = defaultValue } : null
        };
    }

    private static Argument CreateArgument(string name, object? value)
    {
        return new Argument
        {
            Name = name,
            Value = value
        };
    }

    private static Argument CreateArgumentWithVariable(string name, string variableName)
    {
        return new Argument
        {
            Name = name,
            Value = new Variable { Name = variableName }
        };
    }

    private static FieldSelection CreateFieldSelection(string name, params (string argName, object value)[] arguments)
    {
        var args = arguments.Select(arg => CreateArgument(arg.argName, arg.value)).ToArray();
        return new FieldSelection
        {
            Name = name,
            Arguments = args.Length > 0 ? new Arguments(args) : null
        };
    }

    private static FieldSelection CreateFieldSelectionWithVariable(string name, string argName, string variableName)
    {
        return new FieldSelection
        {
            Name = name,
            Arguments = new Arguments(new[] { CreateArgumentWithVariable(argName, variableName) })
        };
    }

    private static TypeBase ParseType(string type)
    {
        if (type.EndsWith("!"))
        {
            var innerType = ParseType(type.Substring(0, type.Length - 1));
            return new NonNullType { OfType = innerType };
        }
        
        if (type.StartsWith("[") && type.EndsWith("]"))
        {
            var innerType = ParseType(type.Substring(1, type.Length - 2));
            return new ListType { OfType = innerType };
        }
        
        return new NamedType { Name = type };
    }
}