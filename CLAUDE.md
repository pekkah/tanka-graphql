# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Tanka GraphQL is a comprehensive .NET GraphQL library providing a complete GraphQL implementation for .NET applications. The project includes:

- **GraphQL execution engine** with validation and type system
- **ASP.NET Core server** with HTTP/WebSocket transport
- **Source generators** for automatic schema type generation
- **Apollo Federation** subgraph support
- **Real-time subscriptions** via GraphQL over WebSocket (graphql-ws compatible)

## Development Commands

### Building and Testing
```bash
# Full build with tests and packaging
./build.ps1

# Build only (no tests or packaging)
./build.ps1 -OnlyBuild $True

# Restore tools and dependencies
dotnet tool restore
dotnet restore

# Build in Release configuration
dotnet build -c Release --no-restore

# Run all tests
dotnet test -c Release --no-restore --no-build

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test projects
dotnet test tests/GraphQL.Tests/
dotnet test tests/GraphQL.Server.Tests/
dotnet test tests/GraphQL.Language.Tests/
```

### Documentation and Benchmarks
```bash
# Build documentation
./build-docs.ps1

# Run performance benchmarks
./run-benchmarks.ps1
# Or manually: dotnet run --project benchmarks/GraphQL.benchmarks -c Release
```

### Package Management
```bash
# Create NuGet packages
dotnet pack -c Release

# Install/Update local tools
dotnet tool restore
dotnet tool update gitversion.tool
dotnet tool update tanka.docsgen

# Format code before committing
dotnet format

# Run single test
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"
```

## Architecture Overview

### Core Components

**Source Structure:**
- `src/GraphQL.Language/` - GraphQL parser, lexer, and AST (.NET Standard 2.0)
- `src/GraphQL/` - Core execution engine, validation, and type system
- `src/GraphQL.Server/` - HTTP/WebSocket server implementation with middleware pipeline
- `src/GraphQL.Server.SourceGenerators/` - Compile-time code generation from C# classes
- `src/GraphQL.Extensions.*/` - Extensions (Apollo Federation, Tracing, Experimental)

**Key Architectural Patterns:**
- **Middleware Pipeline** - V3 execution architecture with request/response middleware
- **Delegate-based Resolvers** - Field resolvers implemented as delegates
- **Source Generation** - Compile-time schema generation from C# classes
- **Subscription Model** - Real-time subscriptions via SignalR WebSockets

### Testing Structure

Tests mirror the source structure in `/tests/` directory:
- **Unit Tests** - Individual component testing (xUnit + NSubstitute)
- **Integration Tests** - End-to-end server functionality
- **Snapshot Tests** - Source generator output verification (Verify.XUnit)
- **Real-world Tests** - GitHub GraphQL schema parsing and validation

### Package Distribution

Published NuGet packages:
- `Tanka.GraphQL` - Core execution engine
- `Tanka.GraphQL.Language` - Parser and AST
- `Tanka.GraphQL.Server` - HTTP/WebSocket server
- `Tanka.GraphQL.Server.SourceGenerators` - Code generation
- `Tanka.GraphQL.Extensions.*` - Various extensions

## Development Workflow

### Code Organization
- **Namespace Convention**: `Tanka.{ProjectName}` (auto-generated from MSBuild project name)
- **Assembly Naming**: `Tanka.{ProjectName}` (configured in Directory.Build.props)
- **Target Framework**: .NET 9.0 (GraphQL.Language also targets .NET Standard 2.0 for broader compatibility)

### Version Management
- **GitVersion** for semantic versioning
- **Directory.Build.props** for shared MSBuild properties
- **Artifacts** output to `/artifacts/` directory

### Key Files to Understand
- `Directory.Build.props` - MSBuild shared properties and package metadata
- `build.ps1` - Main build script with version management
- `.config/dotnet-tools.json` - Local development tools (GitVersion, Tanka DocsGen)
- Test projects use xUnit with NSubstitute for mocking and Verify for snapshot testing

### Source Generator Development
When working with source generators (`GraphQL.Server.SourceGenerators`):
- Use `Verify.SourceGenerators` for snapshot testing
- Generated code snapshots are stored in `/Snapshots/` directories
- Tests validate both compilation and generated output correctness

### Apollo Federation
Federation-related code is in `GraphQL.Extensions.ApolloFederation` with support for:
- Subgraph schema generation
- Entity resolvers
- Federation directives (`@key`, `@external`, `@requires`, `@provides`)

## Documentation Guidelines

### Code Examples in Documentation
- **ALWAYS use `#include::xref://` for code examples** when the code can be a unit test
- This approach keeps examples synchronized with working code and ensures testability
- Examples are pulled from actual test files, guaranteeing they compile and work correctly
- Avoid inline code blocks for complex examples - prefer testable, referenced code
- Format: `#include::xref://project:TestFile.cs?s=ClassName.MethodName`

## Troubleshooting

### Common Development Issues

#### Build Failures
- **Version conflicts**: Run `dotnet tool restore` to ensure correct tool versions
- **Missing dependencies**: Run `dotnet restore` before building
- **Target framework errors**: Ensure .NET 9.0 SDK is installed

#### Test Issues
- **Test discovery problems**: Check project references and ensure test projects target correct framework
- **Snapshot test failures**: Review `/Snapshots/` directories for source generator output changes
- **Integration test failures**: Verify HTTP/WebSocket endpoints and middleware configuration

#### Source Generator Issues
- **Generated code not updating**: Clean and rebuild the solution
- **Compilation errors in generated code**: Check source generator input classes for proper attributes
- **Missing generated types**: Verify `Verify.SourceGenerators` snapshot tests are passing

#### Documentation Build Issues
- **DocsGen tool errors**: Update tool with `dotnet tool update tanka.docsgen`
- **Missing code examples**: Ensure referenced test methods exist and are accessible
- **Live reload not working**: Check WebSocket connection and port configuration

#### Federation Specific Issues
- **Entity resolution failures**: Verify `@key` directives and resolver implementations
- **Subgraph composition errors**: Check federation directive usage and schema compatibility
- **Gateway integration issues**: Validate subgraph schema registration and entity types

### Getting Help
- Check existing GitHub Issues for similar problems
- Review test files for usage examples
- Consult Apollo Federation documentation for federation-specific questions
- Use `dotnet --info` to verify SDK version and runtime information

## References and Links

- **Latest GraphQL Spec Draft**: https://spec.graphql.org/draft/