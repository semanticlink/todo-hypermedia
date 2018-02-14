using System;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoApi.Db;
using TodoApi.Models;
using TodoApi.Utils;

namespace TodoApi
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

    /// <summary>
    ///     All code around <see cref="DbInitializer" needs to be in <see cref="Program.Main"/> to avoid
    ///     problems with migrations
    /// </summary>
    public static class SeedDb
    {

        public static IWebHost Initialise(this IWebHost host, IHostingEnvironment HostingEnvironment)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<TodoContext>();
//                    DbInitializer.Initialize(context);

                    if (HostingEnvironment.IsDevelopment())
                    {
                        services.SeedTestData();
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }

            return host;
        }

        public static IServiceProvider SeedTestData(this IServiceProvider services)
        {
            var context = services.GetRequiredService<TodoContext>();

            var tenants = new[]
            {
                new Tenant
                {
                    Code = "rewire.example.nz",
                    Name = "Rewire NZ",
                    Description = "A sample tenant (company/organisation)"
                }
            };
            context.Tenants.AddRange(tenants);

            var todos = new[]
            {
                new Todo {Name = "One"},
                new Todo {Name = "Two"},
                new Todo {Name = "Three"},
            };
            context.TodoItems.AddRange(todos);

            context.SaveChanges();

            return services;
        }
    }
}