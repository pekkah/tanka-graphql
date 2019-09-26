using System;

namespace Tanka.GraphQL.TypeSystem
{
    public interface INamedType: IType
    {
        string Name { get; }
    }
}