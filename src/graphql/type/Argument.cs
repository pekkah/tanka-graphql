using System;

namespace fugu.graphql.type
{
    public class Argument
    {
        public IGraphQLType Type { get; set; }

        public object DefaultValue { get; set; }

        public Meta Meta { get; set; }

        public static Argument Arg(IGraphQLType type, object defaultValue = null, Meta meta = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return new Argument
            {
                Type = type,
                DefaultValue = defaultValue,
                Meta = meta ?? new Meta()
            };
        }
    }
}