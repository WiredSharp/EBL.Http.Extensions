using System;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace MinimalOwinWebApiSelfHost
{
    internal class TraceMiddleWare: OwinMiddleware
    {
        public TraceMiddleWare(OwinMiddleware next) 
            : base(next)
        {
        }

        public override Task Invoke(IOwinContext context)
        {
            Console.WriteLine($"authentication? {context.Authentication}");
            Console.WriteLine(Display(context.Request));
            return Next.Invoke(context);
        }

        private string Display(IOwinRequest request)
        {
            return $"{request.Path} by '{request.User}'";
        }
    }
}