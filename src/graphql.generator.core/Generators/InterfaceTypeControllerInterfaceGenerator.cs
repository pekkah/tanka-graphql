using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Core.Generators
{
    internal class InterfaceTypeControllerInterfaceGenerator
    {
        private readonly InterfaceType _interfaceType;
        private readonly SchemaBuilder _schema;

        public InterfaceTypeControllerInterfaceGenerator(InterfaceType interfaceType, SchemaBuilder schema)
        {
            _interfaceType = interfaceType;
            _schema = schema;
        }

        public MemberDeclarationSyntax Generate()
        {
            var modelControllerName = _interfaceType.Name.ToModelInterfaceName().ToControllerName();
            var modelName = _interfaceType.Name.ToModelInterfaceName();
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