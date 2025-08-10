using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Tanka.GraphQL.Server.SourceGenerators.Internal;

namespace Tanka.GraphQL.Server.SourceGenerators
{
    internal class InterfaceTypeParser(GeneratorAttributeSyntaxContext context)
    {
        public InterfaceControllerDefinition ParseInterfaceControllerDefinition()
        {
            return ParseDefinition((InterfaceDeclarationSyntax)context.TargetNode);
        }

        private InterfaceControllerDefinition ParseDefinition(InterfaceDeclarationSyntax declaration)
        {
            var (properties, methods) = ParseMembers(declaration);

            return new InterfaceControllerDefinition()
            {
                IsStatic = declaration.Modifiers.Any(SyntaxKind.StaticKeyword),
                Namespace = TypeHelper.GetNamespace(declaration),
                TargetType = declaration.Identifier.Text,
                GraphQLName = NamedTypeExtension.GetName(context.SemanticModel, declaration),
                Properties = properties,
                Methods = methods,
                ParentClass = TypeHelper.GetParentClasses(declaration),
                Usings = TypeHelper.GetUsings(declaration)
            };
        }

        private (List<ObjectPropertyDefinition> Properties, List<ObjectMethodDefinition> Methods) ParseMembers(
            InterfaceDeclarationSyntax classDeclaration)
        {
            var properties = new List<ObjectPropertyDefinition>();
            var methods = new List<ObjectMethodDefinition>();

            foreach (MemberDeclarationSyntax memberDeclarationSyntax in classDeclaration
                         .Members)
            {
                if (memberDeclarationSyntax.IsKind(SyntaxKind.PropertyDeclaration))
                {
                    var propertyDeclaration = (PropertyDeclarationSyntax)memberDeclarationSyntax;
                    var propertyDefinition = new ObjectPropertyDefinition()
                    {
                        Name = propertyDeclaration.Identifier.Text,
                        ReturnType = propertyDeclaration.Type.ToString(),
                        ClosestMatchingGraphQLTypeName = GetClosestMatchingGraphQLTypeName(context.SemanticModel, TypeHelper.UnwrapTaskType(propertyDeclaration.Type)),
                    };
                    properties.Add(propertyDefinition);
                }
                else if (memberDeclarationSyntax.IsKind(SyntaxKind.MethodDeclaration))
                {
                    var methodDeclaration = (MethodDeclarationSyntax)memberDeclarationSyntax;
                    var methodDefinition = new ObjectMethodDefinition()
                    {
                        IsStatic = methodDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword),
                        Name = methodDeclaration.Identifier.Text,
                        ReturnType = TypeHelper.UnwrapTaskType(methodDeclaration.ReturnType).ToString(),
                        ClosestMatchingGraphQLTypeName = GetClosestMatchingGraphQLTypeName(context.SemanticModel, TypeHelper.UnwrapTaskType(methodDeclaration.ReturnType)),
                        Type = GetMethodType(context, methodDeclaration),
                        Parameters = methodDeclaration.ParameterList.Parameters
                            .Where(p => p.Type is not null)
                            .Select(p => new ParameterDefinition()
                            {
                                Name = p.Identifier.Text,
                                Type = p.Type!.ToString(),
                                ClosestMatchingGraphQLTypeName = GetClosestMatchingGraphQLTypeName(context.SemanticModel, TypeHelper.UnwrapTaskType(p.Type)),
                                IsNullable = TypeHelper.IsTypeNullable(p.Type!),
                                IsPrimitive = TypeHelper.IsPrimitiveType(p.Type!),
                                FromArguments = TypeHelper.HasAttribute(p.AttributeLists, "FromArguments"),
                                FromServices = TypeHelper.HasAttribute(p.AttributeLists, "FromServices")
                            }).ToEquatableArray()
                    };
                    methods.Add(methodDefinition);
                }
            }

            return (properties, methods);
        }

        private static MethodType GetMethodType(GeneratorAttributeSyntaxContext context, MethodDeclarationSyntax methodDeclaration)
        {
            var returnType = methodDeclaration.ReturnType;

            if (returnType.IsKind(SyntaxKind.VoidKeyword))
                return MethodType.Void;

            if (returnType is IdentifierNameSyntax identifierNameSyntax)
            {
                return identifierNameSyntax switch
                {
                    { Identifier.ValueText: "Task" } => MethodType.Task,
                    { Identifier.ValueText: "ValueTask" } => MethodType.ValueTask,
                    _ => MethodType.T
                };
            }

            if (returnType is GenericNameSyntax namedTypeSyntax)
            {
                return namedTypeSyntax switch
                {

                    { Identifier.ValueText: "Task", TypeArgumentList.Arguments: not [] } => MethodType.TaskOfT,
                    { Identifier.ValueText: "ValueTask", TypeArgumentList.Arguments: not [] } => MethodType.ValueTaskOfT,
                    { Identifier.ValueText: "IAsyncEnumerable", TypeArgumentList.Arguments: not [] } => MethodType.AsyncEnumerableOfT,
                    { Identifier.ValueText: "IEnumerable", TypeArgumentList.Arguments: not [] } => MethodType.EnumerableT,
                    _ => MethodType.Unknown
                };
            }

            return MethodType.Unknown;
        }

        private static string GetClosestMatchingGraphQLTypeName(SemanticModel model, TypeSyntax typeSyntax)
        {
            var typeSymbol = model.GetTypeInfo(typeSyntax).Type;

            if (typeSymbol is null)
                return typeSyntax.ToString();

            return TypeHelper.GetGraphQLTypeName(typeSymbol, typeSyntax);
        }
    }
}