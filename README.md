# Tanka GraphQL

A comprehensive .NET GraphQL library providing a complete GraphQL implementation for .NET applications.

## ✨ Features

- **🚀 Execute queries, mutations and subscriptions** - Full GraphQL operation support
- **🔍 Comprehensive validation** - Built-in GraphQL specification compliance
- **⚡ Source code generation** - Generate schema types from C# classes with source generators
- **📝 Modern GraphQL parser** - Fast parser for executable and type system documents
- **🔗 Delegate-based resolvers** - Clean resolver implementation with middleware support
- **🏗️ Middleware pipelines** - Flexible execution architecture (v3)
- **🌐 ASP.NET Core server** - HTTP/WebSocket transport with real-time subscriptions
- **🔄 Apollo Federation** - Subgraph support for distributed GraphQL architectures
- **📡 Real-time subscriptions** - [graphql-ws](https://github.com/enisdenjo/graphql-ws) compatible WebSocket server
- **⏭️ Incremental delivery** - @defer and @stream directives for progressive data loading
- **🎯 @oneOf directive** - Polymorphic input types support (Stage 3 RFC)

## 📖 Documentation

- **[Getting Started Guide](https://pekkah.github.io/tanka-graphql/)**
- **[API Documentation](https://pekkah.github.io/tanka-graphql/)**
- **[Samples & Examples](https://github.com/pekkah/tanka-graphql/tree/main/samples)**

## 📦 Packages

[![Nuget](https://img.shields.io/nuget/v/tanka.graphql?style=flat-square)](https://www.nuget.org/packages/Tanka.GraphQL/)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/tanka.graphql?style=flat-square)](https://www.nuget.org/packages/Tanka.GraphQL/)

| Package | Description |
|---------|-------------|
| [`Tanka.GraphQL`](https://www.nuget.org/packages/Tanka.GraphQL/) | Core execution engine |
| [`Tanka.GraphQL.Language`](https://www.nuget.org/packages/Tanka.GraphQL.Language/) | Parser and AST |
| [`Tanka.GraphQL.Server`](https://www.nuget.org/packages/Tanka.GraphQL.Server/) | ASP.NET Core server |
| [`Tanka.GraphQL.Server.SourceGenerators`](https://www.nuget.org/packages/Tanka.GraphQL.Server.SourceGenerators/) | Code generation |

## 🎯 Examples

**Quick Server Setup:**
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddTankaGraphQL()
    .AddHttp()
    .AddWebSockets()
    .AddSchema("MyAPI", schema => {
        schema.Add("Query", new FieldsWithResolvers {
            { "hello: String!", () => "Hello World!" }
        });
    });

var app = builder.Build();
app.UseWebSockets();
app.MapTankaGraphQL("/graphql", "MyAPI");
app.Run();
```

**Advanced Examples:**
- [**HTTP & WebSocket Server**](https://github.com/pekkah/tanka-graphql/tree/main/samples/GraphQL.Samples.Http) - Complete server setup
- [**Source Generation**](https://github.com/pekkah/tanka-graphql/tree/main/samples/GraphQL.Samples.SG.Basic) - Code-first development
- [**Apollo Federation**](https://github.com/pekkah/tanka-graphql/tree/main/dev/GraphQL.Dev.Reviews) - Microservice architecture
- [**Incremental Delivery**](https://github.com/pekkah/tanka-graphql/tree/main/samples/GraphQL.Samples.Defer) - @defer/@stream examples
- [**Tanka Chat**](https://github.com/pekkah/tanka-graphql-samples) - Real-world application


## 🚀 Quick Start

### Installation

**Essential packages:**
```bash
dotnet add package Tanka.GraphQL          # Core GraphQL execution
dotnet add package Tanka.GraphQL.Server   # ASP.NET Core server
```

**Optional packages:**
```bash
dotnet add package Tanka.GraphQL.Server.SourceGenerators    # Code generation
dotnet add package Tanka.GraphQL.Extensions.ApolloFederation # Federation support
```

### Basic Usage

1. **Create a simple GraphQL server:**
   ```csharp
   var builder = WebApplication.CreateBuilder(args);
   
   builder.AddTankaGraphQL()
       .AddHttp()
       .AddSchema("Demo", schema => {
           schema.Add("Query", new FieldsWithResolvers {
               { "greeting(name: String!): String!", (string name) => $"Hello, {name}!" }
           });
       });
   
   var app = builder.Build();
   app.MapTankaGraphQL("/graphql", "Demo");
   app.Run();
   ```

2. **Test your GraphQL endpoint:**
   ```bash
   curl -X POST http://localhost:5000/graphql \
     -H "Content-Type: application/json" \
     -d '{"query": "{ greeting(name: \"World\") }"}'
   ```

## 🏋️ Performance

Run benchmarks to see Tanka GraphQL performance:

```bash
cd benchmarks/GraphQL.Benchmarks
dotnet run --configuration Release --framework net9.0
```

## 🔧 Requirements

- **.NET 9.0** or later
- **ASP.NET Core 9.0** (for server features)

## 🤝 Contributing

We welcome contributions! Please see our [contribution guidelines](CONTRIBUTING.md) for details.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
