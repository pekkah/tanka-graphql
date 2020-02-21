using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Core.Generators
{
    internal class UnionTypeModelGenerator
    {
        private readonly UnionType _unionType;
        private readonly SchemaBuilder _schema;

        public UnionTypeModelGenerator(UnionType unionType, SchemaBuilder schema)
        {
            _unionType = unionType;
            _schema = schema;
        }

        public MemberDeclarationSyntax Generate()
        {
            var modelName = _unionType.Name.ToModelInterfaceName();
            return InterfaceDeclaration(modelName)
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.PartialKeyword)))
                .WithLeadingTrivia(CodeModel.ToXmlComment(_unionType.Description))
                .WithMembers(
                    List(GenerateProperties()));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateProperties()
        {
            var props = new List<MemberDeclarationSyntax>
            {
                CodeModel.TypenameProperty(_unionType.Name)
            };

            /*var fields = _schema.GetFields(_unionType);

            foreach (var field in fields)
            {
                if (ObjectTypeAbstractControllerBaseGenerator.IsAbstract(
                    _schema,
                    _unionType,
                    field))
                    continue;

                props.Add(GenerateProperty(field));
            }*/

            return props;
        }
    }
}