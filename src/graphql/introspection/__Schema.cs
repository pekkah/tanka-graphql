using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.type;
// ReSharper disable InconsistentNaming

namespace tanka.graphql.introspection
{
    public class __Schema
    {
        public IEnumerable<__Type> GetTypes(ISchema schema)
        {
            var types = schema.QueryTypes<IType>()
                .Distinct(new GraphQLTypeComparer());

            return types.Select(t => Examiner.Examine(t, schema));
        }

        public __Type GetType(string name, ISchema schema)
        {
            var type = schema.GetNamedType(name);

            if (type == null)
                return null;

            return Examiner.Examine(type, schema);
        }

        public IEnumerable<__Type> GetPossibleTypes(string name, ISchema schema)
        {
            var type = schema.GetNamedType(name);

            if (type is InterfaceType interfaceType)
            {
                var possibleTypes = schema.QueryTypes<ObjectType>(o => o.Implements(interfaceType));
                return possibleTypes.Select(p => Examiner.Examine(p, schema));
            }

            if (type is UnionType unionType)
            {
                var possibleTypes = schema
                    .QueryTypes<ObjectType>(t => unionType.IsPossible(t));

                return possibleTypes.Select(p => Examiner.Examine(p, schema));
            }

            return null;
        }

        public IEnumerable<__Type> GetInterfacesOf(__Type type, ISchema data)
        {
            if (type.Interfaces == null)
                yield break;

            foreach (var interfaceRef in type.Interfaces)
            {
                var interfaceType = data.GetNamedType<InterfaceType>(interfaceRef);
                yield return Examiner.Examine(interfaceType, data);
            }
        }

        private INamedType UnwindTypeRef(TypeRef typeRef, ISchema data)
        {
            return data.GetNamedType(typeRef.Name);
        }

        /// <summary>
        ///     Resolve named type reference into an type
        /// </summary>
        /// <param name="type">Named type reference chain</param>
        /// <param name="schema"></param>
        /// <returns></returns>
        public __Type GetType(IType type, ISchema schema)
        {
            var __type = Examiner.Examine(type, schema);
            return __type;
        }

        public IEnumerable<__Directive> GetDirectives(ISchema data)
        {
            var directives = data.QueryDirectives();
            return directives.Select(d => Examiner.ExamineDirective(d, data));
        }
    }
}