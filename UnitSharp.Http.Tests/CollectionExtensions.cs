using System.Collections.Generic;
using System.Linq;

namespace UnitSharp.Http
{
    public static class CollectionExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> xs)
        {
            return from x in xs
                   orderby TestAssembly.Random.Next()
                   select x;
        }
    }
}
