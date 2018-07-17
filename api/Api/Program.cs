using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        private static IWebHost BuildWebHost(string[] args) =>
            WebHost
                /*
                 * Default builder will deal with configuration in root directory and also spinning up Kestrel
                 */
                .CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}