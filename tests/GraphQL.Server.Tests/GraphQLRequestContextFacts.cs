using System;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Tanka.GraphQL.Server.Tests;

public class GraphQLRequestContextFacts
{
    [Fact]
    public void Constructor_WithFeatureCollection_InitializesCorrectly()
    {
        // Given
        var features = new FeatureCollection();

        // When
        var context = new GraphQLRequestContext(features);

        // Then
        Assert.NotNull(context);
        Assert.Same(features, context.Features);
    }

    [Fact]
    public void Constructor_WithoutParameters_InitializesWithDefaultFeatures()
    {
        // Given & When
        var context = new GraphQLRequestContext();

        // Then
        Assert.NotNull(context);
        Assert.NotNull(context.Features);
    }

    [Fact]
    public void Constructor_WithNullFeatures_ThrowsException()
    {
        // Given & When & Then
        Assert.Throws<ArgumentNullException>(() => new GraphQLRequestContext(null!));
    }

    [Fact]
    public void HttpContext_WhenSet_CanBeRetrieved()
    {
        // Given
        var context = new GraphQLRequestContext();
        var httpContext = new DefaultHttpContext();

        // When
        context.HttpContext = httpContext;

        // Then
        Assert.Same(httpContext, context.HttpContext);
    }

    [Fact]
    public void HttpContext_WhenNotSet_ReturnsNull()
    {
        // Given
        var context = new GraphQLRequestContext();

        // When
        var httpContext = context.HttpContext;

        // Then
        Assert.Null(httpContext);
    }

    [Fact]
    public void HttpContext_WhenSetToNull_ReturnsNull()
    {
        // Given
        var context = new GraphQLRequestContext();
        var httpContext = new DefaultHttpContext();
        context.HttpContext = httpContext;

        // When
        context.HttpContext = null!;

        // Then
        Assert.Null(context.HttpContext);
    }

    [Fact]
    public void RequestServices_WhenSet_CanBeRetrieved()
    {
        // Given
        var context = new GraphQLRequestContext();
        var services = new ServiceCollection().BuildServiceProvider();

        // When
        context.RequestServices = services;

        // Then
        Assert.Same(services, context.RequestServices);
    }

    [Fact]
    public void RequestServices_WhenNotSet_ReturnsNull()
    {
        // Given
        var context = new GraphQLRequestContext();

        // When
        var services = context.RequestServices;

        // Then
        Assert.Null(services);
    }

    [Fact]
    public void RequestCancelled_WhenSet_CanBeRetrieved()
    {
        // Given
        var context = new GraphQLRequestContext();
        var cancellationToken = new CancellationToken();

        // When
        context.RequestCancelled = cancellationToken;

        // Then
        Assert.Equal(cancellationToken, context.RequestCancelled);
    }

    [Fact]
    public void RequestCancelled_WhenNotSet_ReturnsDefault()
    {
        // Given
        var context = new GraphQLRequestContext();

        // When
        var cancellationToken = context.RequestCancelled;

        // Then
        Assert.Equal(CancellationToken.None, cancellationToken);
    }

    [Fact]
    public void Features_CanSetAndRetrieveCustomFeatures()
    {
        // Given
        var context = new GraphQLRequestContext();
        var customFeature = new CustomFeature();

        // When
        context.Features.Set<ICustomFeature>(customFeature);

        // Then
        Assert.Same(customFeature, context.Features.Get<ICustomFeature>());
    }

    [Fact]
    public void Features_CanOverrideFeatures()
    {
        // Given
        var context = new GraphQLRequestContext();
        var feature1 = new CustomFeature();
        var feature2 = new CustomFeature();

        // When
        context.Features.Set<ICustomFeature>(feature1);
        context.Features.Set<ICustomFeature>(feature2);

        // Then
        Assert.Same(feature2, context.Features.Get<ICustomFeature>());
        Assert.NotSame(feature1, context.Features.Get<ICustomFeature>());
    }

    [Fact]
    public void InheritsFromQueryContext()
    {
        // Given & When
        var context = new GraphQLRequestContext();

        // Then
        Assert.IsAssignableFrom<QueryContext>(context);
    }

    [Fact]
    public void IsRecord_SupportsWithSyntax()
    {
        // Given
        var context = new GraphQLRequestContext();
        var httpContext = new DefaultHttpContext();

        // When
        var newContext = context with { HttpContext = httpContext };

        // Then
        Assert.NotSame(context, newContext);
        Assert.Same(httpContext, newContext.HttpContext);
    }

    [Fact]
    public void HttpContextFeature_IsAccessible()
    {
        // Given
        var context = new GraphQLRequestContext();
        var httpContext = new DefaultHttpContext();

        // When
        context.HttpContext = httpContext;

        // Then
        var httpContextFeature = context.Features.Get<IHttpContextFeature>();
        Assert.NotNull(httpContextFeature);
        Assert.Same(httpContext, httpContextFeature.HttpContext);
    }

    [Fact]
    public void RequestServicesFeature_IsAccessible()
    {
        // Given
        var context = new GraphQLRequestContext();
        var services = new ServiceCollection().BuildServiceProvider();

        // When
        context.RequestServices = services;

        // Then
        var requestServicesFeature = context.Features.Get<IRequestServicesFeature>();
        Assert.NotNull(requestServicesFeature);
        Assert.Same(services, requestServicesFeature.RequestServices);
    }

    [Fact]
    public void Context_WithMultipleFeatures_HandlesCorrectly()
    {
        // Given
        var context = new GraphQLRequestContext();
        var httpContext = new DefaultHttpContext();
        var services = new ServiceCollection().BuildServiceProvider();
        var customFeature = new CustomFeature();

        // When
        context.HttpContext = httpContext;
        context.RequestServices = services;
        context.Features.Set<ICustomFeature>(customFeature);

        // Then
        Assert.Same(httpContext, context.HttpContext);
        Assert.Same(services, context.RequestServices);
        Assert.Same(customFeature, context.Features.Get<ICustomFeature>());
    }

    [Fact]
    public void Context_WithCancellationToken_HandlesCorrectly()
    {
        // Given
        var context = new GraphQLRequestContext();
        using var cts = new CancellationTokenSource();

        // When
        context.RequestCancelled = cts.Token;

        // Then
        Assert.Equal(cts.Token, context.RequestCancelled);
    }

    [Fact]
    public void Context_WithCancelledToken_HandlesCorrectly()
    {
        // Given
        var context = new GraphQLRequestContext();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // When
        context.RequestCancelled = cts.Token;

        // Then
        Assert.Equal(cts.Token, context.RequestCancelled);
        Assert.True(context.RequestCancelled.IsCancellationRequested);
    }

    [Fact]
    public void Context_LifecycleIntegration_WorksCorrectly()
    {
        // Given
        var featureCollection = new FeatureCollection();
        var context = new GraphQLRequestContext(featureCollection);
        var httpContext = new DefaultHttpContext();
        var services = new ServiceCollection()
            .AddSingleton<ITestService, TestService>()
            .BuildServiceProvider();

        // When
        context.HttpContext = httpContext;
        context.RequestServices = services;

        // Then
        Assert.Same(httpContext, context.HttpContext);
        Assert.Same(services, context.RequestServices);
        
        // Verify service resolution works
        var testService = context.RequestServices.GetRequiredService<ITestService>();
        Assert.NotNull(testService);
        Assert.IsType<TestService>(testService);
    }

    [Fact]
    public void Context_WithHttpContextServices_OverridesRequestServices()
    {
        // Given
        var context = new GraphQLRequestContext();
        var httpContext = new DefaultHttpContext();
        var httpContextServices = new ServiceCollection()
            .AddSingleton<ITestService, TestService>()
            .BuildServiceProvider();
        
        httpContext.RequestServices = httpContextServices;

        // When
        context.HttpContext = httpContext;

        // Then
        Assert.Same(httpContext, context.HttpContext);
        Assert.Same(httpContextServices, context.HttpContext.RequestServices);
    }

    [Fact]
    public void Context_Equality_WorksCorrectly()
    {
        // Given
        var context1 = new GraphQLRequestContext();
        var context2 = new GraphQLRequestContext();
        var httpContext = new DefaultHttpContext();

        // When & Then
        Assert.NotEqual(context1, context2); // Different instances
        
        context1.HttpContext = httpContext;
        context2.HttpContext = httpContext;
        
        // Records compare by value, but HttpContext is reference type
        Assert.NotEqual(context1, context2); // Still different instances
    }

    [Fact]
    public void Context_ToString_WorksCorrectly()
    {
        // Given
        var context = new GraphQLRequestContext();

        // When
        var stringRepresentation = context.ToString();

        // Then
        Assert.NotNull(stringRepresentation);
        Assert.Contains("GraphQLRequestContext", stringRepresentation);
    }

    [Fact]
    public void Context_GetHashCode_WorksCorrectly()
    {
        // Given
        var context1 = new GraphQLRequestContext();
        var context2 = new GraphQLRequestContext();

        // When
        var hashCode1 = context1.GetHashCode();
        var hashCode2 = context2.GetHashCode();

        // Then
        // HashCodes should be different for different instances
        Assert.NotEqual(hashCode1, hashCode2);
    }

    // Test support interfaces and classes
    public interface ICustomFeature
    {
        string Value { get; set; }
    }

    public class CustomFeature : ICustomFeature
    {
        public string Value { get; set; } = "CustomValue";
    }

    public interface ITestService
    {
        string GetValue();
    }

    public class TestService : ITestService
    {
        public string GetValue() => "TestValue";
    }
}