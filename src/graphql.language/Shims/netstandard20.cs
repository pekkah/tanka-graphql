using System.Collections.Generic;

#if !NETSTANDARD2_1
namespace System
{
    internal static class Extensions
    {
        public static void Deconstruct<K, V>(this KeyValuePair<K, V> pair, out K key, out V value)
        {
            key = pair.Key;
            value = pair.Value;
        }

        public static bool TryPop<T>(this Stack<T> stack, out T? value)
        {
            if (stack.Count == 0)
            {
                value = default;
                return false;
            }

            value = stack.Pop();
            return true;
        }

        public static bool TryPeek<T>(this Stack<T> stack, out T? value)
        {
            if (stack.Count == 0)
            {
                value = default;
                return false;
            }

            value = stack.Peek();
            return true;
        }
    }

    namespace Text
    {
        internal static class EncodingExtensions
        {
            public static string GetString(this Encoding encoding, in ReadOnlySpan<byte> span)
            {
                return encoding.GetString(span.ToArray());
            }
        }
    }
}
#endif