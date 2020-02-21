using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.Introspection;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Core.Generators
{
    public class ObjectTypeModelGenerator
    {
        private readonly ObjectType _objectType;
        private readonly SchemaBuilder _schema;

        public ObjectTypeModelGenerator(ObjectType objectType, SchemaBuilder schema)
        {
            _objectType = objectType;
            _schema = schema;
        }

        public MemberDeclarationSyntax Generate()
        {
            var modelName = _objectType.Name.ToModelName();
            var classDeclaration = ClassDeclaration(modelName)
                
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.PartialKeyword)))
                .WithLeadingTrivia(CodeModel.ToXmlComment(_objectType.Description))
                .WithMembers(
                    List(GenerateProperties()));

            var baseList = WithInterfaces(out var count);

            if (count > 0)
            {
                classDeclaration = classDeclaration.WithBaseList(baseList);
            }
        
            return classDeclaration;
        }

        private BaseListSyntax WithInterfaces(out int count)
        {
            var interfaceNames = new List<string>();

            // interface types
            if (_objectType.Interfaces != null && _objectType.Interfaces.Any())
            {
                interfaceNames.AddRange(_objectType.Interfaces.Select(
                    interfaceType => interfaceType.Name.ToModelInterfaceName()));
            }

            // union types
            var unionTypes = _schema.GetTypes<UnionType>()
                .Where(unionType => unionType.IsPossible(_objectType))
                .ToList();

            if (unionTypes.Count > 0)
            {
                interfaceNames.AddRange(unionTypes.Select(
                    unionType => unionType.Name.ToModelInterfaceName()));
            }

            // create implemented interface list
            var interfaceCount = interfaceNames.Count;

            if (interfaceCount == 0)
            {
                count = 0;
                return BaseList();
            }

            var interfaceList = new List<SyntaxNodeOrToken>();

            for (int i = 0; i < interfaceCount; i++)
            {
                var interfaceName = interfaceNames[i];

                interfaceList.Add(SimpleBaseType(IdentifierName(interfaceName)));

                if (interfaceCount > 1 && i < interfaceCount - 1)
                    interfaceList.Add(Token(SyntaxKind.CommaToken));
            }

            count = interfaceCount;
            return BaseList(
                SeparatedList<BaseTypeSyntax>(interfaceList));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateProperties()
        {
            var props = new List<MemberDeclarationSyntax>();
            props.Add(CodeModel.TypenameProperty(_objectType.Name));

            var fields = _schema.GetFields(_objectType);

            foreach (var field in fields)
            {
                if (ObjectTypeAbstractControllerBaseGenerator.IsAbstract(
                    _schema, 
                    _objectType, 
                    field))
                    continue;

                props.Add(GenerateProperty(field));
            }

            return props;
        }

        private MemberDeclarationSyntax GenerateProperty(KeyValuePair<string, IField> field)
        {
            var propertyName = field.Key.ToFieldResolverName();
            var typeName = SelectFieldType(field);
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
                                AccessorDeclaration(
                                        SyntaxKind.SetAccessorDeclaration)
                                    .WithSemicolonToken(
                                        Token(SyntaxKind.SemicolonToken))
                            })));
        }

        public string SelectFieldType(KeyValuePair<string, IField> field)
        {
            return CodeModel.SelectFieldTypeName(_schema, _objectType, field);
        }
    }
}