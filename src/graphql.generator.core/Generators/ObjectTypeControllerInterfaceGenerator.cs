using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Core.Generators
{
    public class ObjectTypeControllerInterfaceGenerator
    {
        private readonly ObjectType _objectType;
        private readonly SchemaBuilder _schema;

        public ObjectTypeControllerInterfaceGenerator(ObjectType objectType, SchemaBuilder schema)
        {
            _objectType = objectType;
            _schema = schema;
        }

        public MemberDeclarationSyntax Generate()
        {
            var controllerInterfaceName = _objectType.Name.ToControllerName().ToInterfaceName();
            return InterfaceDeclaration(controllerInterfaceName)
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.PartialKeyword)))
                .WithMembers(List(GenerateFields(_objectType, _schema)));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateFields(ObjectType objectType, SchemaBuilder schema)
        {
            var members = _schema.GetFields(objectType)
                .SelectMany(field => GenerateField(objectType, field, schema))
                .ToList();

            return members;
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateField(ObjectType objectType, KeyValuePair<string, IField> field,
            SchemaBuilder schema)
        {
            var methodName = field.Key.ToFieldResolverName();
            var isSubscription = _schema.IsSubscriptionType(objectType);

            if (isSubscription)
                foreach (var member in GenerateSubscriptionField(objectType, field))
                    yield return member;

            yield return MethodDeclaration(
                    GenericName(Identifier(nameof(ValueTask)))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    IdentifierName(nameof(IResolverResult))))),
                    Identifier(methodName))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(
                                    Identifier("context"))
                                .WithType(
                                    IdentifierName(nameof(IResolverContext))))))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateSubscriptionField(ObjectType objectType, KeyValuePair<string, IField> field)
        {
            var methodName = field.Key.ToFieldResolverName();
            var returnType = nameof(ISubscriberResult);

            yield return MethodDeclaration(
                    GenericName(
                            Identifier("ValueTask"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    IdentifierName(returnType)))),
                    Identifier(methodName))
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Parameter(
                                        Identifier("context"))
                                    .WithType(
                                        IdentifierName(nameof(IResolverContext))),
                                Token(SyntaxKind.CommaToken),
                                Parameter(
                                        Identifier("unsubscribe"))
                                    .WithType(
                                        IdentifierName(nameof(CancellationToken)))
                            })))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
        }
    }
}