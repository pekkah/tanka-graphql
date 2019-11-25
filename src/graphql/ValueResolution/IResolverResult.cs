﻿using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.ValueResolution
{
    public interface IResolverResult
    {
        ValueTask<object> CompleteValueAsync(
            IResolverContext context);
    }

    public interface IResolverResult3
    {
        object Value { get; }

        Task<object> CompleteValueAsync(IExecutorContext executorContext,
            ObjectType objectType,
            IField field,
            IType fieldType,
            GraphQLFieldSelection selection,
            IReadOnlyCollection<GraphQLFieldSelection> fields,
            NodePath path);
    }
}