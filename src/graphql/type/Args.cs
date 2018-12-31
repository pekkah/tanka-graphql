using System.Collections.Generic;

namespace tanka.graphql.type
{
    public class Args : Dictionary<string, Argument>
    {
        public Args()
        {
            
        }

        public Args(IEnumerable<KeyValuePair<string, Argument>> arguments)
        {
            foreach (var argument in arguments)
            {
                this[argument.Key] = argument.Value;
            }
        }
    }
}