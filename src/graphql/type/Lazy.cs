using System;

namespace fugu.graphql.type
{
    public class Lazy : IWrappingType, IGraphQLType
    {
        private readonly Lazy<IGraphQLType> _lazyType;

        public Lazy(Lazy<IGraphQLType> lazyType)
        {
            _lazyType = lazyType;
        }

        public Lazy(Func<IGraphQLType> lazyFunc)
        {
            _lazyType = new Lazy<IGraphQLType>(lazyFunc);
        }

        public string Name { get; } = null;

        public IGraphQLType WrappedType => _lazyType.Value;
    }
}