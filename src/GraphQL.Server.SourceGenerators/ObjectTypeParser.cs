﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

using Tanka.GraphQL.Server.SourceGenerators.Internal;

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
                Namespace = TypeHelper.GetNamespace(classDeclaration),
                TargetType = classDeclaration.Identifier.Text,
                GraphQLName = NamedTypeExtension.GetName(context.SemanticModel, classDeclaration),
                Properties = properties,
                Methods = methods,
                ParentClass = TypeHelper.GetParentClasses(classDeclaration),
                Usings = TypeHelper.GetUsings(classDeclaration),
                Implements = GetImplements(context.SemanticModel, classDeclaration),
            };
        }

        private static IReadOnlyList<BaseDefinition> GetImplements(SemanticModel model, ClassDeclarationSyntax classDeclaration)
        {
            return SymbolHelper.GetImplements(model.GetDeclaredSymbol(classDeclaration) ?? throw new InvalidOperationException());
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

            var typeName = TypeHelper.GetGraphQLTypeName(typeSymbol);

            // dirty hack until we have a better way to handle this
            if (typeSyntax is NullableTypeSyntax && typeName.AsSpan().EndsWith("!"))
            {
                return typeName.AsSpan().Slice(0, typeName.Length - 1).ToString();
            }

            return typeName;
        }
    }
}
