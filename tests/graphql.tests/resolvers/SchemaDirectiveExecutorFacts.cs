using System.Collections.Generic;
using System.Threading.Tasks;
using fugu.graphql.sdl;
using fugu.graphql.tools;
using fugu.graphql.type;
using Xunit;

namespace fugu.graphql.tests.resolvers
{
    public abstract class DirectiveVisitorBase : SchemaVisitorBase
    {
        protected DirectiveVisitorBase(ISchema schema, IResolverMap resolverMap, ISubscriberMap subscriberMap) : base(
            schema, resolverMap, subscriberMap)
        {
        }
    }

    public class DeprecatedVisitor : DirectiveVisitorBase
    {
        public DeprecatedVisitor(ISchema schema, IResolverMap resolverMap, ISubscriberMap subscriberMap) : base(schema,
            resolverMap, subscriberMap)
        {
        }

        protected override Task VisitObjectFieldAsync(ObjectType objectType,
            KeyValuePair<string, IField> objectTypeField)
        {
            var deprecated = objectTypeField.Value.GetDirective("deprecated");

            if (deprecated == null)
                return Task.CompletedTask;

            var field = objectTypeField.Value;
            field.Meta = new Meta(field.Meta.Description, deprecated.GetArgument("reason")?.DefaultValue.ToString());

            return Task.CompletedTask;
        }
    }

    public class SchemaDirectiveExecutorFacts
    {
        public SchemaDirectiveExecutorFacts()
        {
            _directiveType = new DirectiveType(
                "deprecated",
                DirectiveType.TypeSystemLocations,
                new Args
                {
                    ["reason"] = new Argument
                    {
                        Type = ScalarType.String,
                        DefaultValue = "No longer supported"
                    }
                });
            _schema = Sdl.Schema(Parser.ParseDocument(@"
type Query {
    deprecated: String @deprecated
    deprecatedWithReason: String @deprecated(reason: ""Reason"")
}

schema {
    query: Query
}
"), directives: new[] {_directiveType});
        }

        private readonly ISchema _schema;
        private readonly DirectiveType _directiveType;


        [Fact]
        public async Task Execute_directive_without_args()
        {
            /* Given */
            SchemaVisitorBase VisitorFactory(ISchema schema, IResolverMap resolvers, ISubscriberMap subscribers)
            {
                return new DeprecatedVisitor(schema, resolvers, subscribers);
            }

            /* When */
            var executable = await SchemaTools.MakeExecutableSchemaAsync(
                _schema,
                new ResolverMap(),
                null,
                new SchemaVisitorFactory[] {VisitorFactory});

            /* Then */
            var deprecatedField = executable.Query.GetField("deprecated");
            Assert.True(deprecatedField.Meta.IsDeprecated);
            Assert.Equal("No longer supported", deprecatedField.Meta.DeprecationReason);
        }
    }
}