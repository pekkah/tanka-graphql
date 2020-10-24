using System;
using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.TypeSystem.ValueSerialization;
using Argument = Tanka.GraphQL.TypeSystem.Argument;
using EnumValue = Tanka.GraphQL.Language.Nodes.EnumValue;

namespace Tanka.GraphQL.SDL
{
    public delegate bool TrySerializeValue(IType type, object value, out ValueBase? valueNode);

    public class SchemaPrinterOptions
    {
        protected static IReadOnlyList<IType> IgnoredStandardTypes =
            Enumerable.Empty<IType>()
                .Concat(ScalarType.Standard.Select(t => t.Type))
                .Concat(new IType[]
                {
                    DirectiveType.Skip,
                    DirectiveType.Deprecated,
                    DirectiveType.Include
                })
                .ToList();

        public SchemaPrinterOptions(ISchema schema)
        {
            Schema = schema;
            TrySerializeValue = TrySerializeValueDefault;
        }

        public ISchema Schema { get; set; }

        public Func<IType, bool> ShouldPrintType { get; set; } =
            type =>
            {
                if (IgnoredStandardTypes.Contains(type))
                    return false;

                if (type.Unwrap()?.Name.StartsWith("__") == true)
                    return false;

                return true;
            };

        public Func<DirectiveInstance, bool> ShouldPrintDirective { get; set; } =
            directive => !directive.Name.StartsWith("__");

        public Func<string, IField, bool> ShouldPrintField { get; set; } = (name, _) => !name.StartsWith("__");

        public TrySerializeValue TrySerializeValue { get; set; }

        public bool TrySerializeValueDefault(IType type, object value, out ValueBase? valueNode)
        {
            var actualType = type.Unwrap();
            valueNode = actualType switch
            {
                InputObjectType inputObjectType => null,
                ScalarType scalarType => Schema.GetValueConverter(scalarType.Name).SerializeLiteral(value),
                IValueConverter converter => converter.SerializeLiteral(value),
                _ => null
            };

            if (valueNode == null) return false;

            return true;
        }
    }

    public class SchemaPrinterContext
    {
        public SchemaPrinterContext(SchemaPrinterOptions options)
        {
            Options = options;
        }

        public SchemaPrinterOptions Options { get; }

        public List<DirectiveDefinition> DirectiveDefinitions { get; } = new List<DirectiveDefinition>();

        public List<SchemaDefinition> SchemaDefinitions { get; } = new List<SchemaDefinition>();

        public List<SchemaExtension> SchemaExtensions { get; } = new List<SchemaExtension>();

        public List<TypeDefinition> TypeDefinitions { get; } = new List<TypeDefinition>();

        public List<TypeExtension> TypeExtensions { get; } = new List<TypeExtension>();

        public TypeSystemDocument GetTypeSystemDocument()
        {
            return new TypeSystemDocument(
                SchemaDefinitions.Any() ? SchemaDefinitions : null,
                TypeDefinitions.Any() ? TypeDefinitions : null,
                DirectiveDefinitions.Any() ? DirectiveDefinitions : null,
                SchemaExtensions.Any() ? SchemaExtensions : null,
                TypeExtensions.Any() ? TypeExtensions : null);
        }
    }

    public class SchemaPrinter
    {
        private static readonly IReadOnlyList<string> RootTypeNames = new List<string>
        {
            "Query",
            "Mutation",
            "Subscription"
        };

        private SchemaPrinter(SchemaPrinterOptions options)
        {
            Context = new SchemaPrinterContext(options);
        }

        protected SchemaPrinterContext Context { get; }

        protected SchemaPrinterOptions Options => Context.Options;

        protected ISchema Schema => Options.Schema;

        public static TypeSystemDocument Print(SchemaPrinterOptions options)
        {
            var printer = new SchemaPrinter(options);
            return printer.Print(options.Schema);
        }

        public TypeSystemDocument Print(ISchema schema)
        {
            VisitSchema(schema);
            return Context.GetTypeSystemDocument();
        }

        private void VisitSchema(ISchema schema)
        {
            foreach (var scalarType in schema.QueryTypes<ScalarType>())
                VisitScalarType(scalarType);

            foreach (var enumType in schema.QueryTypes<EnumType>())
                VisitEnumType(enumType);

            foreach (var scalarType in schema.QueryDirectiveTypes())
                VisitDirectiveType(scalarType);

            foreach (var inputType in schema.QueryTypes<InputObjectType>())
                VisitInputObjectType(inputType);

            foreach (var interfaceType in schema.QueryTypes<InterfaceType>())
                VisitInterfaceType(interfaceType);

            foreach (var objectType in schema.QueryTypes<ObjectType>(obj => !RootTypeNames.Contains(obj.Name)))
                VisitObjectType(objectType);

            foreach (var unionType in schema.QueryTypes<UnionType>()) VisitUnionType(unionType);


            var rootOperationTypeDefs = new List<RootOperationTypeDefinition>();
            if (schema.Query != null)
            {
                var fields = Schema.GetFields(schema.Query.Name);

                if (fields.Any(
                    f => Options.ShouldPrintField(f.Key, f.Value)))
                {
                    VisitObjectType(schema.Query);
                    rootOperationTypeDefs.Add(
                        new RootOperationTypeDefinition(
                            OperationType.Query,
                            new NamedType(schema.Query.Name)));
                }
            }

            if (schema.Mutation != null)
            {
                var fields = Schema.GetFields(schema.Mutation.Name);

                if (fields.Any(
                    f => Options.ShouldPrintField(f.Key, f.Value)))
                {
                    VisitObjectType(schema.Mutation);
                    rootOperationTypeDefs.Add(
                        new RootOperationTypeDefinition(
                            OperationType.Mutation,
                            new NamedType(schema.Mutation.Name)));
                }
            }

            if (schema.Subscription != null)
            {
                var fields = Schema.GetFields(schema.Subscription.Name);

                if (fields.Any(
                    f => Options.ShouldPrintField(f.Key, f.Value)))
                {
                    VisitObjectType(schema.Subscription);
                    rootOperationTypeDefs.Add(
                        new RootOperationTypeDefinition(
                            OperationType.Subscription,
                            new NamedType(schema.Subscription.Name)));
                }
            }

            if (rootOperationTypeDefs.Any())
                Context.SchemaDefinitions.Add(new SchemaDefinition(
                    null,
                    Directives(schema.Directives),
                    new RootOperationTypeDefinitions(rootOperationTypeDefs)));
        }

        private void VisitUnionType(UnionType unionType)
        {
            if (!Options.ShouldPrintType(unionType))
                return;

            var possibleTypes = Schema.GetPossibleTypes(unionType)
                .Select(t => new NamedType(t.Name))
                .ToList();

            Context.TypeDefinitions.Add(new UnionDefinition(
                    unionType.Description,
                    unionType.Name,
                    Directives(unionType.Directives),
                    possibleTypes.Any() ? new UnionMemberTypes(possibleTypes) : null
                )
            );
        }

        private void VisitObjectType(ObjectType objectType)
        {
            if (!Options.ShouldPrintType(objectType))
                return;

            var implements = Implements(objectType.Interfaces.ToList());
            var fields = FieldsDefinition(objectType);
            var objectNode = new ObjectDefinition(
                objectType.Description,
                objectType.Name,
                implements,
                Directives(objectType.Directives),
                fields
            );

            Context.TypeDefinitions.Add(objectNode);
        }

        private void VisitInterfaceType(InterfaceType interfaceType)
        {
            if (!Options.ShouldPrintType(interfaceType))
                return;

            var interfaceNode = new InterfaceDefinition(
                interfaceType.Description,
                interfaceType.Name,
                null, //todo: interfaces implementing interfaces
                Directives(interfaceType.Directives),
                FieldsDefinition(interfaceType)
            );

            Context.TypeDefinitions.Add(interfaceNode);
        }

        private ImplementsInterfaces? Implements(IReadOnlyList<InterfaceType> interfaces)
        {
            if (!interfaces.Any())
                return null;

            var namedTypes = new List<NamedType>();

            foreach (var interfaceType in interfaces)
                namedTypes.Add(interfaceType.Name);

            return new ImplementsInterfaces(namedTypes);
        }

        private void VisitEnumType(EnumType enumType)
        {
            if (!Options.ShouldPrintType(enumType))
                return;

            var enumValues = enumType
                .Values
                .Select(v => new EnumValueDefinition(
                    v.Value.Description,
                    new EnumValue(v.Key),
                    Directives(v.Value.Directives)))
                .ToList();

            var enumNode = new EnumDefinition(
                enumType.Description,
                enumType.Name,
                Directives(enumType.Directives),
                enumValues.Any() ? new EnumValuesDefinition(enumValues) : null
            );

            Context.TypeDefinitions.Add(enumNode);
        }

        private void VisitInputObjectType(InputObjectType inputObjectType)
        {
            if (!Options.ShouldPrintType(inputObjectType))
                return;

            var fields = InputFieldsDefinition(Schema.GetInputFields(inputObjectType.Name).ToList());
            var inputNode = new InputObjectDefinition(
                inputObjectType.Description,
                inputObjectType.Name,
                Directives(inputObjectType.Directives),
                fields
            );

            Context.TypeDefinitions.Add(inputNode);
        }

        private void VisitDirectiveType(DirectiveType directiveType)
        {
            if (!Options.ShouldPrintType(directiveType))
                return;

            var directiveNode = new DirectiveDefinition(
                directiveType.Description,
                directiveType.Name,
                ArgumentDefinitions(directiveType.Arguments.ToList()),
                false,
                directiveType.Locations.Select(l => l.ToString()).ToList()
            );

            Context.DirectiveDefinitions.Add(directiveNode);
        }

        private ArgumentsDefinition? ArgumentDefinitions(IReadOnlyList<KeyValuePair<string, Argument>>? arguments)
        {
            if (arguments == null || !arguments.Any())
                return null;

            var args = new List<InputValueDefinition>();

            foreach (var (name, arg) in arguments)
            {
                var inputValueDefinition = new InputValueDefinition(
                    arg.Description,
                    name,
                    Type(arg.Type),
                    DefaultValue(arg.Type, arg.DefaultValue),
                    null
                );

                args.Add(inputValueDefinition);
            }


            return new ArgumentsDefinition(args);
        }

        private void VisitScalarType(ScalarType scalarType)
        {
            if (!Options.ShouldPrintType(scalarType))
                return;

            var scalarNode = new ScalarDefinition(
                scalarType.Description,
                scalarType.Name,
                Directives(scalarType.Directives)
            );

            Context.TypeDefinitions.Add(scalarNode);
        }

        private Language.Nodes.Directives? Directives(IEnumerable<DirectiveInstance>? directives)
        {
            if (directives == null)
                return null;

            var directiveNodes = new List<Directive>();
            foreach (var directive in directives)
            {
                if (!Options.ShouldPrintDirective(directive))
                    continue;

                directiveNodes.Add(new Directive(
                    directive.Name,
                    Arguments(directive))
                );
            }

            if (!directiveNodes.Any())
                return null;

            return new Language.Nodes.Directives(directiveNodes);
        }

        private Arguments? Arguments(DirectiveInstance directiveInstance)
        {
            var directiveType = Context.Options.Schema
                .GetDirectiveType(directiveInstance.Name);

            var args = new List<Language.Nodes.Argument>();
            foreach (var (name, arg) in directiveType.Arguments)
                if (directiveInstance.Arguments.TryGetValue(name, out var argValue))
                {
                    var valueNode = SerializeValue(
                        arg.Type,
                        argValue
                    );

                    args.Add(new Language.Nodes.Argument(name, valueNode));
                }

            return new Arguments(args);
        }

        private InputFieldsDefinition? InputFieldsDefinition(
            IReadOnlyList<KeyValuePair<string, InputObjectField>> inputFields)
        {
            if (!inputFields.Any())
                return null;

            var args = new List<InputValueDefinition>();

            foreach (var (name, field) in inputFields)
            {
                var inputValueDefinition = new InputValueDefinition(
                    field.Description,
                    name,
                    Type(field.Type),
                    DefaultValue(field.Type, field.DefaultValue),
                    null
                );

                args.Add(inputValueDefinition);
            }

            return new InputFieldsDefinition(args);
        }

        private FieldsDefinition? FieldsDefinition(ComplexType type)
        {
            var fields = Schema.GetFields(type.Name)
                .ToList();

            if (!fields.Any())
                return null;

            var fieldNodes = new List<FieldDefinition>(fields.Count);

            foreach (var (name, field) in fields)
            {
                if (!Options.ShouldPrintField(name, field))
                    continue;

                var fieldDefinition = new FieldDefinition(
                    field.Description,
                    name,
                    ArgumentDefinitions(field.Arguments.ToList()),
                    Type(field.Type),
                    Directives(field.Directives)
                );

                fieldNodes.Add(fieldDefinition);
            }

            return new FieldsDefinition(fieldNodes);
        }

        private ValueBase SerializeValue(IType type, object? value)
        {
            if (value == null)
                return new NullValue(); // what if type is nonNull?

            if (Options.TrySerializeValue(type, value, out var serializedValue))
                return serializedValue;

            throw new InvalidOperationException($"Cannot serialize value '{value}' of type '{type}'.");
        }

        private DefaultValue DefaultValue(IType type, object? defaultValue)
        {
            return new DefaultValue(SerializeValue(type, defaultValue));
        }

        private TypeBase Type(IType type)
        {
            if (type is NonNull nonNull)
                return new NonNullType(Type(nonNull.OfType));

            if (type is List list)
                return new ListType(Type(list.OfType));

            var namedType = type.Unwrap();

            return new NamedType(namedType.Name);
        }
    }
}