using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Core.Generators
{
    internal class UnionTypeControllerInterfaceGenerator
    {
        private readonly UnionType _unionType;
        private readonly SchemaBuilder _schema;

        public UnionTypeControllerInterfaceGenerator(UnionType unionType, SchemaBuilder schema)
        {
            _unionType = unionType;
            _schema = schema;
        }

        public MemberDeclarationSyntax Generate()
        {
            var modelControllerName = _unionType.Name.ToModelInterfaceName().ToControllerName();
            var modelName = _unionType.Name.ToModelInterfaceName();
            return InterfaceDeclaration(modelControllerName)
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword)))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        MethodDeclaration(
                                IdentifierName("INamedType"),
                                Identifier("IsTypeOf"))
                            .WithParameterList(
                                ParameterList(
                                    SeparatedList<ParameterSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                            Parameter(
                                                    Identifier("instance"))
                                                .WithType(
                                                    IdentifierName($"{modelName}")),
                                            Token(SyntaxKind.CommaToken),
                                            Parameter(
                                                    Identifier("schema"))
                                                .WithType(
                                                    IdentifierName("ISchema"))
                                        })))
                            .WithSemicolonToken(
                                Token(SyntaxKind.SemicolonToken))));
        }
    }
}