# Tanka GraphQL Apollo Federation Compatibility Sample

This sample demonstrates Tanka GraphQL's compatibility with Apollo Federation v2.3 specification. It implements the standard Apollo Federation subgraph compatibility test schema for testing federation features.

## Features

- ✅ Apollo Federation v2.3 with @link directive support
- ✅ Complete subgraph compatibility schema implementation
- ✅ Entity resolution with multiple @key strategies
- ✅ Advanced Federation directives (@shareable, @inaccessible, @override, @tag, etc.)
- ✅ Reference resolvers for entity federation
- ✅ Docker support for compatibility testing

## Quick Start

### Local Development

```bash
# Navigate to the sample directory
cd samples/GraphQL.Samples.ApolloFederation.Compatibility

# Run the application
dotnet run

# The GraphQL endpoint will be available at:
# http://localhost:4001/
```

### Docker

```bash
# Build and run with Docker Compose
docker-compose up --build

# The GraphQL endpoint will be available at:
# http://localhost:4001/
```

## Testing Federation Compatibility

This sample is designed to work with the Apollo Federation subgraph compatibility test suite:

### Installation

```bash
# Install the compatibility testing tool
npm install -g @apollo/federation-subgraph-compatibility
```

### Setup for Full Federation Testing

If you want to run the complete federation setup with multiple subgraphs:

```bash
# Option A: Create a symlink to the installed tests (recommended)
ln -sf $(npm root -g)/@apollo/federation-subgraph-compatibility/node_modules/@apollo/federation-subgraph-compatibility-tests ./federation-tests

# Option B: Set environment variable to point to your installation
export FEDERATION_TESTS_PATH="$(npm root -g)/@apollo/federation-subgraph-compatibility/node_modules/@apollo/federation-subgraph-compatibility-tests"
```

### Run Compatibility Tests

```bash
# Test the running subgraph for Apollo Federation compatibility
npx apollo-federation-subgraph-compatibility 4001
```

### Full Federation Setup with Docker

To run a complete federated graph with multiple subgraphs and Apollo Router:

```bash
# Ensure federation tests are set up (see Setup section above)
# Then start all services with Docker Compose
docker-compose -f supergraph-compose.yaml up
```

This will start:
- Products subgraph (Tanka GraphQL) on port 4001
- Inventory subgraph (Node.js) on port 4003  
- Users subgraph (Node.js) on port 4002
- Apollo Router on port 4000

## Endpoints

- **GraphQL**: `http://localhost:4001/` - Main GraphQL endpoint
- **Schema**: `http://localhost:4001/schema` - View the raw GraphQL schema
- **Health**: `http://localhost:4001/health` - Health check endpoint

## Sample Queries

### Product Query
```graphql
query {
  product(id: "apollo-federation") {
    id
    sku
    package
    dimensions {
      size
      weight
    }
    createdBy {
      email
      totalProductsCreated
    }
  }
}
```

### Federation Service Query
```graphql
query {
  _service {
    sdl
  }
}
```

### Entity Resolution Query
```graphql
query {
  _entities(representations: [
    { __typename: "Product", id: "apollo-studio" }
  ]) {
    ... on Product {
      id
      sku
      notes
    }
  }
}
```

## Schema Features

This sample implements all required Federation v2.3 features:

- **Multiple Keys**: Products can be resolved by `id`, `sku package`, or `sku variation { id }`
- **Entity Types**: Product, DeprecatedProduct, ProductResearch, Inventory
- **Advanced Directives**: @shareable, @inaccessible, @tag, @interfaceObject
- **Cross-Subgraph Relations**: User type extension with @provides
- **Deprecation**: Deprecated queries and fields

## Federation Architecture

```
┌─────────────────────┐
│   Users Subgraph    │ ← Provided by test framework
│   (External)        │
└─────────────────────┘
           │
           │ Federation
           │
┌─────────────────────┐
│  Products Subgraph  │ ← This Tanka GraphQL implementation
│  (This Sample)      │
└─────────────────────┘
           │
           │ Federation  
           │
┌─────────────────────┐
│ Inventory Subgraph  │ ← Provided by test framework
│   (External)        │
└─────────────────────┘
```

## Implementation Notes

- Uses Tanka GraphQL's native Federation v2.3 support with @link directives
- Implements proper entity resolution with reference resolvers
- Supports all Federation directive features required for compatibility
- Follows Apollo Federation subgraph specification exactly
- Ready for production federation environments