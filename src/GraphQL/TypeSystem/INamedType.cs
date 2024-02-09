using System.Diagnostics.CodeAnalysis;

namespace Tanka.GraphQL.TypeSystem;

public interface INamedType
{
    [SuppressMessage("ReSharper", "InconsistentNaming")] 
    public string __Typename { get; }
}