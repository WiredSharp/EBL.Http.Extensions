using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MinimalOwinWebApiSelfHost.Clients;

namespace MinimalOwinWebApiSelfHost.Test
{
    [TestFixture]
    public class DelegatingHandlerExtensionsTest
    {
        [Test]
        public void i_can_reach_handler_at_depth([Values(0,1,2,3,4)] int depth)
        {
            DelegatingHandler[] handlers = CreateHandlers(5);
            DelegatingHandler handlerChain = CreateTopBottomHandler(handlers);
            Assert.AreEqual(handlers[depth], handlerChain.Inner(depth), $"unexpected handler returned at depth {depth}");
        }

        [Test]
        public void raise_indexOutOfRangeException_when_reaching_handler_out_of_depth()
        {
            DelegatingHandler[] handlers = CreateHandlers(3);
            DelegatingHandler handlerChain = CreateTopBottomHandler(handlers);
            Assert.Throws<IndexOutOfRangeException>(() => handlerChain.Inner(5), $"should raise an exception when trying to reach handler out of range");
        }

        [Test]
        public void i_can_append_multiple_delegate_to_not_queued_handler()
        {
            DelegatingHandler[] handlers = CreateHandlers(3);
            DelegatingHandler handlerChain = CreateTopBottomHandler(handlers);
            DelegatingHandler[] handlersToAppend = CreateHandlers(3, 4);
            DelegatingHandler toAppend = CreateTopBottomHandler(handlersToAppend);
            handlerChain.Append(toAppend);
            Assert.AreEqual(toAppend, handlers.Last().InnerHandler, "handler has not been appended");
        }

        [Test]
        public void i_can_append_multiple_delegate_to_queued_handler()
        {
            DelegatingHandler[] handlers = CreateHandlers(3);
            DelegatingHandler handlerChain = CreateTopBottomHandler(handlers, true);
            DelegatingHandler[] handlersToAppend = CreateHandlers(3, 4);
            DelegatingHandler toAppend = CreateTopBottomHandler(handlersToAppend);
            handlerChain.Append(toAppend);
            Assert.AreEqual(toAppend, handlers.Last().InnerHandler, "handler has not been appended");
            Assert.IsInstanceOf<TestQueueHandler>(handlersToAppend.Last().InnerHandler, "queue has not been appended to appended chain");
        }

        [Test]
        public void i_can_append_single_delegate_to_not_queued_handler()
        {
            DelegatingHandler[] handlers = CreateHandlers(3);
            DelegatingHandler handlerChain = CreateTopBottomHandler(handlers);
            TestHandler toAppend = new TestHandler("appended");
            handlerChain.Append(toAppend);
            Assert.AreEqual(toAppend, handlers.Last().InnerHandler, "handler has not been appended");
        }

        [Test]
        public void i_can_append_single_messagehandler_to_not_queued_handler()
        {
            DelegatingHandler[] handlers = CreateHandlers(3);
            DelegatingHandler handlerChain = CreateTopBottomHandler(handlers);
            var toAppend = new TestQueueHandler("appended");
            handlerChain.Append(toAppend);
            Assert.AreEqual(toAppend, handlers.Last().InnerHandler, "handler has not been appended");
        }

        [Test]
        public void should_raise_exception_when_append_single_messagehandler_to_queued_handler()
        {
            DelegatingHandler[] handlers = CreateHandlers(3);
            DelegatingHandler handlerChain = CreateTopBottomHandler(handlers, true);
            var toAppend = new TestQueueHandler("appended");
            Assert.Throws<InvalidOperationException>(() => handlerChain.Append(toAppend), "should not be able to add two chains with queue");
        }

        [Test]
        public void i_can_append_single_delegate_to_queued_handler()
        {
            DelegatingHandler[] handlers = CreateHandlers(3);
            DelegatingHandler handlerChain = CreateTopBottomHandler(handlers, true);
            TestHandler toAppend = new TestHandler("appended");
            handlerChain.Append(toAppend);
            Assert.AreEqual(toAppend, handlers.Last().InnerHandler, "handler has not been appended");
        }

        [Test]
        [TestCaseSource(nameof(BottomTopHandlers))]
        public void i_can_walk_bottom_to_top(DelegatingHandler toScan, IList<HttpMessageHandler> expectedHandlers)
        {
            int i = 0;
            foreach (HttpMessageHandler handler in toScan.BottomTop())
            {
                Assert.AreEqual(expectedHandlers[i], handler, $"handler #{i+1} does not match");
                ++i;
            }
        }

        [Test]
        [TestCaseSource(nameof(TopBottomHandlers))]
        public void i_can_walk_top_to_bottom(DelegatingHandler toScan, IList<HttpMessageHandler> expectedHandlers)
        {
            int i = 0;
            foreach (HttpMessageHandler handler in toScan.TopBottom())
            {
                Assert.AreEqual(expectedHandlers[i], handler, $"handler #{i + 1} does not match");
                ++i;
            }
        }

        [Test]
        public void bottom_top_scan_from_null_returns_no_values()
        {
            Assert.IsEmpty(DelegatingHandlerExtensions.BottomTop(null), "no item should be returned");
        }

        private static IEnumerable<TestCaseData> BottomTopHandlers
        {
            get
            {
                DelegatingHandler[] handlers = CreateHandlers(1);
                yield return new TestCaseData(CreateBottomTopHandler(handlers), handlers) { TestName = "B2T> one delegating handler, no queue" };
                handlers = CreateHandlers(2);
                yield return new TestCaseData(CreateBottomTopHandler(handlers), handlers) { TestName = "B2T> two delegating handlers, no queue" };
                handlers = CreateHandlers(4);
                yield return new TestCaseData(CreateBottomTopHandler(handlers), handlers) { TestName = "B2T> four delegating handlers, no queue" };
                handlers = CreateHandlers(1);
                var expected = new List<HttpMessageHandler>();
                expected.Add(new TestQueueHandler());
                expected.AddRange(handlers);
                yield return new TestCaseData(CreateBottomTopHandler(handlers, true), expected) { TestName = "B2T> one delegating handler, queued" };
                handlers = CreateHandlers(2);
                expected = new List<HttpMessageHandler>();
                expected.Add(new TestQueueHandler());
                expected.AddRange(handlers);
                yield return new TestCaseData(CreateBottomTopHandler(handlers, true), expected) { TestName = "B2T> two delegating handlers, queued" };
                handlers = CreateHandlers(4);
                expected = new List<HttpMessageHandler>();
                expected.Add(new TestQueueHandler());
                expected.AddRange(handlers);
                yield return new TestCaseData(CreateBottomTopHandler(handlers, true), expected) { TestName = "B2T> four delegating handlers, queued" };
            }
        }

        private static IEnumerable<TestCaseData> TopBottomHandlers
        {
            get
            {
                DelegatingHandler[] handlers = CreateHandlers(1);
                yield return new TestCaseData(CreateTopBottomHandler(handlers), handlers) { TestName = "T2B> one delegating handler, no queue" };
                handlers = CreateHandlers(2);
                yield return new TestCaseData(CreateTopBottomHandler(handlers), handlers) { TestName = "T2B> two delegating handlers, no queue" };
                handlers = CreateHandlers(4);
                yield return new TestCaseData(CreateTopBottomHandler(handlers), handlers) { TestName = "T2B> four delegating handlers, no queue" };
                handlers = CreateHandlers(1);
                var expected = new List<HttpMessageHandler>();
                expected.AddRange(handlers);
                expected.Add(new TestQueueHandler());
                yield return new TestCaseData(CreateTopBottomHandler(handlers, true), expected) { TestName = "T2B> one delegating handler, queued" };
                handlers = CreateHandlers(2);
                expected = new List<HttpMessageHandler>();
                expected.AddRange(handlers);
                expected.Add(new TestQueueHandler());
                yield return new TestCaseData(CreateTopBottomHandler(handlers, true), expected) { TestName = "T2B> two delegating handlers, queued" };
                handlers = CreateHandlers(4);
                expected = new List<HttpMessageHandler>();
                expected.AddRange(handlers);
                expected.Add(new TestQueueHandler());
                yield return new TestCaseData(CreateTopBottomHandler(handlers, true), expected) { TestName = "T2B> four delegating handlers, queued" };
            }
        }

        private static DelegatingHandler[] CreateHandlers(int count, int start = 1)
        {
            return Enumerable.Range(start, count).Select(i => new TestHandler($"handler #{i:00}")).ToArray();
        }

        private static DelegatingHandler CreateBottomTopHandler(DelegatingHandler[] handlers, bool addQueue = false)
        {
            if (addQueue)
            {
                handlers[0].InnerHandler = new TestQueueHandler();
            }
            for (int i = handlers.Length-1; i > 0; --i)
            {
                handlers[i].InnerHandler = handlers[i - 1];
            }
            return handlers[handlers.Length - 1];
        }

        private static DelegatingHandler CreateTopBottomHandler(DelegatingHandler[] handlers, bool addQueue = false)
        {
            for (int i = 0; i < handlers.Length - 1; ++i)
            {
                handlers[i].InnerHandler = handlers[i + 1];
            }
            if (addQueue)
            {
                handlers[handlers.Length-1].InnerHandler = new TestQueueHandler();
            }
            return handlers[0];
        }
    }

    internal class TestHandler:DelegatingHandler
    {
        public TestHandler(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public static bool AreEqual(TestHandler lhs, TestHandler rhs)
        {
            if (ReferenceEquals(lhs, rhs)) return true;
            if (lhs == null || rhs == null) return false;
            return String.Equals(lhs.Name, rhs.Name, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return AreEqual(this, obj as TestHandler);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }


    internal class TestQueueHandler : HttpMessageHandler
    {
        public TestQueueHandler()
            :this("Queue")
        {

        }

        public TestQueueHandler(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public static bool AreEqual(TestQueueHandler lhs, TestQueueHandler rhs)
        {
            if (ReferenceEquals(lhs, rhs)) return true;
            if (lhs == null || rhs == null) return false;
            return String.Equals(lhs.Name, rhs.Name, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return AreEqual(this, obj as TestQueueHandler);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
