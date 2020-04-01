using System;
using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL.SchemaBuilding
{
    public partial class SchemaBuilder
    {
        public ISchema Build(bool validate = true)
        {
            if (validate)
            {
                var validationResult = Validate();
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult);
                }
            }

            var (fields, inputFields, resolvers, subscribers) = _connections.Build();
            return new SchemaGraph(
                _types,
                fields,
                inputFields,
                _directives,
                BuildResolvers(resolvers),
                BuildSubscribers(subscribers),
                _valueConverters,
                QueryTypeName,
                MutationTypeName,
                SubscriptionTypeName,
                _schemaDirectives);
        }

        public (ISchema Schema, ValidationResult ValidationResult) BuildAndValidate()
        {
            var validationResult = Validate();

            if (!validationResult.IsValid)
                return (null, validationResult);

            var schema = Build();
            return (schema, new ValidationResult());
        }

        private ValidationResult Validate()
        {
            var errors = new List<ValidationError>();

            // validate serializers exists for scalar types
            foreach (var scalarType in GetTypes<ScalarType>())
            {
                if (!TryGetValueConverter(scalarType.Name, out _))
                {
                    errors.Add(new ValidationError(
                        "SCHEMA_VALUE_CONVERTER_MISSING",
                        $"Could not find value converter for type '{scalarType.Name}'",
                        Enumerable.Empty<INode>()));
                }
            }

            // validate query root
            if (!TryGetQuery(out _))
            {
                errors.Add(new ValidationError(
                    "SCHEMA_QUERY_ROOT_MISSING",
                    $"Could not find Query root",
                    Enumerable.Empty<INode>()));
            }

            return new ValidationResult()
            {
                Errors = errors
            };
        }

        private IReadOnlyDictionary<string, Dictionary<string, Subscriber>> BuildSubscribers(
            IReadOnlyDictionary<string, Dictionary<string, SubscriberBuilder>> subscribers)
        {
            var result = new Dictionary<string, Dictionary<string, Subscriber>>();

            foreach (var type in subscribers)
                result[type.Key] = type.Value.Select(f => (f.Key, f.Value.Build()))
                    .ToDictionary(f => f.Key, f => f.Item2);

            return result;
        }

        private IReadOnlyDictionary<string, Dictionary<string, Resolver>> BuildResolvers(
            IReadOnlyDictionary<string, Dictionary<string, ResolverBuilder>> resolvers)
        {
            var result = new Dictionary<string, Dictionary<string, Resolver>>();

            foreach (var type in resolvers)
                result[type.Key] = type.Value.Select(f => (f.Key, f.Value.Build()))
                    .ToDictionary(f => f.Key, f => f.Item2);

            return result;
        }
    }
}