using System.Collections.Generic;
using System.Linq;

namespace fugu.graphql.introspection
{
    public class __Type
    {
        public __TypeKind Kind { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        ///     OBJECT and INTERFACE only
        /// </summary>
        /// <param name="includeDeprecated"></param>
        /// <returns></returns>
        public IEnumerable<__Field> GetFields(bool includeDeprecated = false)
        {
            if (Fields == null)
                return null;

            if (includeDeprecated)
                return Fields;

            return Fields.Where(f => f.IsDeprecated == false);
        }

        /// <summary>
        ///     OBJECT only
        /// </summary>
        public List<string> Interfaces { get; set; }

        /// <summary>
        ///     ENUM only
        /// </summary>
        /// <param name="includeDeprecated"></param>
        /// <returns></returns>
        public IEnumerable<__EnumValue> GetEnumValues(bool includeDeprecated = false)
        {
            if (EnumValues == null)
                return null;

            if (includeDeprecated)
                return EnumValues;

            return EnumValues.Where(f => f.IsDeprecated == false);
        }

        public List<__EnumValue> EnumValues { get; set; }

        /// <summary>
        ///     INPUT_OBJECT only
        /// </summary>
        public List<__InputValue> InputFields { get; set; }

        /// <summary>
        ///     NON_NULL and LIST only
        /// </summary>
        public __Type OfType { get;set; }

        public List<__Field> Fields { get; set; }
    }
}