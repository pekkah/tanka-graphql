using System;

namespace Tanka.GraphQL.DTOs
{
    [Obsolete("todo: this is a migration clutch")]
    public static class DefaultJsonSerializer 
    {
        public static ISerializer Serializer { get; } = new Serializer();
    }
}