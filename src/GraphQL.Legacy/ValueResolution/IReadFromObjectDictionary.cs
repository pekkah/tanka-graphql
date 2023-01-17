using System.Collections.Generic;

namespace Tanka.GraphQL.ValueResolution;

public interface IReadFromObjectDictionary
{
    void Read(IReadOnlyDictionary<string, object> source);
}