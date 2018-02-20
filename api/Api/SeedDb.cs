using System;
using Domain.Models;
using Infrastructure.Db;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Api
{
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
//                    var context = services.GetRequiredService<TodoContext>();
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

            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Seeding dev data.");

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