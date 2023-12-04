using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MTCG
{
    internal class Program
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // entry point                                                                                                      //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Main entry point.</summary>
        /// <param name="args">Arguments.</param>
        static void Main(string[] args)
        {

            HttpServer svr = new();
            svr.Run();

            // ConstructorInfo info = typeof(User).GetConstructors()[0];
            // User user = info.MapArgumentsAndCreateInstance<User>();
            // Console.WriteLine("test");
        }
    }
}