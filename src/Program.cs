using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MTCG
{
    internal class Program
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // entry point                                                                                                      //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Main entry point.</summary>
        /// <param name="args">Arguments.</param>
        static async Task Main(string[] args)
        {

            HttpServer svr = new();
            await svr.Run();

            // ConstructorInfo info = typeof(User).GetConstructors()[0];
            // User user = info.MapArgumentsAndCreateInstance<User>();
            // Console.WriteLine("test");
        }
    }
}