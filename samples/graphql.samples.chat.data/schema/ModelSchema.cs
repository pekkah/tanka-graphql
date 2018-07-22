using System.Threading.Tasks;
using fugu.graphql.type;
using static fugu.graphql.type.ScalarType;

namespace fugu.graphql.samples.chat.data.schema
{
    public static class ModelSchema
    {
        public static async Task<ISchema> CreateAsync()
        {
            var member = new ObjectType(
                "Member",
                new Fields
                {
                    ["name"] = new Field(NonNullString)
                });

            var channel = new ObjectType(
                "Channel",
                new Fields
                {
                    ["name"] = new Field(NonNullString),
                    ["members"] = new Field(new NonNull(new List(new NonNull(member))))
                });

            var schema = new type.Schema(
                new ObjectType(
                    "Query",
                    new Fields
                    {
                        ["channels"] = new Field(new NonNull(new List(new NonNull(channel))))
                    }));

            await schema.InitializeAsync();
            return schema;
        }
    }
}