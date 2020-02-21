using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Core.Generators
{
    internal class InterfaceTypeModelGenerator
    {
        private readonly InterfaceType _interfaceType;
        private readonly SchemaBuilder _schema;

        public InterfaceTypeModelGenerator(InterfaceType interfaceType, SchemaBuilder schema)
        {
            _interfaceType = interfaceType;
            _schema = schema;
        }

         public MemberDeclarationSyntax Generate()
        {
            var modelName = _interfaceType.Name.ToModelInterfaceName();
            return InterfaceDeclaration(modelName)
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.PartialKeyword)))
                .WithLeadingTrivia(CodeModel.ToXmlComment(_interfaceType.Description))
                .WithMembers(
                    List(GenerateProperties()));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateProperties()
        {
            var props = new List<MemberDeclarationSyntax>();
            props.Add(CodeModel.TypenameProperty(_interfaceType.Name));

            var fields = _schema.GetFields(_interfaceType);

            foreach (var field in fields)
            {
                if (ObjectTypeAbstractControllerBaseGenerator.IsAbstract(
                    _schema,
                    _interfaceType,
                    field))
                    continue;

                props.Add(GenerateProperty(field));
            }

            return props;
        }

        private MemberDeclarationSyntax GenerateProperty(KeyValuePair<string, IField> field)
        {
            var propertyName = field.Key.ToFieldResolverName();
            var typeName = CodeModel.SelectFieldTypeName(_schema, _interfaceType, field);
            return PropertyDeclaration(
                    IdentifierName(typeName),
                    Identifier(propertyName))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithLeadingTrivia(CodeModel.ToXmlComment(field.Value.Description))
                .WithAccessorList(
                    AccessorList(
                        List(
                            new[]
                            {
                                AccessorDeclaration(
                                        SyntaxKind.GetAccessorDeclaration)
                                    .WithSemicolonToken(
                                        Token(SyntaxKind.SemicolonToken)),
                                /*AccessorDeclaration(
                                        SyntaxKind.SetAccessorDeclaration)
                                    .WithSemicolonToken(
                                        Token(SyntaxKind.SemicolonToken))*/
                            })));
        }
    }
}