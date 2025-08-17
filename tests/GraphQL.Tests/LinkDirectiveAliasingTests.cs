using System;
using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.TypeSystem;

using Xunit;

namespace Tanka.GraphQL.Tests;

/// <summary>
/// Tests to verify that @link directive aliasing actually works
/// </summary>
public class LinkDirectiveAliasingTests
{
    [Fact]
    public async Task Directive_Aliasing_Should_Work()
    {
        var schema = """
            extend schema @link(
              url: "https://mycompany.com/schemas/auth/v1.0",
              import: [
                { name: "@authenticated", as: "@requiresAuth" },
                { name: "@authorized", as: "@requiresPermission" }
              ]
            )

            type Query {
                profile: User
            }

            type User {
                id: ID!
                name: String
            }
            """;

        var builder = new SchemaBuilder();
        builder.Add(schema);

        var options = new SchemaBuildOptions();
        options.SchemaLoader = new TestAuthSchemaLoader();

        var builtSchema = await builder.Build(options);

        // The aliased directives should be available
        var requiresAuthDirective = builtSchema.GetDirectiveType("requiresAuth");
        var requiresPermissionDirective = builtSchema.GetDirectiveType("requiresPermission");

        // The original directive names should NOT be available (they were aliased)
        var authenticatedDirective = builtSchema.GetDirectiveType("authenticated");
        var authorizedDirective = builtSchema.GetDirectiveType("authorized");

        // Debug output
        var allDirectives = builtSchema.QueryDirectiveTypes();
        var directiveNames = string.Join(", ", allDirectives.Select(d => d.Name));

        Assert.True(requiresAuthDirective != null,
            $"requiresAuth directive not found. Available directives: {directiveNames}");
        Assert.True(requiresPermissionDirective != null,
            $"requiresPermission directive not found. Available directives: {directiveNames}");
        Assert.True(authenticatedDirective == null,
            $"authenticated directive should not be available when aliased. Available directives: {directiveNames}");
        Assert.True(authorizedDirective == null,
            $"authorized directive should not be available when aliased. Available directives: {directiveNames}");
    }

    [Fact]
    public async Task Type_Aliasing_Should_Work()
    {
        var schema = """
            extend schema @link(
              url: "https://mycompany.com/schemas/auth/v1.0",
              import: [
                { name: "Role", as: "UserRole" },
                { name: "Permission", as: "UserPermission" }
              ]
            )

            type Query {
                profile: User
            }

            type User {
                id: ID!
                name: String
                role: UserRole
                permissions: [UserPermission!]
            }
            """;

        var builtSchema = await new SchemaBuilder()
            .Add(schema)
            .Build(options =>
            {
                options.SchemaLoader = new TestAuthSchemaLoader();
            });

        // The aliased types should be available
        var userRoleType = builtSchema.GetNamedType("UserRole");
        var userPermissionType = builtSchema.GetNamedType("UserPermission");

        // The original type names should NOT be available (they were aliased)
        var roleType = builtSchema.GetNamedType("Role");
        var permissionType = builtSchema.GetNamedType("Permission");

        Assert.NotNull(userRoleType);
        Assert.NotNull(userPermissionType);
        Assert.Null(roleType);
        Assert.Null(permissionType);
    }

    [Fact]
    public async Task Mixed_Import_With_And_Without_Aliases_Should_Work()
    {
        var schema = """
            extend schema @link(
              url: "https://mycompany.com/schemas/auth/v1.0",
              import: [
                { name: "@authenticated", as: "@requiresAuth" },
                "@authorized",
                "Role"
              ]
            )

            type Query {
                profile: User
            }

            type User {
                id: ID!
                name: String
                role: Role
            }
            """;

        var builtSchema = await new SchemaBuilder()
            .Add(schema)
            .Build(options =>
            {
                options.SchemaLoader = new TestAuthSchemaLoader();
            });

        // Aliased directive should be available with new name
        Assert.NotNull(builtSchema.GetDirectiveType("requiresAuth"));
        Assert.Null(builtSchema.GetDirectiveType("authenticated"));

        // Non-aliased imports should be available with original names
        Assert.NotNull(builtSchema.GetDirectiveType("authorized"));
        Assert.NotNull(builtSchema.GetNamedType("Role"));
    }

    private class TestAuthSchemaLoader : ISchemaLoader
    {
        public bool CanLoad(string url)
        {
            var canLoad = url.StartsWith("https://mycompany.com/schemas/auth/");
            Console.WriteLine($"[TestAuthSchemaLoader] CanLoad({url}) = {canLoad}");
            return canLoad;
        }

        public Task<Language.Nodes.TypeSystem.TypeSystemDocument?> LoadSchemaAsync(
            string url,
            System.Threading.CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"[TestAuthSchemaLoader] LoadSchemaAsync({url}) called");

            if (!CanLoad(url))
                return Task.FromResult<Language.Nodes.TypeSystem.TypeSystemDocument?>(null);

            var authSchema = """
                directive @authenticated on FIELD_DEFINITION
                directive @authorized(role: Role!) on FIELD_DEFINITION
                
                enum Role {
                    USER
                    ADMIN
                    MODERATOR
                }

                enum Permission {
                    READ
                    WRITE
                    DELETE
                }
                """;

            var doc = (Language.Nodes.TypeSystem.TypeSystemDocument)authSchema;
            Console.WriteLine($"[TestAuthSchemaLoader] Returning schema with {doc.DirectiveDefinitions?.Count ?? 0} directives");
            return Task.FromResult<Language.Nodes.TypeSystem.TypeSystemDocument?>(doc);
        }
    }
}