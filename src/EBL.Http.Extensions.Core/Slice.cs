using System;
using System.Collections;
using System.Collections.Generic;

namespace EBL.Http.Extensions
{
    public class Slice<TValue> : IReadOnlyList<TValue>
    {
        private readonly IReadOnlyList<TValue> _source;
        private readonly int _start;
        private readonly int _count;

        public Slice(IList<TValue> source, int start = 0, int? count = null)
            :this(source.AsReadOnly(), start, count)
        {
        }

        public Slice(IReadOnlyList<TValue> source, int start = 0, int? count = null)
        {
            if (start < 0) throw new ArgumentException("start index must be greater or equal to zero");
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (start > source.Count - 1) throw new ArgumentException($"start index must be lower than {nameof(source)} upper bound index");
            if (count < 0) throw new ArgumentException($"count must be greater or equal to zero");
            if (count > source.Count - start) throw new ArgumentException($"count must be lower than {nameof(source)} count from {nameof(start)}");
            _source = source;
            _start = start;
            _count = count ?? source.Count - start;
        }

        public TValue this[int index] => _source[index + _start];

        public int Count => _count;

        public IEnumerator<TValue> GetEnumerator()
        {
            int index = -_start;
            foreach (TValue item in _source)
            {
                if (index >= 0 && index < _count)
                {
                    yield return item;
                }
                if (index >= _count)
                {
                    yield break;
                }
                ++index;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString()
        {
            return $"[{_start}:{_count}]";
        }
    }
}
