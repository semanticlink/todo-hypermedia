using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;

namespace Api
{
    public class Program
    {
        private static IHostingEnvironment HostingEnvironment { get; set; }

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
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // need to retain a handle to this for use in seeding test data below
                    HostingEnvironment = hostingContext.HostingEnvironment;
                })
                .UseStartup<Startup>()
                .Build()
                // KLUDGE: seed dev data (given that lambda is used in production over kestrel
                .InitialiseDynamoDb(HostingEnvironment);
    }
}