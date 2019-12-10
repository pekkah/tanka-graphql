﻿using System;
using System.Collections.Generic;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.TypeSystem.ValueSerialization;

namespace Tanka.GraphQL.SchemaBuilding
{
    public partial class SchemaBuilder
    {
        private readonly ConnectionBuilder _connections;
        private readonly Dictionary<string, DirectiveType> _directives = new Dictionary<string, DirectiveType>();

        private readonly List<DirectiveInstance> _schemaDirectives = new List<DirectiveInstance>();

        private readonly Dictionary<string, INamedType> _types =
            new Dictionary<string, INamedType>();

        private string _schemaDescription;
        private readonly Dictionary<string, IValueConverter> _scalarSerializers = new Dictionary<string, IValueConverter>();

        private string _queryTypeName = "Query";
        private string _mutationTypeName = "Mutation";
        private string _subscriptionTypeName = "Subscription";

        public SchemaBuilder()
        {
            _connections = new ConnectionBuilder(this);

            foreach (var scalar in ScalarType.Standard)
            {
                Include(scalar.Type);
                ScalarSerializer(scalar.Type.Name, scalar.Serializer);
            }

            Include(TypeSystem.DirectiveType.Include);
            Include(TypeSystem.DirectiveType.Skip);
        }

        public SchemaBuilder Connections(Action<ConnectionBuilder> connections)
        {
            if (connections == null)
                throw new ArgumentNullException(nameof(connections));

            connections(_connections);

            return this;
        }

        public SchemaBuilder Object(
            string name,
            out ObjectType definition,
            string description = null,
            IEnumerable<InterfaceType> interfaces = null,
            IEnumerable<DirectiveInstance> directives = null)
        {
            definition = new ObjectType(
                name,
                description,
                interfaces,
                directives);

            Include(definition);
            return this;
        }

        public SchemaBuilder Interface(
            string name,
            out InterfaceType definition,
            string description = null,
            IEnumerable<DirectiveInstance> directives = null)
        {
            definition = new InterfaceType(
                name,
                description,
                directives);

            Include(definition);
            return this;
        }

        public SchemaBuilder InputObject(
            string name,
            out InputObjectType definition,
            string description = null,
            IEnumerable<DirectiveInstance> directives = null)
        {
            definition = new InputObjectType(name, description, directives);
            Include(definition);
            return this;
        }

        public SchemaBuilder Enum(string name,
            out EnumType enumType,
            string description = null,
            Action<EnumValuesBuilder> values = null,
            IEnumerable<DirectiveInstance> directives = null)
        {
            var valuesBuilder = new EnumValuesBuilder();
            values?.Invoke(valuesBuilder);
            enumType = new EnumType(
                name,
                valuesBuilder.Build(),
                description,
                directives);

            Include(enumType);
            return this;
        }

        public SchemaBuilder DirectiveType(
            string name,
            out DirectiveType directiveType,
            IEnumerable<DirectiveLocation> locations,
            string description = null,
            Action<ArgsBuilder> args = null)
        {
            var argsBuilder = new ArgsBuilder();
            args?.Invoke(argsBuilder);
            directiveType = new DirectiveType(name, locations, argsBuilder.Build());
            Include(directiveType);
            return this;
        }

        public SchemaBuilder Union(
            string name,
            out UnionType unionType,
            string description = null,
            IEnumerable<DirectiveInstance> directives = null,
            params ObjectType[] possibleTypes)
        {
            unionType = new UnionType(name, possibleTypes, description, directives);
            Include(unionType);
            return this;
        }

        public SchemaBuilder Scalar(
            string name,
            out ScalarType scalarType,
            IValueConverter serializer,
            string description = null,
            IEnumerable<DirectiveInstance> directives = null)
        {
            scalarType = new ScalarType(name, description, directives);
            Include(scalarType);
            ScalarSerializer(name, serializer);
            return this;
        }

        public SchemaBuilder Scalar(
            string name,
            out ScalarType scalarType,
            string description = null,
            IEnumerable<DirectiveInstance> directives = null)
        {
            scalarType = new ScalarType(name, description, directives);
            Include(scalarType);

            return this;
        }

        public SchemaBuilder ScalarSerializer(
            string name,
            IValueConverter serializer)
        {
            _scalarSerializers.Add(name, serializer);
            return this;
        }

        public SchemaBuilder Schema(
            string description = null,
            IEnumerable<DirectiveInstance> directives = null)
        {
            _schemaDescription = description;
            if (directives != null)
                _schemaDirectives.AddRange(directives);

            return this;
        }
    }
}