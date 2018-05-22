using System;
using Domain.Models;
using Domain.Persistence;
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
        public static IWebHost InitialiseDynamoDb(this IWebHost host, IHostingEnvironment HostingEnvironment)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
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
            var logger = services.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Seeding DynamoDb data");

            var tenantStore = services.GetRequiredService<ITenantStore>();

            tenantStore.Create(new TenantCreateData
                {
                    Code = "rewire.example.nz",
                    Name = "Rewire NZ",
                    Description = "A sample tenant (company/organisation)"
                })
                .ConfigureAwait(false);

            var todoStore = services.GetRequiredService<ITodoStore>();

            todoStore.Create(new TodoCreateData {Name = "One Todo"}).ConfigureAwait(false);
            todoStore.Create(new TodoCreateData {Name = "Two Todo", Completed = true}).ConfigureAwait(false);
            todoStore.Create(new TodoCreateData {Name = "Three Todo"}).ConfigureAwait(false);


            return services;
        }
    }
}