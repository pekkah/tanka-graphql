using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Core.Generators
{
    internal class InterfaceTypeControllerGenerator
    {
        private readonly InterfaceType _interfaceType;
        private readonly SchemaBuilder _schema;

        public InterfaceTypeControllerGenerator(InterfaceType interfaceType, SchemaBuilder schema)
        {
            _interfaceType = interfaceType;
            _schema = schema;
        }

        public MemberDeclarationSyntax Generate()
        {
            var controllerName = _interfaceType.Name
                .ToControllerName();

            var controllerInterfaceName = _interfaceType.Name
                .ToModelInterfaceName()
                .ToControllerName();
            
            var modelName = _interfaceType.Name
                .ToModelInterfaceName();

            return ClassDeclaration(controllerName)
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword)))
                .WithBaseList(
                    BaseList(
                        SingletonSeparatedList<BaseTypeSyntax>(
                            SimpleBaseType(
                                IdentifierName(controllerInterfaceName)))))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        MethodDeclaration(
                                IdentifierName("INamedType"),
                                Identifier("IsTypeOf"))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword)))
                            .WithParameterList(
                                ParameterList(
                                    SeparatedList<ParameterSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                            Parameter(
                                                    Identifier("instance"))
                                                .WithType(
                                                    IdentifierName(modelName)),
                                            Token(SyntaxKind.CommaToken),
                                            Parameter(
                                                    Identifier("schema"))
                                                .WithType(
                                                    IdentifierName("ISchema"))
                                        })))
                            .WithBody(
                                Block(
                                    SingletonList<StatementSyntax>(
                                        ReturnStatement(
                                            InvocationExpression(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        IdentifierName("schema"),
                                                        IdentifierName("GetNamedType")))
                                                .WithArgumentList(
                                                    ArgumentList(
                                                        SingletonSeparatedList(
                                                            Argument(
                                                                MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    IdentifierName("instance"),
                                                                    IdentifierName("__Typename"))))))))))));
        }
    }
}