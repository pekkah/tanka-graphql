using System;
using System.Collections.Generic;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.SchemaBuilding
{
    public partial class ConnectionBuilder
    {
        private readonly Dictionary<string, Dictionary<string, IField>> _fields =
            new Dictionary<string, Dictionary<string, IField>>();

        private readonly Dictionary<string, Dictionary<string, InputObjectField>> _inputFields =
            new Dictionary<string, Dictionary<string, InputObjectField>>();

        private readonly Dictionary<string, Dictionary<string, ResolverBuilder>> _resolvers =
            new Dictionary<string, Dictionary<string, ResolverBuilder>>();

        private readonly Dictionary<string, Dictionary<string, SubscriberBuilder>> _subscribers =
            new Dictionary<string, Dictionary<string, SubscriberBuilder>>();

        public ConnectionBuilder(SchemaBuilder builder)
        {
            Builder = builder;
        }

        public SchemaBuilder Builder { get; }

        public ConnectionBuilder Field(
            ComplexType owner,
            string fieldName,
            IType to,
            string? description = null,
            Action<ResolverBuilder>? resolve = null,
            Action<SubscriberBuilder>? subscribe = null,
            IEnumerable<DirectiveInstance>? directives = null,
            Action<ArgsBuilder>? args = null)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));
            if (to == null) throw new ArgumentNullException(nameof(to));

            if (!Builder.TryGetType<ComplexType>(owner.Name, out _))
                throw new SchemaBuilderException(owner.Name,
                    $"Cannot add Field. Owner type {owner.Name} is not known for {fieldName}.");

            var target = to.Unwrap();
            if (!Builder.TryGetType<INamedType>(target.Name, out _))
                throw new SchemaBuilderException(owner.Name,
                    $"Cannot add Field '{fieldName} to {owner.Name}'. Target type {target.Name} is not known.");

            if (!_fields.ContainsKey(owner.Name))
                _fields[owner.Name] = new Dictionary<string, IField>();

            if (_fields[owner.Name].ContainsKey(fieldName))
                throw new SchemaBuilderException(owner.Name,
                    $"Cannot add field '{fieldName}'. Type '{owner.Name}' already has field with same name.");

            var argsBuilder = new ArgsBuilder();
            args?.Invoke(argsBuilder);

            var field = new Field(
                to,
                argsBuilder.Build(),
                description,
                null,
                directives);

            _fields[owner.Name].Add(fieldName, field);

            if (resolve != null)
            {
                var resolver = Resolver(owner, fieldName);
                resolve(resolver);
            }

            if (subscribe != null)
            {
                var subscriber = Subscriber(owner, fieldName);
                subscribe(subscriber);
            }

            return this;
        }

        public ConnectionBuilder InputField(
            InputObjectType owner,
            string fieldName,
            IType to,
            object? defaultValue = null,
            string? description = null,
            IEnumerable<DirectiveInstance>? directives = null)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));
            if (to == null) throw new ArgumentNullException(nameof(to));

            if (!Builder.TryGetType<InputObjectType>(owner.Name, out _))
                throw new SchemaBuilderException(owner.Name,
                    $"Cannot add InputField. Owner type {owner.Name} is not known for {fieldName}.");

            var target = to.Unwrap();
            if (!Builder.TryGetType<INamedType>(target.Name, out _))
                throw new SchemaBuilderException(owner.Name,
                    $"Cannot add Field '{fieldName} to {owner.Name}'. Target type {target.Name} is not known.");

            if (!_inputFields.ContainsKey(owner.Name))
                _inputFields[owner.Name] = new Dictionary<string, InputObjectField>();

            if (_inputFields[owner.Name].ContainsKey(fieldName))
                throw new SchemaBuilderException(owner.Name,
                    $"Cannot add input field '{fieldName}'. Type '{owner.Name}' already has field with same name.");

            _inputFields[owner.Name].Add(
                fieldName,
                new InputObjectField(to, description, defaultValue, directives));

            return this;
        }

        public ResolverBuilder Resolver(ComplexType owner, string fieldName)
        {
            if (!_resolvers.ContainsKey(owner.Name))
                _resolvers[owner.Name] = new Dictionary<string, ResolverBuilder>();

            if (_resolvers[owner.Name].ContainsKey(fieldName))
                throw new SchemaBuilderException(owner.Name,
                    $"Cannot add resolver for '{fieldName}'. Resolver has been already created. Use {nameof(GetOrAddResolver)}.");

            var builder = new ResolverBuilder();
            _resolvers[owner.Name].Add(fieldName, builder);
            return builder;
        }

        public SubscriberBuilder Subscriber(ComplexType owner, string fieldName)
        {
            if (!_subscribers.ContainsKey(owner.Name))
                _subscribers[owner.Name] = new Dictionary<string, SubscriberBuilder>();

            if (_subscribers[owner.Name].ContainsKey(fieldName))
                throw new SchemaBuilderException(owner.Name,
                    $"Cannot add subscriber for '{fieldName}'. Subscriber has been already created. Use {nameof(GetOrAddSubscriber)}.");

            var builder = new SubscriberBuilder();
            _subscribers[owner.Name].Add(fieldName, builder);
            return builder;
        }
    }
}