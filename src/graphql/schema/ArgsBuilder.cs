using tanka.graphql.type;

namespace tanka.graphql.schema
{
    public class ArgsBuilder
    {
        private readonly Args _args = new Args();

        public ArgsBuilder Arg(
            string name, 
            IType type, 
            object defaultValue, 
            string description)
        {
            _args.Add(name, type, defaultValue, description);
            return this;
        }

        public Args Build()
        {
            return _args;
        }
    }
}