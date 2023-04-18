using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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
                Namespace = GetNamespace(classDeclaration),
                TargetType = classDeclaration.Identifier.Text,
                Properties = properties,
                Methods = methods,
                ParentClass = GetParentClasses(classDeclaration)
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
                        ClosestMatchingGraphQLTypeName = GetClosestMatchingGraphQLTypeName(context.SemanticModel, UnwrapTaskType(propertyDeclaration.Type)),
                        IsNullable = IsTypeNullable(propertyDeclaration.Type),
                    };
                    properties.Add(propertyDefinition);
                }
                else if (memberDeclarationSyntax.IsKind(SyntaxKind.MethodDeclaration))
                {
                    var methodDeclaration = (MethodDeclarationSyntax)memberDeclarationSyntax;
                    var methodDefinition = new ObjectMethodDefinition()
                    {
                        Name = methodDeclaration.Identifier.Text,
                        ReturnType = UnwrapTaskType(methodDeclaration.ReturnType).ToString(),
                        ClosestMatchingGraphQLTypeName = GetClosestMatchingGraphQLTypeName(context.SemanticModel, UnwrapTaskType(methodDeclaration.ReturnType)),
                        IsAsync = IsValueTaskOrTask(methodDeclaration.ReturnType),
                        Parameters = methodDeclaration.ParameterList.Parameters
                            .Where(p => p.Type is not null)
                            .Select(p => new ParameterDefinition()
                                {
                                    Name = p.Identifier.Text,
                                    Type = p.Type!.ToString(),
                                    ClosestMatchingGraphQLTypeName = GetClosestMatchingGraphQLTypeName(context.SemanticModel, UnwrapTaskType(p.Type)),
                                    IsNullable = IsTypeNullable(p.Type!),
                                    IsPrimitive = IsPrimitiveType(p.Type!),
                                    FromArguments = HasAttribute(p.AttributeLists, "FromArguments"),
                                    FromServices = HasAttribute(p.AttributeLists, "FromServices")
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

        public static bool HasAttribute(
            SyntaxList<AttributeListSyntax> attributeLists,
            string attributeName)
        {
            // Check if the parameter has any attributes
            if (!attributeLists.Any())
            {
                return false;
            }

            foreach (var attributeList in attributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    // should probably check fully qualified name
                    if (attribute.Name.ToString() == attributeName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }


    public static bool IsPrimitiveType(TypeSyntax typeSyntax)
        {
            // Define a list of C# primitive types
            string[] primitiveTypes = new string[]
            {
                "bool",
                "byte",
                "sbyte",
                "char",
                "decimal",
                "double",
                "float",
                "int",
                "uint",
                "long",
                "ulong",
                "short",
                "ushort",
                "string"
            };

            // Get the name of the type syntax
            string typeName = UnwrapNullable(typeSyntax).ToString();

            // Check if the type name is a primitive type
            return primitiveTypes.Contains(typeName);
        }


        public static bool IsTypeNullable(TypeSyntax typeSyntax)
        {
            return typeSyntax.IsKind(SyntaxKind.NullableType);
        }

        public static TypeSyntax UnwrapTaskType(TypeSyntax typeSyntax)
        {
            // Check if the type is a named type syntax
            if (typeSyntax is GenericNameSyntax namedTypeSyntax)
            {
                // Check if the name of the type is "Task" or "ValueTask"
                if (namedTypeSyntax.Identifier.ValueText is "Task" or "ValueTask")
                {
                    // Get the type argument list syntax for the named type syntax
                    var typeArgumentListSyntax = namedTypeSyntax.TypeArgumentList;

                    // If the type argument list syntax exists and contains an argument, return the type syntax for the argument
                    if (typeArgumentListSyntax?.Arguments.Any() == true)
                    {
                        return typeArgumentListSyntax.Arguments[0];
                    }
                }
            }

            // If the type is not Task<T> or ValueTask<T>, return the type syntax itself
            return typeSyntax;
        }

        public static TypeSyntax UnwrapNullable(TypeSyntax typeSyntax)
        {
            if (typeSyntax is NullableTypeSyntax namedTypeSyntax)
            {
                return namedTypeSyntax.ElementType;
            }

            return typeSyntax;
        }

        public static bool IsValueTaskOrTask(TypeSyntax typeSyntax)
        {
            // Check if the type is a named type syntax
            if (typeSyntax is GenericNameSyntax namedTypeSyntax)
            {
                // Check if the name of the type is "ValueTask" or "Task"
                if (namedTypeSyntax.Identifier.ValueText is "ValueTask" or "Task")
                {
                    // Check if the named type syntax has type arguments
                    var typeArgumentListSyntax = namedTypeSyntax.TypeArgumentList;
                    if (typeArgumentListSyntax?.Arguments.Any() == true)
                    {
                        return true;
                    }
                }
            }

            // If the type is not a named type syntax for ValueTask<T> or Task<T>, return false
            return false;
        }


        //source: https://andrewlock.net/creating-a-source-generator-part-5-finding-a-type-declarations-namespace-and-type-hierarchy/
        // determine the namespace the class/enum/struct is declared in, if any
        private static string GetNamespace(BaseTypeDeclarationSyntax syntax)
        {
            // If we don't have a namespace at all we'll return an empty string
            // This accounts for the "default namespace" case
            string nameSpace = string.Empty;

            // Get the containing syntax node for the type declaration
            // (could be a nested type, for example)
            SyntaxNode? potentialNamespaceParent = syntax.Parent;

            // Keep moving "out" of nested classes etc until we get to a namespace
            // or until we run out of parents
            while (potentialNamespaceParent != null &&
                   potentialNamespaceParent is not NamespaceDeclarationSyntax
                   && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
            {
                potentialNamespaceParent = potentialNamespaceParent.Parent;
            }

            // Build up the final namespace by looping until we no longer have a namespace declaration
            if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
            {
                // We have a namespace. Use that as the type
                nameSpace = namespaceParent.Name.ToString();

                // Keep moving "out" of the namespace declarations until we 
                // run out of nested namespace declarations
                while (true)
                {
                    if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                    {
                        break;
                    }

                    // Add the outer namespace as a prefix to the final namespace
                    nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                    namespaceParent = parent;
                }
            }

            // return the final namespace
            return nameSpace;
        }

        public static ParentClass? GetParentClasses(BaseTypeDeclarationSyntax typeSyntax)
        {
            // Try and get the parent syntax. If it isn't a type like class/struct, this will be null
            var parentSyntax = typeSyntax.Parent as TypeDeclarationSyntax;
            ParentClass? parentClassInfo = null;

            // Keep looping while we're in a supported nested type
            while (parentSyntax != null && IsAllowedKind(parentSyntax.Kind()))
            {
                // Record the parent type keyword (class/struct etc), name, and constraints
                parentClassInfo = new ParentClass(
                    parentSyntax.Keyword.ValueText,
                    parentSyntax.Identifier.ToString() + parentSyntax.TypeParameterList,
                    parentSyntax.ConstraintClauses.ToString(),
                    parentClassInfo); // set the child link (null initially)

                // Move to the next outer type
                parentSyntax = parentSyntax.Parent as TypeDeclarationSyntax;
            }

            // return a link to the outermost parent type
            return parentClassInfo;
        }

        public static string GetResource(string nameSpace, ParentClass? parentClass)
        {
            var sb = new StringBuilder();

            // If we don't have a namespace, generate the code in the "default"
            // namespace, either global:: or a different <RootNamespace>
            bool hasNamespace = !string.IsNullOrEmpty(nameSpace);
            if (hasNamespace)
                // We could use a file-scoped namespace here which would be a little impler, 
                // but that requires C# 10, which might not be available. 
                // Depends what you want to support!
                sb
                    .Append("namespace ")
                    .Append(nameSpace)
                    .AppendLine(@"
    {");

            var parentsCount = 0;
            // Loop through the full parent type hiearchy, starting with the outermost
            while (parentClass is not null)
            {
                sb
                    .Append("    partial ")
                    .Append(parentClass.Keyword) // e.g. class/struct/record
                    .Append(' ')
                    .Append(parentClass.Name) // e.g. Outer/Generic<T>
                    .Append(' ')
                    .Append(parentClass.Constraints) // e.g. where T: new()
                    .AppendLine(@"
        {");
                parentsCount++; // keep track of how many layers deep we are
                parentClass = parentClass.Child; // repeat with the next child
            }

            // Write the actual target generation code here. Not shown for brevity
            sb.AppendLine(@"public partial readonly struct TestId
    {
    }");

            // We need to "close" each of the parent types, so write
            // the required number of '}'
            for (var i = 0; i < parentsCount; i++) sb.AppendLine(@"    }");

            // Close the namespace, if we had one
            if (hasNamespace) sb.Append('}').AppendLine();

            return sb.ToString();
        }

        public static bool IsAllowedKind(SyntaxKind kind)
        {
            return kind == SyntaxKind.ClassDeclaration ||
                   kind == SyntaxKind.StructDeclaration ||
                   kind == SyntaxKind.RecordDeclaration;
        }
    }

    public class ObjectControllerDefinition: IEquatable<ObjectControllerDefinition>
    {
        public string? Namespace { get; init; }

        public string TargetType { get; init; }

        public List<ObjectPropertyDefinition> Properties { get; set; } = new List<ObjectPropertyDefinition>();

        public List<ObjectMethodDefinition>  Methods { get; set;  } = new List<ObjectMethodDefinition>();
       
        public ParentClass? ParentClass { get; set;  }

        public bool IsStatic { get; init; }

        public List<string> Usings { get; set; } = new List<string>();

        public virtual bool Equals(ObjectControllerDefinition? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Namespace == other.Namespace
                   && TargetType == other.TargetType
                   && Properties.SequenceEqual(other.Properties, EqualityComparer<ObjectPropertyDefinition>.Default)
                   && Methods.SequenceEqual(other.Methods, EqualityComparer<ObjectMethodDefinition>.Default)
                   && Usings.SequenceEqual(other.Usings, EqualityComparer<string>.Default);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Namespace != null ? Namespace.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TargetType.GetHashCode();
                hashCode = (hashCode * 397) ^ Properties.GetHashCode();
                hashCode = (hashCode * 397) ^ Methods.GetHashCode();
                hashCode = (hashCode * 397) ^ Usings.GetHashCode();
                hashCode = (hashCode * 397) ^ (ParentClass != null ? ParentClass.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public record ObjectPropertyDefinition
    {
        public string Name { get; init; }

        public string ReturnType { get; init; }

        public bool IsNullable { get; init; }
        public string ClosestMatchingGraphQLTypeName { get; set; }
    }

    public record ObjectMethodDefinition
    {
        public string Name { get; set; }

        public bool IsAsync { get; set; }

        public List<ParameterDefinition> Parameters { get; set; } = new List<ParameterDefinition>();
        
        public string ReturnType { get; init; }

        public string ClosestMatchingGraphQLTypeName { get; set; }
    }

    public record ParameterDefinition
    {
        public string Name { get; init; }

        public string Type { get; init; }

        public bool IsNullable { get; set; } = false;

        public bool? FromServices { get; set; }

        public bool? FromArguments { get; set; }

        public bool IsPrimitive { get; init; }
        public string ClosestMatchingGraphQLTypeName { get; set; }
    }


}
