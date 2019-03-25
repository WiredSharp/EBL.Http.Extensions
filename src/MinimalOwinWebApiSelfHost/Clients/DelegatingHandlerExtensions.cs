using System;
using System.Collections.Generic;
using System.Net.Http;

namespace MinimalOwinWebApiSelfHost.Clients
{
    public static class DelegatingHandlerExtensions
    {
        public static IEnumerable<HttpMessageHandler> BottomTop(this DelegatingHandler handler)
        {
            if (handler == null) yield break;
            if (handler.InnerHandler is DelegatingHandler innerHandler)
            {
                foreach (HttpMessageHandler inner in BottomTop(innerHandler))
                {
                    yield return inner;
                }
            }
            else if (handler.InnerHandler != null)
            {
                yield return handler.InnerHandler;
            }
            yield return handler;
        }

        public static HttpMessageHandler Inner(this DelegatingHandler handler, int depth)
        {
            int index = 0;
            foreach (HttpMessageHandler innerHandler in TopBottom(handler))
            {
                if (index == depth)
                {
                    return innerHandler;
                }
                ++index;
            }
            throw new IndexOutOfRangeException($"no handler at depth {depth}");
        }

        public static IEnumerable<HttpMessageHandler> TopBottom(this DelegatingHandler handler)
        {
            if (handler == null) yield break;
            yield return handler;
            if (handler.InnerHandler is DelegatingHandler innerDelegating)
            {
                foreach (HttpMessageHandler inner in TopBottom(innerDelegating))
                {
                    yield return inner;
                }
            }
            else if (handler.InnerHandler != null)
            {
                yield return handler.InnerHandler;
            }
        }

        public static DelegatingHandler Prepend(this HttpMessageHandler handler, DelegatingHandler toPrepend)
        {
            return Append(toPrepend, handler);
        }

        public static DelegatingHandler Append(this DelegatingHandler handler, HttpMessageHandler toAppend)
        {
            if (toAppend == null)
            {
                return handler;
            }
            if (handler == null)
            {
                throw new ArgumentNullException("cannot append to a null reference", nameof(handler));
            }
            if (handler.InnerHandler == null)
            {
                handler.InnerHandler = toAppend;
            }
            else
            {
                if (handler.InnerHandler is DelegatingHandler delegateHandler)
                {
                    Append(delegateHandler, toAppend);
                }
                else
                {
                    if (toAppend is DelegatingHandler delegateToAppend)
                    {
                        Append(delegateToAppend, handler.InnerHandler);
                        handler.InnerHandler = toAppend;
                    }
                    else
                    {
                        throw new InvalidOperationException("cannot append, both side have a final handler");
                    }
                }
            }
            return handler;
        }
    }
}
