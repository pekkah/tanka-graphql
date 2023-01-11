using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Experimental.Directives;

public delegate T? DirectiveNodeVisitor<T>(Directive directive, T node);