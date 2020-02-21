using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Core.Generators
{
    public class ObjectTypeFieldResolversGenerator
    {
        private readonly ObjectType _objectType;
        private readonly SchemaBuilder _schema;
        private string _name;

        public ObjectTypeFieldResolversGenerator(ObjectType objectType, SchemaBuilder schema)
        {
            _objectType = objectType;
            _schema = schema;
            _name = _objectType.Name.ToFieldResolversName();
        }

        public MemberDeclarationSyntax Generate()
        {
            return ClassDeclaration(_name)
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.PartialKeyword)))
                .WithBaseList(
                    BaseList(
                        SingletonSeparatedList<BaseTypeSyntax>(
                            SimpleBaseType(
                                IdentifierName(nameof(FieldResolversMap))))))
                .WithMembers(
                    List(WithMembers()));
        }

        private IEnumerable<MemberDeclarationSyntax> WithMembers()
        {
            yield return WithConstructor();
            yield return MethodDeclaration(
                    PredefinedType(
                        Token(SyntaxKind.VoidKeyword)),
                    Identifier("Modify"))
                .WithModifiers(
                    TokenList(
                        new []{
                            Token(SyntaxKind.PartialKeyword)}))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
        }

        private MemberDeclarationSyntax WithConstructor()
        {
            return ConstructorDeclaration(
                    Identifier(_name))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithBody(Block(WithResolvers()));
        }

        private List<StatementSyntax> WithResolvers()
        {
            var statements = _schema.GetFields(_objectType)
                .SelectMany(WithAddResolver)
                .ToList();

            statements.Add(ExpressionStatement(InvocationExpression(IdentifierName("Modify"))));

            return statements;
        }

        private IEnumerable<MemberDeclarationSyntax> WithModify()
        {
            yield return MethodDeclaration(
                    PredefinedType(
                        Token(SyntaxKind.VoidKeyword)),
                    Identifier("Modify"))
                .WithModifiers(
                    TokenList(
                        new []{
                            Token(SyntaxKind.PartialKeyword)}))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
        }

        private IEnumerable<StatementSyntax> WithAddResolver(KeyValuePair<string, IField> field)
        {
            var isSubscription = _schema.IsSubscriptionType(_objectType);

            if (isSubscription)
            {
                yield return WithAddSubscriber(field);
                yield break;
            }


            var interfaceName = _objectType.Name.ToControllerName().ToInterfaceName();
            var fieldName = field.Key;
            var methodName = fieldName.ToFieldResolverName();

            yield return ExpressionStatement(
                InvocationExpression(
                        IdentifierName(nameof(FieldResolversMap.Add)))
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[]
                                {
                                    Argument(
                                        LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            Literal(fieldName))),
                                    Token(SyntaxKind.CommaToken),
                                    Argument(
                                        SimpleLambdaExpression(
                                            Parameter(
                                                Identifier("context")),
                                            InvocationExpression(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        InvocationExpression(
                                                            MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                IdentifierName("context"),
                                                                GenericName(
                                                                        Identifier("Use"))
                                                                    .WithTypeArgumentList(
                                                                        TypeArgumentList(
                                                                            SingletonSeparatedList<TypeSyntax>(
                                                                                IdentifierName(interfaceName)))))),
                                                        IdentifierName(methodName)))
                                                .WithArgumentList(
                                                    ArgumentList(
                                                        SingletonSeparatedList(
                                                            Argument(
                                                                IdentifierName("context")))))))
                                }))));
        }

        private StatementSyntax WithAddSubscriber(KeyValuePair<string, IField> field)
        {
            var interfaceName = _objectType.Name.ToControllerName().ToInterfaceName();
            var fieldName = field.Key;
            var methodName = fieldName.ToFieldResolverName();

            var subscribe = Argument(
                ParenthesizedLambdaExpression(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Parameter(Identifier("context")),
                                Token(SyntaxKind.CommaToken),
                                Parameter(Identifier("unsubscribe"))
                            })),
                        InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("context"),
                                            GenericName(
                                                    Identifier("Use"))
                                                .WithTypeArgumentList(
                                                    TypeArgumentList(
                                                        SingletonSeparatedList<TypeSyntax>(
                                                            IdentifierName(
                                                                interfaceName)))))),
                                    IdentifierName(methodName)))
                            .WithArgumentList(
                                ArgumentList(
                                    SeparatedList<ArgumentSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                            Argument(
                                                IdentifierName("context")),
                                            Token(SyntaxKind.CommaToken),
                                            Argument(
                                                IdentifierName("unsubscribe"))
                                        })))));

            return ExpressionStatement(
                InvocationExpression(
                        IdentifierName(nameof(FieldResolversMap.Add)))
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[]
                                {
                                    Argument(
                                        LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            Literal(fieldName))),
                                    Token(SyntaxKind.CommaToken),
                                    subscribe,
                                    Token(SyntaxKind.CommaToken),
                                    Argument(
                                        SimpleLambdaExpression(
                                            Parameter(
                                                Identifier("context")),
                                            InvocationExpression(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        InvocationExpression(
                                                            MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                IdentifierName("context"),
                                                                GenericName(
                                                                        Identifier("Use"))
                                                                    .WithTypeArgumentList(
                                                                        TypeArgumentList(
                                                                            SingletonSeparatedList<TypeSyntax>(
                                                                                IdentifierName(interfaceName)))))),
                                                        IdentifierName(methodName)))
                                                .WithArgumentList(
                                                    ArgumentList(
                                                        SingletonSeparatedList(
                                                            Argument(
                                                                IdentifierName("context")))))))
                                }))));
        }
    }
}