namespace Tanka.GraphQL.Server.SourceGenerators.Tests;

public class ComplexTypeScenariosFacts
{
    [Fact]
    public Task DeeplyNestedGenericTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static Dictionary<string, List<Tuple<int, Dictionary<string, object>>>> ComplexNested() => new();
                        public static Task<Dictionary<string, List<Tuple<int, Dictionary<string, object>>>>> ComplexNestedAsync() => Task.FromResult(new Dictionary<string, List<Tuple<int, Dictionary<string, object>>>>());
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task CircularReferencesInObjectTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public class Person
                     {
                        public string Name { get; set; }
                        public List<Person> Children { get; set; } = new();
                        public Person? Parent { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task MutuallyRecursiveTypes()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public class Company
                     {
                        public string Name { get; set; }
                        public List<Employee> Employees { get; set; } = new();
                     }
                     
                     [ObjectType]
                     public class Employee
                     {
                        public string Name { get; set; }
                        public Company Company { get; set; }
                        public Employee? Manager { get; set; }
                        public List<Employee> Subordinates { get; set; } = new();
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task ComplexInheritanceHierarchy()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IEntity
                     {
                        string Id { get; }
                        DateTime CreatedAt { get; }
                     }
                     
                     [InterfaceType]
                     public interface INamed : IEntity
                     {
                        string Name { get; }
                     }
                     
                     [ObjectType]
                     public class Person : INamed
                     {
                        public string Id { get; set; }
                        public DateTime CreatedAt { get; set; }
                        public string Name { get; set; }
                        public int Age { get; set; }
                     }
                     
                     [ObjectType]
                     public class Company : INamed
                     {
                        public string Id { get; set; }
                        public DateTime CreatedAt { get; set; }
                        public string Name { get; set; }
                        public List<Person> Employees { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task MultipleInterfaceImplementations()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [InterfaceType]
                     public interface IIdentifiable
                     {
                        string Id { get; }
                     }
                     
                     [InterfaceType]
                     public interface ITimestamped
                     {
                        DateTime CreatedAt { get; }
                        DateTime UpdatedAt { get; }
                     }
                     
                     [InterfaceType]
                     public interface IVersioned
                     {
                        int Version { get; }
                     }
                     
                     [ObjectType]
                     public class Document : IIdentifiable, ITimestamped, IVersioned
                     {
                        public string Id { get; set; }
                        public DateTime CreatedAt { get; set; }
                        public DateTime UpdatedAt { get; set; }
                        public int Version { get; set; }
                        public string Title { get; set; }
                        public string Content { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task ComplexNullabilityPatterns()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static string? OptionalString() => null;
                        public static List<string?> ListOfOptionalStrings() => new();
                        public static List<string>? OptionalListOfStrings() => null;
                        public static List<string?>? OptionalListOfOptionalStrings() => null;
                        public static Dictionary<string, string?> DictWithOptionalValues() => new();
                        public static Dictionary<string?, string> DictWithOptionalKeys() => new();
                        public static Task<string?> AsyncOptionalString() => Task.FromResult<string?>(null);
                        public static Task<List<string?>> AsyncListOfOptionalStrings() => Task.FromResult(new List<string?>());
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task ComplexGenericConstraints()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public class Repository<T> where T : class, IEntity, new()
                     {
                        public List<T> Items { get; set; } = new();
                        public T? GetById(string id) => Items.FirstOrDefault(x => x.Id == id);
                        public async Task<T> CreateAsync(T entity) => await Task.FromResult(entity);
                     }
                     
                     public interface IEntity
                     {
                        string Id { get; set; }
                     }
                     
                     [ObjectType]
                     public class Person : IEntity
                     {
                        public string Id { get; set; }
                        public string Name { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task ComplexAsyncPatterns()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static async Task<string> SimpleAsync() => await Task.FromResult("hello");
                        public static async ValueTask<string> SimpleValueTaskAsync() => await ValueTask.FromResult("hello");
                        public static async IAsyncEnumerable<string> AsyncEnumerable([EnumeratorCancellation] CancellationToken cancellationToken = default)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                await Task.Delay(100, cancellationToken);
                                yield return $"Item {i}";
                            }
                        }
                        public static async IAsyncEnumerable<Dictionary<string, object>> ComplexAsyncEnumerable([EnumeratorCancellation] CancellationToken cancellationToken = default)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                await Task.Delay(100, cancellationToken);
                                yield return new Dictionary<string, object> { ["index"] = i, ["data"] = $"Item {i}" };
                            }
                        }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task ComplexParameterScenarios()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static string MixedParameters(
                            [FromArguments] string name,
                            [FromServices] IService service,
                            ResolverContext context,
                            CancellationToken cancellationToken,
                            [FromArguments] int? optionalAge = null,
                            [FromServices] IOptionalService? optionalService = null)
                        {
                            return $"Hello {name}";
                        }
                        
                        public static async Task<string> ComplexAsyncMethod(
                            [FromArguments] List<PersonInput> people,
                            [FromServices] IPersonService personService,
                            ResolverContext context,
                            CancellationToken cancellationToken)
                        {
                            return await personService.ProcessPeopleAsync(people, cancellationToken);
                        }
                     }
                     
                     [InputType]
                     public class PersonInput
                     {
                        public string Name { get; set; }
                        public int Age { get; set; }
                        public List<string> Tags { get; set; } = new();
                     }
                     
                     public interface IService
                     {
                        string GetData();
                     }
                     
                     public interface IOptionalService
                     {
                        string GetOptionalData();
                     }
                     
                     public interface IPersonService
                     {
                        Task<string> ProcessPeopleAsync(List<PersonInput> people, CancellationToken cancellationToken);
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task NestedClassesAndNamespaces()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests.Models
                     {
                        public class OuterClass
                        {
                            [ObjectType]
                            public class InnerType
                            {
                                public string Value { get; set; }
                                public NestedInnerType Nested { get; set; }
                                
                                [ObjectType]
                                public class NestedInnerType
                                {
                                    public string NestedValue { get; set; }
                                }
                            }
                        }
                     }
                     
                     namespace Tests
                     {
                        [ObjectType]
                        public static class Query
                        {
                            public static Tests.Models.OuterClass.InnerType GetInner() => new();
                        }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task ComplexCollectionTypes()
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
                        public static string[] StringArray() => new string[0];
                        public static HashSet<string> StringHashSet() => new();
                        public static Queue<string> StringQueue() => new();
                        public static Stack<string> StringStack() => new();
                        public static Dictionary<string, object> StringObjectDict() => new();
                        public static IDictionary<string, object> StringObjectIDict() => new Dictionary<string, object>();
                        public static IReadOnlyDictionary<string, object> StringObjectReadOnlyDict() => new Dictionary<string, object>();
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task ComplexUnionScenarios()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static SearchResult Search(string query) => new PersonResult();
                        public static List<SearchResult> SearchMultiple(string query) => new();
                     }
                     
                     [ObjectType]
                     public abstract class SearchResult
                     {
                        public string Id { get; set; }
                        public string Title { get; set; }
                     }
                     
                     [ObjectType]
                     public class PersonResult : SearchResult
                     {
                        public string Name { get; set; }
                        public int Age { get; set; }
                     }
                     
                     [ObjectType]
                     public class CompanyResult : SearchResult
                     {
                        public string CompanyName { get; set; }
                        public string Industry { get; set; }
                     }
                     
                     [ObjectType]
                     public class DocumentResult : SearchResult
                     {
                        public string Content { get; set; }
                        public string Author { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task ComplexEnumScenarios()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static Status GetStatus() => Status.Active;
                        public static List<Status> GetStatuses() => new() { Status.Active, Status.Inactive };
                        public static Dictionary<Status, string> GetStatusDescriptions() => new();
                        public static Status? GetOptionalStatus() => null;
                        public static async Task<Status> GetStatusAsync() => await Task.FromResult(Status.Active);
                     }
                     
                     public enum Status
                     {
                        Active,
                        Inactive,
                        Pending,
                        Suspended
                     }
                     
                     [Flags]
                     public enum Permissions
                     {
                        None = 0,
                        Read = 1,
                        Write = 2,
                        Execute = 4,
                        Admin = Read | Write | Execute
                     }
                     
                     [ObjectType]
                     public class User
                     {
                        public string Name { get; set; }
                        public Status Status { get; set; }
                        public Permissions Permissions { get; set; }
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }

    [Fact]
    public Task ComplexMixedScenario()
    {
        var source = """
                     using Tanka.GraphQL.Server;
                     
                     namespace Tests;

                     [ObjectType]
                     public static class Query
                     {
                        public static async Task<List<ISearchResult>> ComplexSearch(
                            [FromArguments] SearchInput input,
                            [FromServices] ISearchService searchService,
                            ResolverContext context,
                            CancellationToken cancellationToken)
                        {
                            return await searchService.SearchAsync(input, cancellationToken);
                        }
                        
                        public static async IAsyncEnumerable<ISearchResult> StreamingSearch(
                            [FromArguments] SearchInput input,
                            [FromServices] ISearchService searchService,
                            [EnumeratorCancellation] CancellationToken cancellationToken = default)
                        {
                            await foreach (var result in searchService.StreamSearchAsync(input, cancellationToken))
                            {
                                yield return result;
                            }
                        }
                     }
                     
                     [InputType]
                     public class SearchInput
                     {
                        public string Query { get; set; }
                        public List<string> Categories { get; set; } = new();
                        public SearchFilters? Filters { get; set; }
                        public int? Limit { get; set; }
                        public int? Offset { get; set; }
                     }
                     
                     [InputType]
                     public class SearchFilters
                     {
                        public DateRange? DateRange { get; set; }
                        public List<string> Tags { get; set; } = new();
                        public Dictionary<string, object> CustomFilters { get; set; } = new();
                     }
                     
                     [InputType]
                     public class DateRange
                     {
                        public DateTime? Start { get; set; }
                        public DateTime? End { get; set; }
                     }
                     
                     [InterfaceType]
                     public interface ISearchResult
                     {
                        string Id { get; }
                        string Title { get; }
                        float Score { get; }
                        DateTime CreatedAt { get; }
                     }
                     
                     [ObjectType]
                     public class PersonSearchResult : ISearchResult
                     {
                        public string Id { get; set; }
                        public string Title { get; set; }
                        public float Score { get; set; }
                        public DateTime CreatedAt { get; set; }
                        public string FirstName { get; set; }
                        public string LastName { get; set; }
                        public string Email { get; set; }
                        public List<string> Skills { get; set; } = new();
                     }
                     
                     public interface ISearchService
                     {
                        Task<List<ISearchResult>> SearchAsync(SearchInput input, CancellationToken cancellationToken);
                        IAsyncEnumerable<ISearchResult> StreamSearchAsync(SearchInput input, CancellationToken cancellationToken);
                     }
                     """;

        return TestHelper<ObjectTypeGenerator>.Verify(source);
    }
}