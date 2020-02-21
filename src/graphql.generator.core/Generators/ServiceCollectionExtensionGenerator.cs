using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.SchemaBuilding;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Core.Generators
{
    public class ServiceCollectionExtensionGenerator
    {
        private readonly SchemaBuilder _schema;
        private readonly string _schemaName;

        public ServiceCollectionExtensionGenerator(SchemaBuilder schema, string schemaName)
        {
            _schema = schema;
            _schemaName = schemaName;
        }

        public MemberDeclarationSyntax Generate()
        {
            var builderName = _schemaName.ToServiceBuilderName();
            return ClassDeclaration("ServiceCollectionExtension")
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        MethodDeclaration(
                                IdentifierName(builderName),
                                Identifier($"Add{_schemaName}Controllers"))
                            .WithModifiers(
                                TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                            .WithParameterList(
                                ParameterList(
                                    SingletonSeparatedList(
                                        Parameter(
                                                Identifier("services"))
                                            .WithModifiers(
                                                TokenList(
                                                    Token(SyntaxKind.ThisKeyword)))
                                            .WithType(
                                                IdentifierName("IServiceCollection")))))
                            .WithBody(
                                Block(
                                    LocalDeclarationStatement(
                                        VariableDeclaration(
                                                IdentifierName("var"))
                                            .WithVariables(
                                                SingletonSeparatedList(
                                                    VariableDeclarator(
                                                            Identifier("builder"))
                                                        .WithInitializer(
                                                            EqualsValueClause(
                                                                ObjectCreationExpression(
                                                                        IdentifierName(builderName))
                                                                    .WithArgumentList(
                                                                        ArgumentList(
                                                                            SingletonSeparatedList(
                                                                                Argument(
                                                                                    IdentifierName("services")))))))))),
                                    ReturnStatement(
                                        IdentifierName("builder"))))))
                .NormalizeWhitespace();
        }
    }
}