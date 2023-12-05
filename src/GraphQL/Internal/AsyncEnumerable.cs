namespace Tanka.GraphQL;

internal static class AsyncEnumerableEx
{
    public static async Task<T?> SingleOrDefaultAsync<T>(this IAsyncEnumerable<T> source,
        CancellationToken cancellationToken)
    {
        await using IAsyncEnumerator<T> e = source.GetAsyncEnumerator(cancellationToken);

        if (!await e.MoveNextAsync()) return default;

        var value = e.Current;
        
        await ThrowIfMore(e);

        return value;

        static async ValueTask ThrowIfMore(IAsyncEnumerator<T> e)
        {
            if (await e.MoveNextAsync()) 
                throw new InvalidOperationException("Expected single item but got more.");
        }
    }

    public static async Task<T> SingleAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken)
    {
        await using IAsyncEnumerator<T> e = source.GetAsyncEnumerator(cancellationToken);

        if (!await e.MoveNextAsync()) throw new InvalidOperationException("Expected single item but not zero items.");

        var value = e.Current;

        await ThrowIfMore(e);
        
        return value;

        static async ValueTask ThrowIfMore(IAsyncEnumerator<T> e)
        {
            if (await e.MoveNextAsync()) throw new InvalidOperationException("Expected single item but got more.");
        }
    }

    public static IAsyncEnumerable<T> Return<T>(T value) => new ReturnEnumerable<T>(value);

    public static IAsyncEnumerable<T> Empty<T>() => new EmptyEnumerable<T>();

    private sealed class ReturnEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly T _value;

        public ReturnEnumerable(T value) => _value = value;

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested(); // NB: [LDM-2018-11-28] Equivalent to async iterator behavior.

            return new ReturnEnumerator(_value);
        }

        private sealed class ReturnEnumerator : IAsyncEnumerator<T>
        {
            private bool _once;

            public ReturnEnumerator(T current) => Current = current;

            public T Current { get; private set; }

            public ValueTask DisposeAsync()
            {
                Current = default!;
                return default;
            }

            public ValueTask<bool> MoveNextAsync()
            {
                if (_once)
                {
                    return new ValueTask<bool>(false);
                }

                _once = true;
                return new ValueTask<bool>(true);
            }
        }
    }

    private sealed class EmptyEnumerable<T> : IAsyncEnumerable<T>
    {
        public EmptyEnumerable()
        {
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested(); // NB: [LDM-2018-11-28] Equivalent to async iterator behavior.

            return new EmptyEnumerator();
        }

        private sealed class EmptyEnumerator : IAsyncEnumerator<T>
        {
            public ValueTask DisposeAsync()
            {
                return default;
            }

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(false);
            }

            public T Current => default!;
        }
    }
}