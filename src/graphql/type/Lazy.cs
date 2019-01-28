using System;

namespace tanka.graphql.type
{
    public class Lazy : IWrappingType
    {
        private readonly Lazy<IType> _lazyType;

        public Lazy(Lazy<IType> lazyType)
        {
            _lazyType = lazyType;
        }

        public Lazy(Func<IType> lazyFunc)
        {
            _lazyType = new Lazy<IType>(lazyFunc);
        }

        public IType WrappedType => _lazyType.Value;
    }
}