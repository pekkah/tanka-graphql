using System.Linq;
using fugu.graphql.introspection;
using fugu.graphql.type;
using Xunit;

namespace fugu.graphql.tests.introspection
{
    public class ExamineObjectFacts
    {
        private readonly Schema _schema;
        private InterfaceType _implementedInterface;
        private ObjectType _objectType;

        public ExamineObjectFacts()
        {
            _implementedInterface = new InterfaceType(
                "Interface",
                fields: new Fields
                {
                    ["field"] = new Field(ScalarType.Int, new Args()
                    {
                        ["arg"] = Argument.Arg(ScalarType.String, "0")
                    })
                });

            _objectType = new ObjectType(
                "Object",
                implements:new [] {_implementedInterface},
                fields: new Fields()
                {
                    ["field"] = new Field(ScalarType.Int, new Args()
                    {
                        ["arg"] = Argument.Arg(ScalarType.String, "0")
                    })
                });

            _schema = new Schema(
                new ObjectType("Query",
                    new Fields
                    {
                        ["object"] = new Field(_objectType)
                    }));

            _schema.InitializeAsync().Wait();
        }
    }
}