using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

using Tanka.GraphQL.Request;
using Tanka.GraphQL.Response;
using Tanka.GraphQL.Server;

using Xunit;

namespace Tanka.GraphQL.Server.Tests;

public class AuthenticationAuthorizationFacts
{
    [Fact]
    public async Task Middleware_ShouldAccessAuthenticatedUser()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "test-user"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim("custom-claim", "custom-value")
        };
        var identity = new ClaimsIdentity(claims, "test");
        httpContext.User = new ClaimsPrincipal(identity);

        var context = new GraphQLRequestContext();
        context.HttpContext = httpContext;

        var userInfo = new UserInfo();
        var authMiddleware = new AuthenticationMiddleware(userInfo);

        GraphQLRequestDelegate finalDelegate = ctx =>
        {
            Assert.True(userInfo.IsAuthenticated);
            Assert.Equal("test-user", userInfo.Name);
            Assert.Equal("test@example.com", userInfo.Email);
            Assert.Equal("custom-value", userInfo.CustomClaim);
            return Task.CompletedTask;
        };

        // Act
        await authMiddleware.Invoke(context, finalDelegate);

        // Assert
        Assert.True(userInfo.IsAuthenticated);
    }

    [Fact]
    public async Task Middleware_ShouldHandleUnauthenticatedUser()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        // Leave User as default (no authentication)

        var context = new GraphQLRequestContext();
        context.HttpContext = httpContext;

        var userInfo = new UserInfo();
        var authMiddleware = new AuthenticationMiddleware(userInfo);

        GraphQLRequestDelegate finalDelegate = ctx =>
        {
            Assert.False(userInfo.IsAuthenticated);
            Assert.Null(userInfo.Name);
            Assert.Null(userInfo.Email);
            return Task.CompletedTask;
        };

        // Act
        await authMiddleware.Invoke(context, finalDelegate);

        // Assert
        Assert.False(userInfo.IsAuthenticated);
    }

    [Fact]
    public async Task Middleware_ShouldBlockUnauthorizedRequests()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "regular-user"),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, "test");
        httpContext.User = new ClaimsPrincipal(identity);

        var context = new GraphQLRequestContext();
        context.HttpContext = httpContext;
        context.Request = new GraphQLRequest { Query = "{ adminOnlyField }" };

        var authorizationMiddleware = new AuthorizationMiddleware();
        var finalDelegateCalled = false;

        GraphQLRequestDelegate finalDelegate = ctx =>
        {
            finalDelegateCalled = true;
            return Task.CompletedTask;
        };

        // Act
        await authorizationMiddleware.Invoke(context, finalDelegate);

        // Assert
        Assert.False(finalDelegateCalled);
        
        // Check that error response was set
        await foreach (var result in context.Response)
        {
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Contains("Unauthorized", result.Errors[0].Message);
            break;
        }
    }

    [Fact]
    public async Task Middleware_ShouldAllowAuthorizedRequests()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "admin-user"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "test");
        httpContext.User = new ClaimsPrincipal(identity);

        var context = new GraphQLRequestContext();
        context.HttpContext = httpContext;
        context.Request = new GraphQLRequest { Query = "{ adminOnlyField }" };

        var authorizationMiddleware = new AuthorizationMiddleware();
        var finalDelegateCalled = false;

        GraphQLRequestDelegate finalDelegate = ctx =>
        {
            finalDelegateCalled = true;
            return Task.CompletedTask;
        };

        // Act
        await authorizationMiddleware.Invoke(context, finalDelegate);

        // Assert
        Assert.True(finalDelegateCalled);
    }

    [Fact]
    public async Task Middleware_ShouldRequireAuthentication()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        // No authentication

        var context = new GraphQLRequestContext();
        context.HttpContext = httpContext;
        context.Request = new GraphQLRequest { Query = "{ secureField }" };

        var requireAuthMiddleware = new RequireAuthenticationMiddleware();
        var finalDelegateCalled = false;

        GraphQLRequestDelegate finalDelegate = ctx =>
        {
            finalDelegateCalled = true;
            return Task.CompletedTask;
        };

        // Act
        await requireAuthMiddleware.Invoke(context, finalDelegate);

        // Assert
        Assert.False(finalDelegateCalled);
        
        // Check that error response was set
        await foreach (var result in context.Response)
        {
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Contains("Authentication required", result.Errors[0].Message);
            break;
        }
    }

    [Fact]
    public async Task Middleware_ShouldValidateSpecificClaims()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "test-user"),
            new Claim("department", "engineering"),
            new Claim("clearance-level", "3")
        };
        var identity = new ClaimsIdentity(claims, "test");
        httpContext.User = new ClaimsPrincipal(identity);

        var context = new GraphQLRequestContext();
        context.HttpContext = httpContext;

        var claimsValidationMiddleware = new ClaimsValidationMiddleware(
            requiredClaims: new Dictionary<string, string>
            {
                { "department", "engineering" },
                { "clearance-level", "3" }
            });

        var finalDelegateCalled = false;

        GraphQLRequestDelegate finalDelegate = ctx =>
        {
            finalDelegateCalled = true;
            return Task.CompletedTask;
        };

        // Act
        await claimsValidationMiddleware.Invoke(context, finalDelegate);

        // Assert
        Assert.True(finalDelegateCalled);
    }

    [Fact]
    public async Task Middleware_ShouldRejectMissingClaims()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "test-user"),
            new Claim("department", "marketing") // Different department
        };
        var identity = new ClaimsIdentity(claims, "test");
        httpContext.User = new ClaimsPrincipal(identity);

        var context = new GraphQLRequestContext();
        context.HttpContext = httpContext;

        var claimsValidationMiddleware = new ClaimsValidationMiddleware(
            requiredClaims: new Dictionary<string, string>
            {
                { "department", "engineering" },
                { "clearance-level", "3" }
            });

        var finalDelegateCalled = false;

        GraphQLRequestDelegate finalDelegate = ctx =>
        {
            finalDelegateCalled = true;
            return Task.CompletedTask;
        };

        // Act
        await claimsValidationMiddleware.Invoke(context, finalDelegate);

        // Assert
        Assert.False(finalDelegateCalled);
        
        // Check that error response was set
        await foreach (var result in context.Response)
        {
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Contains("Insufficient claims", result.Errors[0].Message);
            break;
        }
    }

    [Fact]
    public async Task Middleware_ShouldHandleRoleBasedAuthorization()
    {
        // Test multiple roles
        var testCases = new[]
        {
            new { Roles = new[] { "Admin" }, ExpectedAllowed = true },
            new { Roles = new[] { "Manager" }, ExpectedAllowed = true },
            new { Roles = new[] { "User" }, ExpectedAllowed = false },
            new { Roles = new[] { "Admin", "User" }, ExpectedAllowed = true },
            new { Roles = new string[0], ExpectedAllowed = false }
        };

        foreach (var testCase in testCases)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, "test-user") };
            foreach (var role in testCase.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            var identity = new ClaimsIdentity(claims, "test");
            httpContext.User = new ClaimsPrincipal(identity);

            var context = new GraphQLRequestContext();
            context.HttpContext = httpContext;

            var roleAuthMiddleware = new RoleBasedAuthorizationMiddleware(
                requiredRoles: new[] { "Admin", "Manager" });

            var finalDelegateCalled = false;

            GraphQLRequestDelegate finalDelegate = ctx =>
            {
                finalDelegateCalled = true;
                return Task.CompletedTask;
            };

            // Act
            await roleAuthMiddleware.Invoke(context, finalDelegate);

            // Assert
            Assert.Equal(testCase.ExpectedAllowed, finalDelegateCalled);
        }
    }

    [Fact]
    public async Task Middleware_ShouldHandlePermissionBasedAuthorization()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "test-user"),
            new Claim("permission", "read:users"),
            new Claim("permission", "write:posts"),
            new Claim("permission", "delete:comments")
        };
        var identity = new ClaimsIdentity(claims, "test");
        httpContext.User = new ClaimsPrincipal(identity);

        var context = new GraphQLRequestContext();
        context.HttpContext = httpContext;
        context.Request = new GraphQLRequest { Query = "{ users { id name } }" };

        var permissionMiddleware = new PermissionBasedAuthorizationMiddleware();
        var finalDelegateCalled = false;

        GraphQLRequestDelegate finalDelegate = ctx =>
        {
            finalDelegateCalled = true;
            return Task.CompletedTask;
        };

        // Act
        await permissionMiddleware.Invoke(context, finalDelegate);

        // Assert
        Assert.True(finalDelegateCalled);
    }

    [Fact]
    public async Task Middleware_ShouldHandleJwtTokenValidation()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "Bearer valid-jwt-token";

        var context = new GraphQLRequestContext();
        context.HttpContext = httpContext;

        var jwtValidationMiddleware = new JwtValidationMiddleware();
        var userWasSet = false;

        GraphQLRequestDelegate finalDelegate = ctx =>
        {
            userWasSet = ctx.HttpContext.User.Identity?.IsAuthenticated == true;
            return Task.CompletedTask;
        };

        // Act
        await jwtValidationMiddleware.Invoke(context, finalDelegate);

        // Assert
        Assert.True(userWasSet);
    }

    [Fact]
    public async Task Middleware_ShouldHandleInvalidJwtToken()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "Bearer invalid-jwt-token";

        var context = new GraphQLRequestContext();
        context.HttpContext = httpContext;

        var jwtValidationMiddleware = new JwtValidationMiddleware();
        var finalDelegateCalled = false;

        GraphQLRequestDelegate finalDelegate = ctx =>
        {
            finalDelegateCalled = true;
            return Task.CompletedTask;
        };

        // Act
        await jwtValidationMiddleware.Invoke(context, finalDelegate);

        // Assert
        Assert.False(finalDelegateCalled);
        
        // Check that error response was set
        await foreach (var result in context.Response)
        {
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Contains("Invalid token", result.Errors[0].Message);
            break;
        }
    }

    [Fact]
    public async Task Middleware_ShouldHandleApiKeyAuthentication()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-API-Key"] = "valid-api-key-123";

        var context = new GraphQLRequestContext();
        context.HttpContext = httpContext;

        var apiKeyMiddleware = new ApiKeyAuthenticationMiddleware();
        var userWasSet = false;

        GraphQLRequestDelegate finalDelegate = ctx =>
        {
            userWasSet = ctx.HttpContext.User.Identity?.IsAuthenticated == true;
            return Task.CompletedTask;
        };

        // Act
        await apiKeyMiddleware.Invoke(context, finalDelegate);

        // Assert
        Assert.True(userWasSet);
    }

    [Fact]
    public async Task Middleware_ShouldRateLimitPerUser()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.Name, "test-user") };
        var identity = new ClaimsIdentity(claims, "test");
        httpContext.User = new ClaimsPrincipal(identity);

        var context = new GraphQLRequestContext();
        context.HttpContext = httpContext;

        var rateLimitMiddleware = new UserRateLimitMiddleware(maxRequestsPerMinute: 2);
        
        // First request should succeed
        var firstRequestCalled = false;
        GraphQLRequestDelegate firstDelegate = ctx =>
        {
            firstRequestCalled = true;
            return Task.CompletedTask;
        };

        await rateLimitMiddleware.Invoke(context, firstDelegate);
        Assert.True(firstRequestCalled);

        // Second request should succeed
        var secondRequestCalled = false;
        GraphQLRequestDelegate secondDelegate = ctx =>
        {
            secondRequestCalled = true;
            return Task.CompletedTask;
        };

        await rateLimitMiddleware.Invoke(context, secondDelegate);
        Assert.True(secondRequestCalled);

        // Third request should be rate limited
        var thirdRequestCalled = false;
        GraphQLRequestDelegate thirdDelegate = ctx =>
        {
            thirdRequestCalled = true;
            return Task.CompletedTask;
        };

        await rateLimitMiddleware.Invoke(context, thirdDelegate);
        Assert.False(thirdRequestCalled);
        
        // Check that rate limit error was set
        await foreach (var result in context.Response)
        {
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Contains("Rate limit exceeded", result.Errors[0].Message);
            break;
        }
    }

    [Fact]
    public async Task Middleware_ShouldHandleComplexAuthorizationScenarios()
    {
        // Arrange: User with multiple roles and permissions
        var httpContext = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "complex-user"),
            new Claim(ClaimTypes.Email, "user@company.com"),
            new Claim(ClaimTypes.Role, "Employee"),
            new Claim(ClaimTypes.Role, "ProjectManager"),
            new Claim("department", "engineering"),
            new Claim("team", "backend"),
            new Claim("permission", "read:projects"),
            new Claim("permission", "write:projects"),
            new Claim("clearance", "confidential")
        };
        var identity = new ClaimsIdentity(claims, "test");
        httpContext.User = new ClaimsPrincipal(identity);

        var context = new GraphQLRequestContext();
        context.HttpContext = httpContext;
        context.Request = new GraphQLRequest { Query = "{ confidentialProjects { id name } }" };

        var complexAuthMiddleware = new ComplexAuthorizationMiddleware();
        var finalDelegateCalled = false;

        GraphQLRequestDelegate finalDelegate = ctx =>
        {
            finalDelegateCalled = true;
            return Task.CompletedTask;
        };

        // Act
        await complexAuthMiddleware.Invoke(context, finalDelegate);

        // Assert
        Assert.True(finalDelegateCalled);
    }

    // Test middleware implementations
    private class UserInfo
    {
        public bool IsAuthenticated { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? CustomClaim { get; set; }
    }

    private class AuthenticationMiddleware : IGraphQLRequestMiddleware
    {
        private readonly UserInfo _userInfo;

        public AuthenticationMiddleware(UserInfo userInfo)
        {
            _userInfo = userInfo;
        }

        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            var user = context.HttpContext.User;
            _userInfo.IsAuthenticated = user.Identity?.IsAuthenticated == true;

            if (_userInfo.IsAuthenticated)
            {
                _userInfo.Name = user.FindFirst(ClaimTypes.Name)?.Value;
                _userInfo.Email = user.FindFirst(ClaimTypes.Email)?.Value;
                _userInfo.CustomClaim = user.FindFirst("custom-claim")?.Value;
            }

            await next(context);
        }
    }

    private class AuthorizationMiddleware : IGraphQLRequestMiddleware
    {
        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            var user = context.HttpContext.User;
            var query = context.Request?.Query?.ToString();

            // Simple authorization logic for demo
            if (query?.Contains("adminOnlyField") == true)
            {
                if (!user.IsInRole("Admin"))
                {
                    context.Response = CreateErrorResponse("Unauthorized: Admin role required");
                    return;
                }
            }

            await next(context);
        }
    }

    private class RequireAuthenticationMiddleware : IGraphQLRequestMiddleware
    {
        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            var user = context.HttpContext.User;

            if (user.Identity?.IsAuthenticated != true)
            {
                context.Response = CreateErrorResponse("Authentication required");
                return;
            }

            await next(context);
        }
    }

    private class ClaimsValidationMiddleware : IGraphQLRequestMiddleware
    {
        private readonly Dictionary<string, string> _requiredClaims;

        public ClaimsValidationMiddleware(Dictionary<string, string> requiredClaims)
        {
            _requiredClaims = requiredClaims;
        }

        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            var user = context.HttpContext.User;

            foreach (var requiredClaim in _requiredClaims)
            {
                var claimValue = user.FindFirst(requiredClaim.Key)?.Value;
                if (claimValue != requiredClaim.Value)
                {
                    context.Response = CreateErrorResponse($"Insufficient claims: Missing or invalid {requiredClaim.Key}");
                    return;
                }
            }

            await next(context);
        }
    }

    private class RoleBasedAuthorizationMiddleware : IGraphQLRequestMiddleware
    {
        private readonly string[] _requiredRoles;

        public RoleBasedAuthorizationMiddleware(string[] requiredRoles)
        {
            _requiredRoles = requiredRoles;
        }

        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            var user = context.HttpContext.User;

            if (!_requiredRoles.Any(role => user.IsInRole(role)))
            {
                context.Response = CreateErrorResponse($"Unauthorized: Requires one of roles: {string.Join(", ", _requiredRoles)}");
                return;
            }

            await next(context);
        }
    }

    private class PermissionBasedAuthorizationMiddleware : IGraphQLRequestMiddleware
    {
        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            var user = context.HttpContext.User;
            var query = context.Request?.Query?.ToString();

            // Simple permission check for demo
            if (query?.Contains("users") == true)
            {
                var hasReadUsersPermission = user.FindAll("permission").Any(c => c.Value == "read:users");
                if (!hasReadUsersPermission)
                {
                    context.Response = CreateErrorResponse("Unauthorized: read:users permission required");
                    return;
                }
            }

            await next(context);
        }
    }

    private class JwtValidationMiddleware : IGraphQLRequestMiddleware
    {
        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            var authHeader = context.HttpContext.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                context.Response = CreateErrorResponse("Missing or invalid Authorization header");
                return;
            }

            var token = authHeader.Substring("Bearer ".Length);

            // Mock JWT validation
            if (token == "valid-jwt-token")
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, "jwt-user"),
                    new Claim(ClaimTypes.Email, "jwt@example.com")
                };
                var identity = new ClaimsIdentity(claims, "jwt");
                context.HttpContext.User = new ClaimsPrincipal(identity);
            }
            else
            {
                context.Response = CreateErrorResponse("Invalid token");
                return;
            }

            await next(context);
        }
    }

    private class ApiKeyAuthenticationMiddleware : IGraphQLRequestMiddleware
    {
        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            var apiKey = context.HttpContext.Request.Headers["X-API-Key"].ToString();

            if (string.IsNullOrEmpty(apiKey))
            {
                context.Response = CreateErrorResponse("Missing API key");
                return;
            }

            // Mock API key validation
            if (apiKey == "valid-api-key-123")
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, "api-client"),
                    new Claim("client-id", "client-123")
                };
                var identity = new ClaimsIdentity(claims, "apikey");
                context.HttpContext.User = new ClaimsPrincipal(identity);
            }
            else
            {
                context.Response = CreateErrorResponse("Invalid API key");
                return;
            }

            await next(context);
        }
    }

    private class UserRateLimitMiddleware : IGraphQLRequestMiddleware
    {
        private readonly int _maxRequestsPerMinute;
        private readonly Dictionary<string, List<DateTime>> _userRequests = new();

        public UserRateLimitMiddleware(int maxRequestsPerMinute)
        {
            _maxRequestsPerMinute = maxRequestsPerMinute;
        }

        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            var user = context.HttpContext.User;
            var userId = user.FindFirst(ClaimTypes.Name)?.Value ?? "anonymous";

            var now = DateTime.UtcNow;
            var oneMinuteAgo = now.AddMinutes(-1);

            if (!_userRequests.ContainsKey(userId))
            {
                _userRequests[userId] = new List<DateTime>();
            }

            var userRequestTimes = _userRequests[userId];
            
            // Remove old requests
            userRequestTimes.RemoveAll(time => time < oneMinuteAgo);

            if (userRequestTimes.Count >= _maxRequestsPerMinute)
            {
                context.Response = CreateErrorResponse("Rate limit exceeded");
                return;
            }

            userRequestTimes.Add(now);

            await next(context);
        }
    }

    private class ComplexAuthorizationMiddleware : IGraphQLRequestMiddleware
    {
        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            var user = context.HttpContext.User;
            var query = context.Request?.Query?.ToString();

            // Complex authorization logic
            if (query?.Contains("confidentialProjects") == true)
            {
                // Requires specific role AND department AND clearance
                var hasProjectManagerRole = user.IsInRole("ProjectManager");
                var isInEngineering = user.FindFirst("department")?.Value == "engineering";
                var hasConfidentialClearance = user.FindFirst("clearance")?.Value == "confidential";
                var hasReadProjectsPermission = user.FindAll("permission").Any(c => c.Value == "read:projects");

                if (!hasProjectManagerRole || !isInEngineering || !hasConfidentialClearance || !hasReadProjectsPermission)
                {
                    context.Response = CreateErrorResponse("Unauthorized: Insufficient privileges for confidential projects");
                    return;
                }
            }

            await next(context);
        }
    }

    // Helper method to create error responses
    private static async IAsyncEnumerable<ExecutionResult> CreateErrorResponse(string message)
    {
        yield return new ExecutionResult
        {
            Errors = new[] { new ExecutionError { Message = message } }
        };
        await Task.CompletedTask;
    }
}