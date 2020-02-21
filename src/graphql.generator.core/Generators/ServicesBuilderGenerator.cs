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
    internal class ServicesBuilderGenerator
    {
        private readonly SchemaBuilder _schema;
        private readonly string _schemaName;

        public ServicesBuilderGenerator(SchemaBuilder schema, string schemaName)
        {
            _schema = schema;
            _schemaName = schemaName;
        }

        public MemberDeclarationSyntax Generate()
        {
            var builderName = _schemaName.ToServiceBuilderName();
            return ClassDeclaration(builderName)
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithMembers(
                    List(GenerateMembers(builderName)))
                .NormalizeWhitespace();
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateMembers(string builderName)
        {
            // list of generated interface types
            yield return FieldDeclaration(
                    VariableDeclaration(
                            ArrayType(
                                    IdentifierName("Type"))
                                .WithRankSpecifiers(
                                    SingletonList(
                                        ArrayRankSpecifier(
                                            SingletonSeparatedList<ExpressionSyntax>(
                                                OmittedArraySizeExpression())))))
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(
                                        Identifier("GeneratedControllerTypes"))
                                    .WithInitializer(
                                        EqualsValueClause(
                                            InitializerExpression(
                                                SyntaxKind.ArrayInitializerExpression,
                                                SeparatedList<ExpressionSyntax>(
                                                    GenerateControllerInterfaceList())))))))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)));

            // constructor
            yield return ConstructorDeclaration(
                    Identifier(builderName))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(
                                    Identifier("services"))
                                .WithType(
                                    IdentifierName("IServiceCollection")))))
                .WithBody(
                    Block(
                        SingletonList<StatementSyntax>(
                            ExpressionStatement(
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    IdentifierName("Services"),
                                    IdentifierName("services"))))));

            // Services for extensions methods
            yield return PropertyDeclaration(
                    IdentifierName("IServiceCollection"),
                    Identifier("Services"))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(
                    AccessorList(
                        SingletonList(
                            AccessorDeclaration(
                                    SyntaxKind.GetAccessorDeclaration)
                                .WithSemicolonToken(
                                    Token(SyntaxKind.SemicolonToken)))));

            // Add method for each controller interface type
            foreach (var namedType in CollectNames())
                yield return AddControllerMethod(
                    builderName,
                    namedType);
        }

        private MemberDeclarationSyntax AddControllerMethod(string builderName, string namedTypeName)
        {
            var controllerName = namedTypeName.ToControllerName();
            var controllerInterfaceName = namedTypeName.ToControllerName().ToInterfaceName();

            return MethodDeclaration(
                    IdentifierName(builderName),
                    Identifier($"Add{controllerName}"))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithTypeParameterList(
                    TypeParameterList(
                        SingletonSeparatedList(
                            TypeParameter(
                                Identifier("T")))))
                .WithConstraintClauses(
                    SingletonList(
                        TypeParameterConstraintClause(
                                IdentifierName("T"))
                            .WithConstraints(
                                SeparatedList<TypeParameterConstraintSyntax>(
                                    new SyntaxNodeOrToken[]
                                    {
                                        ClassOrStructConstraint(
                                            SyntaxKind.ClassConstraint),
                                        Token(SyntaxKind.CommaToken),
                                        TypeConstraint(
                                            IdentifierName(controllerInterfaceName))
                                    }))))
                .WithBody(
                    Block(
                        ExpressionStatement(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("Services"),
                                    GenericName(
                                            Identifier("TryAddScoped"))
                                        .WithTypeArgumentList(
                                            TypeArgumentList(
                                                SeparatedList<TypeSyntax>(
                                                    new SyntaxNodeOrToken[]
                                                    {
                                                        IdentifierName(controllerInterfaceName),
                                                        Token(SyntaxKind.CommaToken),
                                                        IdentifierName("T")
                                                    })))))),
                        ReturnStatement(
                            ThisExpression())));
        }

        private IEnumerable<SyntaxNodeOrToken> GenerateControllerInterfaceList()
        {
            var typeNames = CollectNames()
                .ToList();

            foreach (var typeName in typeNames)
            {
                var controllerInterfaceName = typeName.ToControllerName().ToInterfaceName();
                yield return TypeOfExpression(IdentifierName(controllerInterfaceName));
                yield return Token(SyntaxKind.CommaToken);
            }
        }

        private IEnumerable<string> CollectNames()
        {
            foreach (var objectType in _schema.GetTypes<ObjectType>())
                yield return objectType.Name;

            foreach (var interfaceType in _schema.GetTypes<InterfaceType>())
                yield return interfaceType.Name;

            foreach (var unionType in _schema.GetTypes<UnionType>())
                yield return unionType.Name;
        }
    }
}