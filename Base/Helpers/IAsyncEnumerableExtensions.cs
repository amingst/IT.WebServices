using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Helpers
{
    public static class IAsyncEnumerableExtensions
    {
        public static async Task<List<TSource>> ToList<TSource>(this IAsyncEnumerable<TSource> source)
        {
            var list = new List<TSource>();

            await foreach (var item in source)
                list.Add(item);

            return list;
        }
    }
}
