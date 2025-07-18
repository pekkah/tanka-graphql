using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tanka.GraphQL.Server.SourceGenerators.Tests;

public class TypeHelperFacts
{
    [Fact]
    public Task GetGraphQLTypeName_PrimitiveTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static bool BooleanField() => true;
                        public static byte ByteField() => 0;
                        public static sbyte SByteField() => 0;
                        public static short ShortField() => 0;
                        public static ushort UShortField() => 0;
                        public static int IntField() => 0;
                        public static uint UIntField() => 0;
                        public static long LongField() => 0;
                        public static ulong ULongField() => 0;
                        public static float FloatField() => 0.0f;
                        public static double DoubleField() => 0.0;
                        public static char CharField() => 'c';
                        public static string StringField() => "string";
                        public static decimal DecimalField() => 0.0m;
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task GetGraphQLTypeName_NullablePrimitiveTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static bool? NullableBooleanField() => null;
                        public static int? NullableIntField() => null;
                        public static float? NullableFloatField() => null;
                        public static double? NullableDoubleField() => null;
                        public static char? NullableCharField() => null;
                        public static decimal? NullableDecimalField() => null;
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task GetGraphQLTypeName_NullableReferenceTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static string? NullableStringField() => null;
                        public static object? NullableObjectField() => null;
                        public static Person? NullablePersonField() => null;
                     }
                     
                     [ObjectType]
                     public class Person
                     {
                        public string Name { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task GetGraphQLTypeName_ArrayTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static string[] StringArray() => new string[0];
                        public static int[] IntArray() => new int[0];
                        public static bool[] BoolArray() => new bool[0];
                        public static Person[] PersonArray() => new Person[0];
                        public static string?[] NullableStringArray() => new string?[0];
                        public static int?[] NullableIntArray() => new int?[0];
                        public static Person?[] NullablePersonArray() => new Person?[0];
                     }
                     
                     [ObjectType]
                     public class Person
                     {
                        public string Name { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task GetGraphQLTypeName_GenericCollectionTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static List<string> StringList() => new();
                        public static IEnumerable<string> StringEnumerable() => new List<string>();
                        public static ICollection<string> StringCollection() => new List<string>();
                        public static IList<string> StringIList() => new List<string>();
                        public static IReadOnlyList<string> StringReadOnlyList() => new List<string>();
                        public static IReadOnlyCollection<string> StringReadOnlyCollection() => new List<string>();
                        public static HashSet<string> StringHashSet() => new();
                        public static Queue<string> StringQueue() => new();
                        public static Stack<string> StringStack() => new();
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task GetGraphQLTypeName_DictionaryTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static Dictionary<string, object> StringObjectDict() => new();
                        public static IDictionary<string, object> StringObjectIDict() => new Dictionary<string, object>();
                        public static IReadOnlyDictionary<string, object> StringObjectReadOnlyDict() => new Dictionary<string, object>();
                        public static Dictionary<string, string?> StringNullableStringDict() => new();
                        public static Dictionary<string?, string> NullableStringStringDict() => new();
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task GetGraphQLTypeName_AsyncEnumerableTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static async IAsyncEnumerable<string> StringAsyncEnumerable([EnumeratorCancellation] CancellationToken cancellationToken = default)
                        {
                            yield return "test";
                        }
                        
                        public static async IAsyncEnumerable<int> IntAsyncEnumerable([EnumeratorCancellation] CancellationToken cancellationToken = default)
                        {
                            yield return 1;
                        }
                        
                        public static async IAsyncEnumerable<Person> PersonAsyncEnumerable([EnumeratorCancellation] CancellationToken cancellationToken = default)
                        {
                            yield return new Person();
                        }
                     }
                     
                     [ObjectType]
                     public class Person
                     {
                        public string Name { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task GetGraphQLTypeName_NestedGenericTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static List<List<string>> NestedStringList() => new();
                        public static Dictionary<string, List<object>> DictWithListValue() => new();
                        public static List<Dictionary<string, object>> ListWithDictValue() => new();
                        public static Dictionary<string, Dictionary<string, object>> NestedDict() => new();
                        public static List<List<List<string>>> DeeplyNestedList() => new();
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task GetGraphQLTypeName_TaskTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static Task<string> StringTask() => Task.FromResult("test");
                        public static Task<int> IntTask() => Task.FromResult(42);
                        public static Task<Person> PersonTask() => Task.FromResult(new Person());
                        public static Task<List<string>> StringListTask() => Task.FromResult(new List<string>());
                        public static Task<string?> NullableStringTask() => Task.FromResult<string?>(null);
                        public static Task<int?> NullableIntTask() => Task.FromResult<int?>(null);
                     }
                     
                     [ObjectType]
                     public class Person
                     {
                        public string Name { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task GetGraphQLTypeName_ValueTaskTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static ValueTask<string> StringValueTask() => ValueTask.FromResult("test");
                        public static ValueTask<int> IntValueTask() => ValueTask.FromResult(42);
                        public static ValueTask<Person> PersonValueTask() => ValueTask.FromResult(new Person());
                        public static ValueTask<List<string>> StringListValueTask() => ValueTask.FromResult(new List<string>());
                        public static ValueTask<string?> NullableStringValueTask() => ValueTask.FromResult<string?>(null);
                        public static ValueTask<int?> NullableIntValueTask() => ValueTask.FromResult<int?>(null);
                     }
                     
                     [ObjectType]
                     public class Person
                     {
                        public string Name { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task GetGraphQLTypeName_CustomTypesWithGraphQLNameAttribute()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static CustomPerson Person() => new();
                        public static List<CustomPerson> People() => new();
                        public static CustomPerson? OptionalPerson() => null;
                     }
                     
                     [ObjectType]
                     [GraphQLName("Person")]
                     public class CustomPerson
                     {
                        public string Name { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task GetGraphQLTypeName_ComplexNullabilityScenarios()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static List<string?> ListOfNullableStrings() => new();
                        public static List<string>? NullableListOfStrings() => null;
                        public static List<string?>? NullableListOfNullableStrings() => null;
                        public static Dictionary<string, string?> DictWithNullableValues() => new();
                        public static Dictionary<string?, string> DictWithNullableKeys() => new();
                        public static Dictionary<string?, string?>? NullableDictWithNullableKeysAndValues() => null;
                        public static Task<string?> AsyncNullableString() => Task.FromResult<string?>(null);
                        public static Task<List<string?>> AsyncListOfNullableStrings() => Task.FromResult(new List<string?>());
                        public static Task<List<string>?> AsyncNullableListOfStrings() => Task.FromResult<List<string>?>(null);
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task GetGraphQLTypeName_EnumTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static Status GetStatus() => Status.Active;
                        public static Status? GetOptionalStatus() => null;
                        public static List<Status> GetStatuses() => new();
                        public static List<Status?> GetOptionalStatuses() => new();
                        public static Task<Status> GetStatusAsync() => Task.FromResult(Status.Active);
                        public static Task<Status?> GetOptionalStatusAsync() => Task.FromResult<Status?>(null);
                     }
                     
                     public enum Status
                     {
                        Active,
                        Inactive,
                        Pending
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task GetGraphQLTypeName_SystemTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static DateTime DateTimeField() => DateTime.Now;
                        public static DateTime? OptionalDateTimeField() => null;
                        public static DateTimeOffset DateTimeOffsetField() => DateTimeOffset.Now;
                        public static DateTimeOffset? OptionalDateTimeOffsetField() => null;
                        public static TimeSpan TimeSpanField() => TimeSpan.Zero;
                        public static TimeSpan? OptionalTimeSpanField() => null;
                        public static Guid GuidField() => Guid.NewGuid();
                        public static Guid? OptionalGuidField() => null;
                        public static Uri UriField() => new Uri("https://example.com");
                        public static Uri? OptionalUriField() => null;
                        public static Version VersionField() => new Version(1, 0, 0);
                        public static Version? OptionalVersionField() => null;
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task GetGraphQLTypeName_TupleTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static (string, int) SimpleTuple() => ("test", 42);
                        public static (string name, int age) NamedTuple() => ("test", 42);
                        public static (string, int)? OptionalTuple() => null;
                        public static List<(string, int)> TupleList() => new();
                        public static Task<(string, int)> AsyncTuple() => Task.FromResult(("test", 42));
                        public static ValueTuple<string, int> ValueTuple() => new("test", 42);
                        public static Tuple<string, int> Tuple() => new("test", 42);
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task GetGraphQLTypeName_InterfaceTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static IAnimal Animal() => new Dog();
                        public static IAnimal? OptionalAnimal() => null;
                        public static List<IAnimal> Animals() => new();
                        public static List<IAnimal?> OptionalAnimals() => new();
                        public static Task<IAnimal> AsyncAnimal() => Task.FromResult<IAnimal>(new Dog());
                     }
                     
                     [InterfaceType]
                     public interface IAnimal
                     {
                        string Name { get; }
                     }
                     
                     [ObjectType]
                     public class Dog : IAnimal
                     {
                        public string Name { get; set; }
                        public string Breed { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task GetGraphQLTypeName_EdgeCaseTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static object ObjectField() => new();
                        public static object? OptionalObjectField() => null;
                        public static dynamic DynamicField() => new { };
                        public static void VoidMethod() { }
                        public static Task VoidTaskMethod() => Task.CompletedTask;
                        public static ValueTask VoidValueTaskMethod() => ValueTask.CompletedTask;
                        public static IntPtr IntPtrField() => IntPtr.Zero;
                        public static UIntPtr UIntPtrField() => UIntPtr.Zero;
                        public static nint NIntField() => 0;
                        public static nuint NUIntField() => 0;
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task TypeHelper_NamespaceHandling()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Very.Deeply.Nested.Namespace
                     {
                        [ObjectType]
                        public static class Query
                        {
                            public static string Hello() => "world";
                        }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task TypeHelper_NestedClassHandling()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests
                     {
                        public class OuterClass
                        {
                            public class MiddleClass
                            {
                                [ObjectType]
                                public class InnerClass
                                {
                                    public string Value { get; set; }
                                }
                            }
                        }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task TypeHelper_ComplexAttributeScenarios()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static string WithFromArguments([FromArguments] string input) => input;
                        public static string WithFromServices([FromServices] IService service) => service.GetData();
                        public static string WithMultipleAttributes([FromArguments] string input, [FromServices] IService service) => $"{input}: {service.GetData()}";
                        public static string WithContext(ResolverContext context) => "context";
                        public static string WithCancellation(CancellationToken cancellationToken) => "cancelled";
                        public static string WithServiceProvider(IServiceProvider serviceProvider) => "provider";
                     }
                     
                     public interface IService
                     {
                        string GetData();
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }
}
