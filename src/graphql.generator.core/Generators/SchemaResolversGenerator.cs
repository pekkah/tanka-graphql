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
    public class SchemaResolversGenerator
    {
        private readonly SchemaBuilder _schema;
        private readonly string _name;
        private string _resolversName;

        public SchemaResolversGenerator(SchemaBuilder schema, string name)
        {
            _schema = schema;
            _name = name;
            _resolversName = _name.ToSchemaResolversName();
        }

        public MemberDeclarationSyntax Generate()
        {
            return ClassDeclaration(_resolversName)
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.PartialKeyword)))
                .WithBaseList(
                    BaseList(
                        SingletonSeparatedList<BaseTypeSyntax>(
                            SimpleBaseType(
                                IdentifierName(nameof(ObjectTypeMap))))))
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
                    Identifier(_resolversName))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithBody(Block(WithAddObjectResolvers()));
        }

        private IEnumerable<StatementSyntax> WithAddObjectResolvers()
        {
            var objectTypes = _schema.GetTypes<ObjectType>();
            foreach (var objectType in objectTypes)
            {
                yield return WithAddObjectFieldResolvers(objectType);
            }

            yield return ExpressionStatement(InvocationExpression(IdentifierName("Modify")));
        }

        private StatementSyntax WithAddObjectFieldResolvers(ObjectType objectType)
        {
            var objectName = objectType.Name;
            var resolversName = objectName.ToFieldResolversName();
            return ExpressionStatement(
                InvocationExpression(
                        IdentifierName(nameof(ObjectTypeMap.Add)))
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[]
                                {
                                    Argument(
                                        LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            Literal(objectName))),
                                    Token(SyntaxKind.CommaToken),
                                    Argument(
                                        ObjectCreationExpression(
                                                IdentifierName(resolversName))
                                            .WithArgumentList(
                                                ArgumentList()))
                                }))));
        }
    }
}