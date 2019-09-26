using System;

namespace Tanka.GraphQL.Extensions.Tracing
{
    internal static class TimeSpanExtensions
    {
        public static long TotalNanoSeconds(this TimeSpan timeSpan)
        {
            return (long)(timeSpan.TotalMilliseconds * 1000 * 1000);
        }
    }
}