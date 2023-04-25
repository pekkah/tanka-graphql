﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace Tanka.GraphQL.Server.SourceGenerators
{
    internal class ObjectTypeParser
    {
        public static ObjectControllerDefinition ParseObjectControllerDefinition(
            GeneratorAttributeSyntaxContext context)
        {
            return ParseObjectControllerDefinition(
                context,
                (ClassDeclarationSyntax)context.TargetNode);
        }

        public static ObjectControllerDefinition ParseObjectControllerDefinition(
            GeneratorAttributeSyntaxContext context,
            ClassDeclarationSyntax classDeclaration)
        {
            var (properties, methods) = ParseMembers(context, classDeclaration);

            return new ObjectControllerDefinition()
            {
                IsStatic = classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword),
                Usings = classDeclaration.SyntaxTree.GetCompilationUnitRoot().Usings.Select(u => u.Name.ToString()).ToList(),
                Namespace = TypeHelper.GetNamespace(classDeclaration),
                TargetType = classDeclaration.Identifier.Text,
                Properties = properties,
                Methods = methods,
                ParentClass = TypeHelper.GetParentClasses(classDeclaration)
            };
        }

        private static (List<ObjectPropertyDefinition> Properties, List<ObjectMethodDefinition> Methods) ParseMembers(
            GeneratorAttributeSyntaxContext context, 
            ClassDeclarationSyntax classDeclaration)
        {
            var properties = new List<ObjectPropertyDefinition>();
            var methods = new List<ObjectMethodDefinition>();

            foreach (MemberDeclarationSyntax memberDeclarationSyntax in classDeclaration
                         .Members
                         .Where(m => m.Modifiers.Any(SyntaxKind.PublicKeyword)))
            {
                if (memberDeclarationSyntax.IsKind(SyntaxKind.PropertyDeclaration))
                {
                    var propertyDeclaration = (PropertyDeclarationSyntax)memberDeclarationSyntax;
                    var propertyDefinition = new ObjectPropertyDefinition()
                    {
                        Name = propertyDeclaration.Identifier.Text,
                        ReturnType = propertyDeclaration.Type.ToString(),
                        ClosestMatchingGraphQLTypeName = GetClosestMatchingGraphQLTypeName(context.SemanticModel, TypeHelper.UnwrapTaskType(propertyDeclaration.Type)),
                        IsNullable = TypeHelper.IsTypeNullable(propertyDeclaration.Type),
                    };
                    properties.Add(propertyDefinition);
                }
                else if (memberDeclarationSyntax.IsKind(SyntaxKind.MethodDeclaration))
                {
                    var methodDeclaration = (MethodDeclarationSyntax)memberDeclarationSyntax;
                    var methodDefinition = new ObjectMethodDefinition()
                    {
                        Name = methodDeclaration.Identifier.Text,
                        ReturnType = TypeHelper.UnwrapTaskType(methodDeclaration.ReturnType).ToString(),
                        ClosestMatchingGraphQLTypeName = GetClosestMatchingGraphQLTypeName(context.SemanticModel, TypeHelper.UnwrapTaskType(methodDeclaration.ReturnType)),
                        IsAsync = TypeHelper.IsValueTaskOrTask(methodDeclaration.ReturnType),
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
                                }).ToList()
                    };
                    methods.Add(methodDefinition);
                }
            }

            return (properties, methods);
        }

        private static string GetClosestMatchingGraphQLTypeName(SemanticModel model, TypeSyntax typeSyntax)
        {
            var typeSymbol = model.GetTypeInfo(typeSyntax).Type;

            if (typeSymbol is null)
                return typeSyntax.ToString();

            return TypeHelper.GetGraphQLTypeName(typeSymbol);
        }
    }
}
