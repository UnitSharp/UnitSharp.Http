using System.Collections.Generic;

namespace UnitSharp.Http
{
    internal static class InternalExtensions
    {
        public static void Deconstruct<TKey, TValue>(
            this KeyValuePair<TKey, TValue> pair,
            out TKey key,
            out TValue value)
        {
            (key, value) = (pair.Key, pair.Value);
        }
    }
}
