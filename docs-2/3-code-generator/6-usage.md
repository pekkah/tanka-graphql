## Usage with Tanka GraphQL Server

Generator will generate resolver mappings class which can be used with the schema builder to connect the resolvers to fields. Mappings use `Use<T>` extension method to get the controller of the object type from scoped `IServiceProvider` and execute the resolver method of the controller for the queried field.


### Bind generated resolver mappings to schema

Generated mapping class implements `IResolverMap` and `ISubscriberMap` interfaces. See example usage in getting started guide.


### Implementing generated controllers

See related documentation about code generation for each supported schema type.


### Adding controllers to `IServiceCollection`

When using Tanka GraphQL server `IServiceScope` is created for the duration of the operation. This allows scoping the controllers to singleton, operation (scoped), or per resolver (transient) lifetimes. Generator generates a service builder which can be used to register implementations of the generated controllers interfaces to scoped lifetime. If required this can be overriden by using the normal `IServiceCollection.Add{Lifetime}` methods.


#### Example

CRM.graphql
```graphql
type Query {
	search(q: String!): ContactSearchResult!
}

schema {
	query: Query
}

type Contact {
	firstName: String
	lastName: String
}

type ContactResults {
	contact: [Contact!]!
}

type SearchSuggestions {
	suggestions: [String!]!
}

union ContactSearchResult = ContactResults | SearchSuggestions 
```

Startup.cs#ConfigureServices
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Generated method name template Add{SchemaFileName}Controllers
    services.AddCRMControllers()
        .AddQueryController<QueryController>()
        .AddContactController<ContactController>()
        .AddContactResultsController<ContactResultsController>()
        .AddContactSearchResultController<ContactSearchResultController>()
        .AddSearchSuggestionsController<SearchSuggestionsController>();

    //...
}
```






