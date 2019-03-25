using System;
using System.Collections;
using System.Collections.Generic;

namespace EBL.Http.Extensions
{
    internal class ReadOnlyAdapter<TValue> : IReadOnlyList<TValue>
    {
        private readonly IList<TValue> _list;

        public ReadOnlyAdapter(IList<TValue> list)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            _list = list;
        }

        public static implicit operator ReadOnlyAdapter<TValue>(List<TValue> list)
        {
            return new ReadOnlyAdapter<TValue>(list);
        }

        public static implicit operator ReadOnlyAdapter<TValue>(TValue[] list)
        {
            return new ReadOnlyAdapter<TValue>(list);
        }

        public TValue this[int index] => _list[index];

        public int Count => _list.Count;

        public IEnumerator<TValue> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();
    }
}
