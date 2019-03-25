using System.Collections.Generic;

namespace EBL.Http.Extensions
{
    public static class ListExtensions
    {
        public static IReadOnlyList<TValue> Slice<TValue>(this IReadOnlyList<TValue> list, int start = 0, int? count = null)
        {
            return new Slice<TValue>(list, start, count);
        }

        public static IReadOnlyList<T> AsReadOnly<T>(this IList<T> list, int start = 0, int? count = null)
        {
            return new Slice<T>(list, start, count);
        }
    }
}
