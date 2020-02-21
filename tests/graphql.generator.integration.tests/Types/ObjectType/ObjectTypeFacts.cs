using System;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Generator.Integration.Tests.Types.ObjectType
{
    public partial class ObjectType
    {
    }

    public class ObjectTypeController : ObjectTypeControllerBase<ObjectType>
    {
        public override ValueTask<int> Method(ObjectType objectValue, int arg1, IResolverContext context)
        {
            throw new NotImplementedException();
        }

        public override ValueTask<int?> Method2(ObjectType objectValue, int? arg1, IResolverContext context)
        {
            throw new NotImplementedException();
        }
    }

    public class ObjectTypeFacts
    {
    }
}