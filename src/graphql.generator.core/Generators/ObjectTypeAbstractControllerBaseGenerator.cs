using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Core.Generators
{
    public class ObjectTypeAbstractControllerBaseGenerator
    {
        private static readonly List<string> RootObjectTypeNames = new List<string>
        {
            "Query",
            "Mutation",
            "Subscription"
        };

        private readonly ObjectType _objectType;
        private readonly SchemaBuilder _schema;

        public ObjectTypeAbstractControllerBaseGenerator(ObjectType objectType, SchemaBuilder schema)
        {
            _objectType = objectType;
            _schema = schema;
        }

        public MemberDeclarationSyntax Generate()
        {
            var interfaceName = _objectType.Name.ToControllerName().ToInterfaceName();
            var modelName = _objectType.Name.ToModelName();
            var name = $"{_objectType.Name.ToControllerName()}Base";

            return ClassDeclaration(name)
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.AbstractKeyword)
                    )
                )
                .WithTypeParameterList(
                    TypeParameterList(
                        SingletonSeparatedList(
                            TypeParameter(
                                Identifier("T")))))
                .WithBaseList(
                    BaseList(
                        SingletonSeparatedList<BaseTypeSyntax>(
                            SimpleBaseType(IdentifierName(interfaceName))
                        )
                    )
                )
                .WithConstraintClauses(
                    SingletonList(
                        TypeParameterConstraintClause(
                                IdentifierName("T"))
                            .WithConstraints(
                                SingletonSeparatedList<TypeParameterConstraintSyntax>(
                                    TypeConstraint(
                                        IdentifierName(modelName))))))
                .WithMembers(List(GenerateFields(_objectType, _schema)));
        }

        public static bool IsAbstract(SchemaBuilder schema, ComplexType ownerType, KeyValuePair<string, IField> field)
        {
            return CodeModel.IsAbstract(schema, ownerType, field);
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateFields(ObjectType objectType, SchemaBuilder schema)
        {
            var members = _schema.GetFields(objectType)
                .SelectMany(field => GenerateField(objectType, field, schema))
                .ToList();

            return members;
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateField(
            ObjectType objectType,
            KeyValuePair<string, IField> field,
            SchemaBuilder schema)
        {
            var isSubscription = _schema.IsSubscriptionType(objectType);

            if (isSubscription)
                foreach (var member in GenerateSubscriptionField(field))
                    yield return member;

            //  Query or Mutation or Subscription resolver
            var methodName = field.Key.ToFieldResolverName();
            var returnType = nameof(IResolverResult);

            yield return MethodDeclaration(
                    GenericName(Identifier(nameof(ValueTask)))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    IdentifierName(returnType)))),
                    Identifier(methodName))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.VirtualKeyword),
                        Token(SyntaxKind.AsyncKeyword)))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(
                                    Identifier("context"))
                                .WithType(
                                    IdentifierName(nameof(IResolverContext))))))
                .WithBody(Block(WithFieldMethodBody(objectType, field, methodName)))
                .WithTrailingTrivia(CarriageReturnLineFeed);

            if (IsAbstract(schema, objectType, field)
                || RootObjectTypeNames.Contains(_objectType.Name))
                yield return WithAbstractFieldMethod(methodName, objectType, field);
            else
                yield return WithPropertyFieldMethod(methodName, objectType, field);
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateSubscriptionField(KeyValuePair<string, IField> field)
        {
            var methodName = field.Key.ToFieldResolverName();
            var returnType = nameof(ISubscriberResult);

            yield return WithSubscriberMethod(returnType, methodName, field);

            yield return WithAbstractSubscriberMethod(returnType, methodName, field);
        }

        private MethodDeclarationSyntax WithSubscriberMethod(
            string returnType,
            string methodName,
            KeyValuePair<string, IField> field)
        {
            return MethodDeclaration(
                    GenericName(
                            Identifier("ValueTask"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    IdentifierName(returnType)))),
                    Identifier(methodName))
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.VirtualKeyword),
                        Token(SyntaxKind.AsyncKeyword)))
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Parameter(
                                        Identifier("context"))
                                    .WithType(
                                        IdentifierName(nameof(IResolverContext))),
                                Token(SyntaxKind.CommaToken),
                                Parameter(
                                        Identifier("unsubscribe"))
                                    .WithType(
                                        IdentifierName("CancellationToken"))
                            })))
                .WithBody(
                    Block(
                        LocalDeclarationStatement(
                            VariableDeclaration(
                                    IdentifierName("var"))
                                .WithVariables(
                                    SingletonSeparatedList(
                                        VariableDeclarator(
                                                Identifier("objectValue"))
                                            .WithInitializer(
                                                EqualsValueClause(
                                                    BinaryExpression(
                                                        SyntaxKind.AsExpression,
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("context"),
                                                            IdentifierName("ObjectValue")),
                                                        IdentifierName("T"))))))),
                        LocalDeclarationStatement(
                            VariableDeclaration(
                                    IdentifierName("var"))
                                .WithVariables(
                                    SingletonSeparatedList(
                                        VariableDeclarator(
                                                Identifier("resultTask"))
                                            .WithInitializer(
                                                EqualsValueClause(
                                                    InvocationExpression(
                                                            IdentifierName(methodName))
                                                        .WithArgumentList(
                                                            ArgumentList(
                                                                SeparatedList<ArgumentSyntax>(
                                                                    WithSubscriptionArguments(field))))))))),
                        IfStatement(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("resultTask"),
                                IdentifierName("IsCompletedSuccessfully")),
                            ReturnStatement(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("resultTask"),
                                    IdentifierName("Result")))),
                        LocalDeclarationStatement(
                            VariableDeclaration(
                                    IdentifierName("var"))
                                .WithVariables(
                                    SingletonSeparatedList(
                                        VariableDeclarator(
                                                Identifier("result"))
                                            .WithInitializer(
                                                EqualsValueClause(
                                                    AwaitExpression(
                                                        IdentifierName("resultTask"))))))),
                        ReturnStatement(
                            IdentifierName("result"))));
        }

        private IEnumerable<SyntaxNodeOrToken> WithSubscriptionArguments(KeyValuePair<string, IField> field)
        {
            yield return Argument(IdentifierName("objectValue"));
            yield return Token(SyntaxKind.CommaToken);

            foreach (var arg in field.Value.Arguments)
            {
                yield return WithArgument(arg);
                yield return Token(SyntaxKind.CommaToken);
            }

            yield return Argument(IdentifierName("unsubscribe"));
            yield return Token(SyntaxKind.CommaToken);

            yield return Argument(IdentifierName("context"));
        }

        private MethodDeclarationSyntax WithAbstractSubscriberMethod(string returnType, string methodName,
            KeyValuePair<string, IField> field)
        {
            return MethodDeclaration(
                    GenericName(
                            Identifier("ValueTask"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    IdentifierName(returnType)))),
                    Identifier(methodName))
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AbstractKeyword)))
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(
                            WithSubscriptionParameters(field))))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
        }

        private IEnumerable<SyntaxNodeOrToken> WithSubscriptionParameters(KeyValuePair<string, IField> field)
        {
            yield return Parameter(
                    Identifier("objectValue"))
                .WithType(
                    IdentifierName("T?"));

            yield return Token(SyntaxKind.CommaToken);

            foreach (var argumentDefinition in field.Value.Arguments)
            {
                yield return WithParameter(argumentDefinition);
                yield return Token(SyntaxKind.CommaToken);
            }

            yield return Parameter(
                    Identifier("unsubscribe"))
                .WithType(
                    IdentifierName("CancellationToken"));

            yield return Token(SyntaxKind.CommaToken);

            yield return Parameter(
                    Identifier("context"))
                .WithType(
                    IdentifierName(nameof(IResolverContext)));
        }

        private IEnumerable<StatementSyntax> WithFieldMethodBody(
            ObjectType objectType,
            KeyValuePair<string, IField> field,
            string methodName)
        {
            var asType = "T";

            var isSubscription = _schema.IsSubscriptionType(objectType);

            if (isSubscription) asType = "object";

            var unwrappedFieldType = field.Value.Type.Unwrap();

            yield return LocalDeclarationStatement(
                VariableDeclaration(
                        IdentifierName("var"))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(
                                    Identifier("objectValue"))
                                .WithInitializer(
                                    EqualsValueClause(
                                        CastExpression(
                                            IdentifierName(asType),
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName("context"),
                                                IdentifierName("ObjectValue"))
                                        ))))));


            if (!RootObjectTypeNames.Contains(_objectType.Name))
                yield return IfStatement(
                        BinaryExpression(
                            SyntaxKind.EqualsExpression,
                            IdentifierName("objectValue"),
                            LiteralExpression(
                                SyntaxKind.NullLiteralExpression)),
                        ReturnStatement(
                            InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("Resolve"),
                                        IdentifierName("As")))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList(
                                            Argument(
                                                LiteralExpression(
                                                    SyntaxKind.NullLiteralExpression)))))))
                    .WithIfKeyword(
                        Token(
                            TriviaList(
                                Comment("// if parent field was null this should never run")),
                            SyntaxKind.IfKeyword,
                            TriviaList()));

            yield return LocalDeclarationStatement(
                VariableDeclaration(
                        IdentifierName("var"))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(
                                    Identifier("resultTask"))
                                .WithInitializer(
                                    EqualsValueClause(
                                        InvocationExpression(
                                                IdentifierName(methodName))
                                            .WithArgumentList(
                                                ArgumentList(
                                                    SeparatedList<ArgumentSyntax>(
                                                        WithArguments(objectType, field)))))))));

            // todo: refactor this whole mess and fix this to work with interfaces and unions
            if (!(unwrappedFieldType is InterfaceType) && !(unwrappedFieldType is UnionType))
            {
                yield return IfStatement(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("resultTask"),
                        IdentifierName("IsCompletedSuccessfully")),
                    ReturnStatement(
                        InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("Resolve"),
                                    IdentifierName("As")))
                            .WithArgumentList(
                                ArgumentList(
                                    SingletonSeparatedList(
                                        Argument(
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName("resultTask"),
                                                IdentifierName("Result"))))))));
            }

            yield return LocalDeclarationStatement(
                VariableDeclaration(
                        IdentifierName("var"))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(
                                    Identifier("result"))
                                .WithInitializer(
                                    EqualsValueClause(
                                        AwaitExpression(
                                            IdentifierName("resultTask")))))));

            if (unwrappedFieldType is InterfaceType || unwrappedFieldType is UnionType)
            {
                var modelName = unwrappedFieldType.Name.ToModelInterfaceName();
                var modelControllerName = unwrappedFieldType.Name
                    .ToModelInterfaceName()
                    .ToControllerName();

                if (field.Value.Type.IsList())
                {
                    yield return ReturnStatement(
                        ObjectCreationExpression(
                                IdentifierName("CompleteValueResult"))
                            .WithArgumentList(
                                ArgumentList(
                                    SeparatedList<ArgumentSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                            Argument(
                                                IdentifierName("result")),
                                            Token(SyntaxKind.CommaToken),
                                            Argument(
                                                SimpleLambdaExpression(
                                                    Parameter(
                                                        Identifier("item")),
                                                    InvocationExpression(
                                                            MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                InvocationExpression(
                                                                    MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        IdentifierName("context"),
                                                                        GenericName(
                                                                                Identifier("Use"))
                                                                            .WithTypeArgumentList(
                                                                                TypeArgumentList(
                                                                                    SingletonSeparatedList<TypeSyntax>(
                                                                                        IdentifierName(
                                                                                            modelControllerName)))))),
                                                                IdentifierName("IsTypeOf")))
                                                        .WithArgumentList(
                                                            ArgumentList(SeparatedList<ArgumentSyntax>(
                                                                new SyntaxNodeOrToken[]{
                                                                    Argument(BinaryExpression(
                                                                        SyntaxKind.AsExpression,
                                                                        IdentifierName("item"),
                                                                        IdentifierName(modelName))),
                                                                    Token(SyntaxKind.CommaToken),
                                                                    Argument(
                                                                        MemberAccessExpression(
                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                            MemberAccessExpression(
                                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                                IdentifierName("context"),
                                                                                IdentifierName("ExecutionContext")),
                                                                            IdentifierName("Schema")))})))))
                                        }))));
                    yield break;
                }

                yield return ReturnStatement(
                    ObjectCreationExpression(
                            IdentifierName("CompleteValueResult"))
                        .WithArgumentList(
                            ArgumentList(
                                SeparatedList<ArgumentSyntax>(
                                    new SyntaxNodeOrToken[]
                                    {
                                        Argument(
                                            IdentifierName("result")),
                                        Token(SyntaxKind.CommaToken),
                                        Argument(
                                            InvocationExpression(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        InvocationExpression(
                                                            MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                IdentifierName("context"),
                                                                GenericName(
                                                                        Identifier("Use"))
                                                                    .WithTypeArgumentList(
                                                                        TypeArgumentList(
                                                                            SingletonSeparatedList<TypeSyntax>(
                                                                                IdentifierName(
                                                                                    modelControllerName)))))),
                                                        IdentifierName("IsTypeOf")))
                                                .WithArgumentList(
                                                    ArgumentList(
                                                        SeparatedList<ArgumentSyntax>(
                                                            new SyntaxNodeOrToken[]{
                                                                Argument(
                                                                    IdentifierName("result")),
                                                                Token(SyntaxKind.CommaToken),
                                                                Argument(
                                                                    MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        MemberAccessExpression(
                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                            IdentifierName("context"),
                                                                            IdentifierName("ExecutionContext")),
                                                                        IdentifierName("Schema")))})))
                                    )}))));
                yield break;
            }

            yield return ReturnStatement(
                InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("Resolve"),
                            IdentifierName("As")))
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    IdentifierName("result"))))));
        }

        private IEnumerable<SyntaxNodeOrToken> WithArguments(
            ObjectType objectType,
            KeyValuePair<string, IField> fieldDefinition)
        {
            yield return Argument(IdentifierName("objectValue"));

            var arguments = fieldDefinition.Value.Arguments;

            foreach (var argumentDefinition in arguments)
            {
                yield return Token(SyntaxKind.CommaToken);
                yield return WithArgument(argumentDefinition);
            }

            yield return Token(SyntaxKind.CommaToken);

            yield return Argument(IdentifierName("context"));
        }

        private SyntaxNodeOrToken WithArgument(KeyValuePair<string, Argument> argumentDefinition)
        {
            var rawArgumentName = argumentDefinition.Key;
            var argument = argumentDefinition.Value;
            var typeName = CodeModel.SelectTypeName(argument.Type);

            var isInputObject = argument.Type.Unwrap() is InputObjectType;

            string getArgumentMethodName;
            if (isInputObject)
            {
                if (argument.Type is List)
                {
                    typeName = CodeModel.SelectTypeName(argument.Type.Unwrap());
                    getArgumentMethodName = nameof(ResolverContextExtensions.GetObjectArgumentList);
                }
                else
                {
                    getArgumentMethodName = nameof(ResolverContextExtensions.GetObjectArgument);
                }
            }
            else
            {
                getArgumentMethodName = nameof(ResolverContextExtensions.GetArgument);
            }

            var getArgumentValue = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("context"),
                        GenericName(
                                Identifier(getArgumentMethodName))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(
                                        IdentifierName(typeName))))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal(rawArgumentName))))));

            return Argument(getArgumentValue);
        }

        private MethodDeclarationSyntax WithAbstractFieldMethod(
            string methodName,
            ObjectType objectType,
            KeyValuePair<string, IField> field)
        {
            var resultTypeName = CodeModel.SelectFieldTypeName(_schema, _objectType, field);

            return MethodDeclaration(
                    GenericName(
                            Identifier(nameof(ValueTask)))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    IdentifierName(resultTypeName)))),
                    Identifier(methodName))
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AbstractKeyword)))
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(
                            WithParameters(objectType, field))))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
        }

        private IEnumerable<SyntaxNodeOrToken> WithParameters(
            ObjectType objectType,
            KeyValuePair<string, IField> field)
        {
            var isSubscription = _schema.IsSubscriptionType(objectType);

            if (!isSubscription)
            {
                if (RootObjectTypeNames.Contains(objectType.Name))
                    yield return Parameter(Identifier("objectValue"))
                        .WithType(IdentifierName("T?"));
                else
                    yield return Parameter(Identifier("objectValue"))
                        .WithType(IdentifierName("T"));
            }
            else
            {
                /*var subscriptionType = CodeModel.SelectFieldTypeName(
                    _schema,
                    _objectType,
                    field);*/

                yield return Parameter(Identifier("objectValue"))
                    .WithType(IdentifierName("object"));
            }

            var arguments = field.Value.Arguments;

            foreach (var argumentDefinition in arguments)
            {
                yield return Token(SyntaxKind.CommaToken);
                yield return WithParameter(argumentDefinition);
            }

            yield return Token(SyntaxKind.CommaToken);

            yield return Parameter(Identifier("context"))
                .WithType(IdentifierName(nameof(IResolverContext)));
        }

        private static SyntaxNodeOrToken WithParameter(
            KeyValuePair<string, Argument> argumentDefinition)
        {
            var argumentName = argumentDefinition.Key.ToFieldArgumentName();
            var argument = argumentDefinition.Value;
            var typeName = CodeModel.SelectTypeName(argument.Type);

            return Parameter(Identifier(argumentName))
                .WithType(ParseTypeName(typeName));
        }

        private MethodDeclarationSyntax WithPropertyFieldMethod(
            string methodName,
            ObjectType objectType,
            KeyValuePair<string, IField> field)
        {
            var resultTypeName = CodeModel.SelectFieldTypeName(_schema, _objectType, field);
            return MethodDeclaration(
                    GenericName(
                            Identifier(nameof(ValueTask)))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    IdentifierName(resultTypeName)))),
                    Identifier(methodName))
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.VirtualKeyword)))
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Parameter(
                                        Identifier("objectValue"))
                                    .WithType(
                                        IdentifierName("T")),
                                Token(SyntaxKind.CommaToken),
                                Parameter(
                                        Identifier("context"))
                                    .WithType(
                                        IdentifierName(nameof(IResolverContext)))
                            })))
                .WithBody(
                    Block(
                        SingletonList<StatementSyntax>(
                            ReturnStatement(
                                ObjectCreationExpression(
                                        GenericName(
                                                Identifier(nameof(ValueTask)))
                                            .WithTypeArgumentList(
                                                TypeArgumentList(
                                                    SingletonSeparatedList<TypeSyntax>(
                                                        IdentifierName(resultTypeName)))))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList(
                                                Argument(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        IdentifierName("objectValue"),
                                                        IdentifierName(methodName))))))))));
        }
    }
}