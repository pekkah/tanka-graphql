using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tanka.GraphQL.Server.Tests;

public static class AsyncExtensions
{
    public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(
        this IEnumerable<T> source)
    {
        return Enumerator(source);

        static async IAsyncEnumerable<T> Enumerator(
            IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                yield return item;
            }

            await Task.Delay(0);
        }
    }
}