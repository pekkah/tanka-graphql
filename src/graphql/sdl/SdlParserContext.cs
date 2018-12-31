using System.Collections.Generic;
using System.Linq;
using tanka.graphql.type;
using GraphQLParser.AST;

namespace tanka.graphql.sdl
{
    public class SdlParserContext
    {
        private readonly Stack<GraphQLObjectTypeDefinition> _objectDefinitions =
            new Stack<GraphQLObjectTypeDefinition>();

        private readonly Stack<GraphQLInterfaceTypeDefinition> _interfaceDefinitions =
            new Stack<GraphQLInterfaceTypeDefinition>();

        public List<IGraphQLType> KnownTypes = new List<IGraphQLType>();

        public SdlParserContext(GraphQLDocument document, IEnumerable<IGraphQLType> knownTypes = null)
        {
            Document = document;

            foreach (var scalarType in ScalarType.Standard)
                KnownTypes.Add(scalarType);

            if (knownTypes != null)
            {
                foreach (var knownType in knownTypes)
                {
                    KnownTypes.Add(knownType);
                }
            }
           
        }

        public GraphQLDocument Document { get; }

        public IGraphQLType GetKnownType(string typeName)
        {
            return KnownTypes.SingleOrDefault(t => t.Name.ToLowerInvariant() == typeName.ToLowerInvariant());
        }

        public void PushObject(GraphQLObjectTypeDefinition definition)
        {
            _objectDefinitions.Push(definition);
        }

        public void PopObject()
        {
            _objectDefinitions.Pop();
        }

        public bool IsBeingBuilt(string typeName)
        {
            if (_objectDefinitions.Any(d => d.Name.Value == typeName))
                return true;

            if (_interfaceDefinitions.Any(d => d.Name.Value == typeName))
                return true;

            return false;
        }

        public void PushInterface(GraphQLInterfaceTypeDefinition definition)
        {
            _interfaceDefinitions.Push(definition);
        }

        public void PopInterface()
        {
            _interfaceDefinitions.Pop();
        }
    }
}