using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using Tanka.GraphQL.Features;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.ValueSerialization;

namespace Tanka.GraphQL.ValueResolution;

public interface IValueCompletionFeature
{
    ValueTask CompleteValueAsync(
        ResolverContext context,
        TypeBase fieldType,
        NodePath path);

    ValueTask CompleteValueAsync(
        ResolverContext context,
        TypeBase fieldType,
        NodePath path,
        int initialCount,
        string? label);
}

public class ValueCompletionFeature : IValueCompletionFeature
{
    public async ValueTask CompleteValueAsync(
        ResolverContext context,
        TypeBase fieldType,
        NodePath path)
    {
        context.CompletedValue = await CompleteValueAsync(context.ResolvedValue, fieldType, context, path);
    }

    public async ValueTask CompleteValueAsync(
        ResolverContext context,
        TypeBase fieldType,
        NodePath path,
        int initialCount,
        string? label)
    {
        context.CompletedValue = await CompleteValueWithStreamAsync(context.ResolvedValue, fieldType, context, path, initialCount, label);
    }

    public ValueTask<object?> CompleteValueAsync(
        object? value,
        TypeBase fieldType,
        ResolverContext context,
        NodePath path)
    {
        if (fieldType is NonNullType nonNullType)
            return CompleteNonNullTypeValueAsync(value, nonNullType, path, context);

        if (value == null)
            return default;

        if (fieldType is ListType list) return CompleteListValueAsync(value, list, path, context);

        if (fieldType is not NamedType namedType)
            throw new InvalidOperationException("FieldType is not NamedType");

        var typeDefinition = context.QueryContext.Schema.GetRequiredNamedType<TypeDefinition>(namedType.Name);
        return typeDefinition switch
        {
            ScalarDefinition scalarType => CompleteScalarType(value, scalarType, context),
            EnumDefinition enumType => CompleteEnumType(value, enumType, context),
            ObjectDefinition objectDefinition => CompleteObjectValueAsync(value, objectDefinition, path, context),

            InterfaceDefinition interfaceType => CompleteInterfaceValueAsync(value, interfaceType, path, context),
            UnionDefinition unionDefinition => CompleteUnionValueAsync(value, unionDefinition, path, context),
            _ => throw new FieldException(
                $"Cannot complete value for field {context.Field.Name}. Cannot complete value of type {Printer.Print(fieldType)}.")
            {
                Path = context.Path,
                Field = context.Field,
                Selection = context.Selection,
                ObjectDefinition = context.ObjectDefinition
            }
        };
    }

    private ValueTask<object?> CompleteEnumType(object? value, EnumDefinition enumType, ResolverContext context)
    {
        //todo: use similar pattern to scalars
        return new(new EnumConverter(enumType).Serialize(value));
    }

    private ValueTask<object?> CompleteScalarType(object? value, ScalarDefinition scalarType, ResolverContext context)
    {
        var converter = context.QueryContext.Schema.GetRequiredValueConverter(scalarType.Name);
        return new(converter.Serialize(value));
    }

    private async ValueTask<object?> CompleteUnionValueAsync(
        object value,
        UnionDefinition unionDefinition,
        NodePath path,
        ResolverContext context)
    {
        var actualType = context.ResolveAbstractType?.Invoke(unionDefinition, value) as ObjectDefinition;

        if (actualType == null)
            throw new FieldException(
                $"Cannot complete value for field '{Printer.Print(context.Field)}'. " +
                "ActualType is required for union values.")
            {
                Path = context.Path,
                Field = context.Field,
                Selection = context.Selection,
                ObjectDefinition = context.ObjectDefinition
            };

        if (!unionDefinition.HasMember(actualType.Name))
            throw new FieldException(
                $"Cannot complete value for field '{Printer.Print(context.Field)}'. " +
                $"ActualType '{actualType.Name}' is not possible for '{unionDefinition.Name}'"
            )
            {
                Path = context.Path,
                Field = context.Field,
                Selection = context.Selection,
                ObjectDefinition = context.ObjectDefinition
            };

        var subSelectionSet = SelectionSetExtensions.MergeSelectionSets(context.Fields);
        var data = await context.QueryContext.ExecuteSelectionSet(
            subSelectionSet,
            actualType,
            value,
            path).ConfigureAwait(false);

        return data;
    }

    private async ValueTask<object?> CompleteInterfaceValueAsync(
        object value,
        InterfaceDefinition interfaceType,
        NodePath path,
        ResolverContext context)
    {
        var actualType = context.ResolveAbstractType?.Invoke(interfaceType, value) as ObjectDefinition;

        if (actualType == null)
            throw new FieldException(
                $"Cannot complete value for field '{Printer.Print(context.Field)}'. " +
                "ActualType is required for interface values."
            )
            {
                Path = context.Path,
                Field = context.Field,
                Selection = context.Selection,
                ObjectDefinition = context.ObjectDefinition
            };

        if (!actualType.HasInterface(interfaceType.Name))
            throw new FieldException(
                $"Cannot complete value for field '{Printer.Print(context.Field)}'. " +
                $"ActualType '{actualType.Name}' does not implement interface '{interfaceType.Name}'"
            )
            {
                Path = context.Path,
                Field = context.Field,
                Selection = context.Selection,
                ObjectDefinition = context.ObjectDefinition
            };

        var subSelectionSet = SelectionSetExtensions.MergeSelectionSets(context.Fields);
        var data = await context.QueryContext.ExecuteSelectionSet(
            subSelectionSet,
            actualType,
            value,
            path);

        return data;
    }

    private static async ValueTask<object?> CompleteObjectValueAsync(
        object value,
        ObjectDefinition objectDefinition,
        NodePath path,
        ResolverContext context)
    {
        var subSelectionSet = SelectionSetExtensions.MergeSelectionSets(context.Fields);
        var data = await context.QueryContext.ExecuteSelectionSet(
            subSelectionSet,
            objectDefinition,
            value,
            path);

        return data;
    }

    private async ValueTask<object?> CompleteNonNullTypeValueAsync(
        object? value,
        NonNullType nonNullType,
        NodePath path,
        ResolverContext context)
    {
        var innerType = nonNullType.OfType;
        var completedResult = await CompleteValueAsync(value, innerType, context, path);

        if (completedResult == null)
            throw new FieldException(
                $"Cannot complete value for field '{Printer.Print(context.Field)}'. " +
                "Completed value would be null for non-null field"
            )
            {
                Path = context.Path,
                Field = context.Field,
                Selection = context.Selection,
                ObjectDefinition = context.ObjectDefinition
            };

        return completedResult;
    }

    private async ValueTask<object?> CompleteListValueAsync(
        object value,
        ListType list,
        NodePath path,
        ResolverContext context)
    {
        if (value is not IEnumerable values)
            throw new FieldException(
                $"Cannot complete value for list field '{context.Field.Name}':'{list}'. " +
                "Resolved value is not a collection"
            )
            {
                Path = context.Path,
                Field = context.Field,
                Selection = context.Selection,
                ObjectDefinition = context.ObjectDefinition
            };

        var innerType = list.OfType;
        var result = new List<object?>();
        var i = 0;
        foreach (var resultItem in values)
        {
            var itemPath = path.Fork().Append(i++);

            try
            {
                var completedResultItem = await CompleteValueAsync(
                    resultItem,
                    innerType,
                    context,
                    itemPath);

                result.Add(completedResultItem);
            }
            catch (Exception e)
            {
                if (innerType is NonNullType) throw;

                context.QueryContext.AddError(e);
                result.Add(null);
            }
        }

        return result;
    }

    public ValueTask<object?> CompleteValueWithStreamAsync(
        object? value,
        TypeBase fieldType,
        ResolverContext context,
        NodePath path,
        int initialCount,
        string? label)
    {
        if (fieldType is NonNullType nonNullType)
            return CompleteNonNullTypeWithStreamAsync(value, nonNullType, path, context, initialCount, label);

        if (value == null)
            return default;

        if (fieldType is ListType list)
            return CompleteListValueWithStreamAsync(value, list, path, context, initialCount, label);

        // For non-list types, use normal completion
        if (fieldType is not NamedType namedType)
            throw new InvalidOperationException("FieldType is not NamedType");

        var typeDefinition = context.QueryContext.Schema.GetRequiredNamedType<TypeDefinition>(namedType.Name);
        return typeDefinition switch
        {
            ScalarDefinition scalarType => CompleteScalarType(value, scalarType, context),
            EnumDefinition enumType => CompleteEnumType(value, enumType, context),
            ObjectDefinition objectDefinition => CompleteObjectValueAsync(value, objectDefinition, path, context),
            InterfaceDefinition interfaceType => CompleteInterfaceValueAsync(value, interfaceType, path, context),
            UnionDefinition unionDefinition => CompleteUnionValueAsync(value, unionDefinition, path, context),
            _ => throw new FieldException(
                $"Cannot complete value for field {context.Field.Name}. Cannot complete value of type {Printer.Print(fieldType)}.")
            {
                Path = context.Path,
                Field = context.Field,
                Selection = context.Selection,
                ObjectDefinition = context.ObjectDefinition
            }
        };
    }

    private async ValueTask<object?> CompleteNonNullTypeWithStreamAsync(
        object? value,
        NonNullType nonNullType,
        NodePath path,
        ResolverContext context,
        int initialCount,
        string? label)
    {
        var innerType = nonNullType.OfType;
        var completedResult = await CompleteValueWithStreamAsync(value, innerType, context, path, initialCount, label);

        if (completedResult == null)
            throw new FieldException(
                $"Cannot complete value for field '{Printer.Print(context.Field)}'. " +
                "Completed value would be null for non-null field"
            )
            {
                Path = context.Path,
                Field = context.Field,
                Selection = context.Selection,
                ObjectDefinition = context.ObjectDefinition
            };

        return completedResult;
    }

    private async ValueTask<object?> CompleteListValueWithStreamAsync(
        object value,
        ListType list,
        NodePath path,
        ResolverContext context,
        int initialCount,
        string? label)
    {
        var innerType = list.OfType;

        // Check for IAsyncEnumerable<T> FIRST for true streaming
        if (TryCompleteAsyncEnumerableStream(value, innerType, path, context, initialCount, label, out var asyncResult))
        {
            return await asyncResult.Value;
        }

        // Then check for regular IEnumerable
        if (value is not IEnumerable values)
            throw new FieldException(
                $"Cannot complete value for list field '{context.Field.Name}':'{list}'. " +
                "Resolved value is not a collection"
            )
            {
                Path = context.Path,
                Field = context.Field,
                Selection = context.Selection,
                ObjectDefinition = context.ObjectDefinition
            };

        // Handle regular IEnumerable with streaming
        return await CompleteEnumerableWithStream(values, innerType, path, context, initialCount, label);
    }

    private bool TryCompleteAsyncEnumerableStream(
        object value,
        TypeBase innerType,
        NodePath path,
        ResolverContext context,
        int initialCount,
        string? label,
        out ValueTask<object?>? result)
    {
        result = null;

        // Check if value implements any IAsyncEnumerable<T>
        var valueType = value.GetType();
        var asyncEnumerableInterface = valueType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>));

        if (asyncEnumerableInterface != null)
        {
            // Get the item type from IAsyncEnumerable<T>
            var itemType = asyncEnumerableInterface.GetGenericArguments()[0];
            
            // Get or create the compiled delegate for this type
            var streamMethod = AsyncEnumerableStreamCache.GetOrCreateStreamMethod(itemType);
            
            // Invoke the compiled delegate
            result = streamMethod(this, value, innerType, path, context, initialCount, label);
            return true;
        }

        return false;
    }

    // Cache for compiled async enumerable streaming methods
    private static class AsyncEnumerableStreamCache
    {
        private static readonly ConcurrentDictionary<Type, Func<ValueCompletionFeature, object, TypeBase, NodePath, ResolverContext, int, string?, ValueTask<object?>>> 
            Cache = new();

        public static Func<ValueCompletionFeature, object, TypeBase, NodePath, ResolverContext, int, string?, ValueTask<object?>> 
            GetOrCreateStreamMethod(Type itemType)
        {
            return Cache.GetOrAdd(itemType, CreateStreamMethod);
        }

        private static Func<ValueCompletionFeature, object, TypeBase, NodePath, ResolverContext, int, string?, ValueTask<object?>> 
            CreateStreamMethod(Type itemType)
        {
            // Get the generic method definition
            var methodInfo = typeof(ValueCompletionFeature)
                .GetMethod(nameof(CompleteAsyncEnumerableStreamGenericAsync), 
                    BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(itemType);

            // Create parameters for the expression
            var instanceParam = Expression.Parameter(typeof(ValueCompletionFeature), "instance");
            var valueParam = Expression.Parameter(typeof(object), "value");
            var innerTypeParam = Expression.Parameter(typeof(TypeBase), "innerType");
            var pathParam = Expression.Parameter(typeof(NodePath), "path");
            var contextParam = Expression.Parameter(typeof(ResolverContext), "context");
            var initialCountParam = Expression.Parameter(typeof(int), "initialCount");
            var labelParam = Expression.Parameter(typeof(string), "label");

            // Create the method call expression
            var callExpr = Expression.Call(
                instanceParam,
                methodInfo,
                Expression.Convert(valueParam, typeof(IAsyncEnumerable<>).MakeGenericType(itemType)),
                innerTypeParam,
                pathParam,
                contextParam,
                initialCountParam,
                labelParam);

            // Compile the expression to a delegate
            var lambda = Expression.Lambda<Func<ValueCompletionFeature, object, TypeBase, NodePath, ResolverContext, int, string?, ValueTask<object?>>>(
                callExpr,
                instanceParam,
                valueParam,
                innerTypeParam,
                pathParam,
                contextParam,
                initialCountParam,
                labelParam);

            return lambda.Compile();
        }
    }

    private async ValueTask<object?> CompleteAsyncEnumerableStreamGenericAsync<T>(
        IAsyncEnumerable<T> asyncEnumerable,
        TypeBase innerType,
        NodePath path,
        ResolverContext context,
        int initialCount,
        string? label)
    {
        var initialItems = new List<object?>();
        var itemIndex = 0;

        await using var enumerator = asyncEnumerable.GetAsyncEnumerator(context.QueryContext.RequestCancelled);

        // Collect initial items for immediate response
        while (itemIndex < initialCount && await enumerator.MoveNextAsync())
        {
            var current = enumerator.Current;
            var itemPath = path.Fork().Append(itemIndex);

            try
            {
                var completedResultItem = await CompleteValueAsync(
                    current,
                    innerType,
                    context,
                    itemPath);

                initialItems.Add(completedResultItem);
            }
            catch (Exception e)
            {
                if (innerType is NonNullType) throw;

                context.QueryContext.AddError(e);
                initialItems.Add(null);
            }

            itemIndex++;
        }

        // Check if there are more items to stream
        if (await enumerator.MoveNextAsync())
        {
            // Set up incremental delivery feature
            var incrementalFeature = context.QueryContext.Features.Get<IIncrementalDeliveryFeature>() ??
                                   new DefaultIncrementalDeliveryFeature();
            context.QueryContext.Features.Set<IIncrementalDeliveryFeature>(incrementalFeature);

            // Register remaining items as deferred work for true streaming
            RegisterRemainingAsyncItems(enumerator, innerType, path, context, itemIndex, label, incrementalFeature);
        }
        else
        {
            // No more items, dispose the enumerator
            await enumerator.DisposeAsync();
        }

        return initialItems;
    }

    private async ValueTask<object?> CompleteEnumerableWithStream(
        IEnumerable values,
        TypeBase innerType,
        NodePath path,
        ResolverContext context,
        int initialCount,
        string? label)
    {
        // First convert to list for indexing
        var itemList = values.Cast<object?>().ToList();
        var initialItems = new List<object?>();

        // Complete initial items
        for (int i = 0; i < Math.Min(initialCount, itemList.Count); i++)
        {
            var itemPath = path.Fork().Append(i);

            try
            {
                var completedResultItem = await CompleteValueAsync(
                    itemList[i],
                    innerType,
                    context,
                    itemPath);

                initialItems.Add(completedResultItem);
            }
            catch (Exception e)
            {
                if (innerType is NonNullType) throw;

                context.QueryContext.AddError(e);
                initialItems.Add(null);
            }
        }

        // Register remaining items as deferred work
        if (initialCount < itemList.Count)
        {
            var incrementalFeature = context.QueryContext.Features.Get<IIncrementalDeliveryFeature>() ??
                                   new DefaultIncrementalDeliveryFeature();
            context.QueryContext.Features.Set<IIncrementalDeliveryFeature>(incrementalFeature);

            RegisterRemainingListItems(itemList, initialCount, innerType, path, context, label, incrementalFeature);
        }

        return initialItems;
    }

    private void RegisterRemainingAsyncItems<T>(
        IAsyncEnumerator<T> enumerator,
        TypeBase innerType,
        NodePath path,
        ResolverContext context,
        int startIndex,
        string? label,
        IIncrementalDeliveryFeature incrementalFeature)
    {
        // Process the current item that we already read
        var currentItem = (object?)enumerator.Current;
        var currentPath = path.Fork().Append(startIndex);

        incrementalFeature.RegisterDeferredWork(label, currentPath, async () =>
        {
            var completedItem = await CompleteValueAsync(
                currentItem,
                innerType,
                context,
                currentPath);

            return new IncrementalPayload
            {
                Path = currentPath,
                Items = new[] { completedItem },
                Label = label
            };
        });

        // Register each subsequent item with slight delay for true streaming
        var itemIndex = startIndex + 1;
        Task.Run(async () =>
        {
            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    var current = (object?)enumerator.Current;
                    var streamPath = path.Fork().Append(itemIndex);

                    // Capture variables for closure
                    var capturedCurrent = current;
                    var capturedPath = streamPath;

                    // Yield control to allow other tasks to run between item processing
                    await Task.Yield();

                    incrementalFeature.RegisterDeferredWork(label, capturedPath, async () =>
                    {
                        var completedItem = await CompleteValueAsync(
                            capturedCurrent,
                            innerType,
                            context,
                            capturedPath);

                        return new IncrementalPayload
                        {
                            Path = capturedPath,
                            Items = new[] { completedItem },
                            Label = label
                        };
                    });

                    itemIndex++;
                }
            }
            catch (Exception ex)
            {
                incrementalFeature.RegisterDeferredWork(label, path, async () =>
                {
                    return new IncrementalPayload
                    {
                        Path = path,
                        Label = label,
                        Errors = new[] { new ExecutionError { Message = ex.Message, Path = path.Segments.ToArray() } }
                    };
                });
            }
            finally
            {
                await enumerator.DisposeAsync();
            }
        }, context.QueryContext.RequestCancelled);
    }

    private void RegisterRemainingListItems(
        IList<object?> itemList,
        int initialCount,
        TypeBase innerType,
        NodePath path,
        ResolverContext context,
        string? label,
        IIncrementalDeliveryFeature incrementalFeature)
    {
        for (int i = initialCount; i < itemList.Count; i++)
        {
            var itemIndex = i; // Capture for closure
            var streamPath = path.Fork().Append(itemIndex);

            incrementalFeature.RegisterDeferredWork(label, streamPath, async () =>
            {
                var completedItem = await CompleteValueAsync(
                    itemList[itemIndex],
                    innerType,
                    context,
                    streamPath);

                return new IncrementalPayload
                {
                    Path = streamPath,
                    Items = new[] { completedItem },
                    Label = label
                };
            });
        }
    }
}