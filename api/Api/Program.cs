using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Api
{
    public class Program
    {
        public static IHostingEnvironment HostingEnvironment { get; set; }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // Assigning the environment for use in ConfigureServices
                    HostingEnvironment = hostingContext.HostingEnvironment;
                })
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build()
                .Initialise(HostingEnvironment);

            host.Run();
        }
    }
}