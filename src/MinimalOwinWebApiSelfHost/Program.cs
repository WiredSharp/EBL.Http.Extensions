using System;

// Add reference to:
using Microsoft.Owin.Hosting;

namespace MinimalOwinWebApiSelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            // Specify the URI to use for the local host:
            string baseUri = "http://localhost:4000";

            Console.WriteLine("Starting web Server...");
            WebApp.Start<Startup>(baseUri);
            Console.WriteLine("Server running at {0} - press any key to quit. ", baseUri);
            Console.ReadKey();
        }
    }
}
