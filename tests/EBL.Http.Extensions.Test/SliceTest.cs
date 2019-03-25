using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EBL.Http.Extensions.Test
{
    [TestFixture]
    public class SliceTest
    {
        private IReadOnlyList<int> _readOnlyList;
        private int[] _list;
        private const int size = 10;

        [SetUp]
        public void Setup()
        {
            _readOnlyList = Enumerable.Range(0, size).ToArray();
            _list = Enumerable.Range(0, size).ToArray();
        }

        [Test]
        public void default_start_index_eqauls_zero()
        {
            var slice = new Slice<int>(_readOnlyList);
            Assert.AreEqual(0, slice[0], "unexpected default start index");
        }

        [Test]
        public void default_count_is_source_count()
        {
            var slice = new Slice<int>(_readOnlyList);
            Assert.AreEqual(_readOnlyList.Count, slice.Count, "unexpected default count");
        }

        [Test]
        public void raise_argumentexception_when_start_index_is_out_of_source_bounds([Values(-2,-1,10,11)]int start)
        {
            Assert.Throws<ArgumentException>(() => new Slice<int>(_readOnlyList, start));
        }

        [Test]
        public void raise_argumentexception_when_count_is_out_of_source_bounds([Values(-1, 11)]int count)
        {
            Assert.Throws<ArgumentException>(() => new Slice<int>(_readOnlyList, count:count));
        }

        [Test]
        [TestCase(1, 10)]
        [TestCase(2, 10)]
        [TestCase(5, 6)]
        [TestCase(9, 2)]
        public void raise_argumentexception_when_slice_bounds_are_not_valid(int start, int count)
        {
            Assert.Throws<ArgumentException>(() => new Slice<int>(_readOnlyList, start, count));
        }

        [Test]
        public void i_can_set_start_index([Values(0,1,2,3,4,5,6,7,8,9)] int start)
        {
            var slice = new Slice<int>(_readOnlyList, start);
            CollectionAssert.AreEqual(Enumerable.Range(start, size - start), slice, "unexpected slice content");
        }

        [Test]
        public void i_can_set_slice_size([Values(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)] int count)
        {
            var slice = new Slice<int>(_readOnlyList, count:count);
            CollectionAssert.AreEqual(Enumerable.Range(0, count), slice, "unexpected slice content");
        }
    }
}
