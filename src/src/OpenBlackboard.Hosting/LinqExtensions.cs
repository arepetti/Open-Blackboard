using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBlackboard.Hosting
{
    static class LinqExtensions
    {
        public static async Task<IEnumerable<TOut>> SelectManyAsync<TIn, TOut>(this IEnumerable<TIn> enumeration, Func<TIn, Task<IEnumerable<TOut>>> func)
        {
            return (await Task.WhenAll(enumeration.Select(func))).SelectMany(s => s);
        }
    }
}
