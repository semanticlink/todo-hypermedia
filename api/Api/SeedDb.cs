using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation.Enum;
using Infrastructure.NoSQL;
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
        public static IWebHost InitialiseDynamoDb(this IWebHost host, IHostingEnvironment hostingEnvironment)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    if (hostingEnvironment.IsDevelopment())
                    {
                        services.SeedTestData().ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            return host;
        }

        /// <summary>
        ///     Creates a tenant, user on the tenant and some todos with tags
        /// </summary>
        public static async Task<IServiceProvider> SeedTestData(this IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Seeding DynamoDb data");
            
            var client = services.GetRequiredService<IAmazonDynamoDB>();

            TableNameConstants
                .AllTables
                .ForEach(table => table.WaitForActiveTable(client).ConfigureAwait(false));

            //////////////////////////
            // Seed a tenant
            // =============
            //
            var tenantStore = services.GetRequiredService<ITenantStore>();
            var tenantId = await tenantStore.Create(new TenantCreateData
            {
                Code = "rewire.example.nz",
                Name = "Rewire NZ",
                Description = "A sample tenant (company/organisation)"
            });
            logger.LogInformation($"[Seed] tenant '{tenantId}'");

            //////////////////////////
            // Seed a user
            // =============
            //
            // Assume a known Auth0 (test) user, register a user and then link to tenant
            //

            // KLUDGE: taken from a precreated user and then decoded JWT through https://jwt.io
            // grab it from the Authorization header in a request
            var knownAuth0Id = "auth0|5b32b696a8c12d3b9a32b138";

            var userId = await services
                .GetRequiredService<IUserStore>()
                .Create(knownAuth0Id, "test@rewire.nz", "test");
            logger.LogInformation($"[Seed] user '{userId}'");
          
            await tenantStore.AddUser(tenantId, userId);
            logger.LogInformation($"[Seed] registered user against tenant '{tenantId}'");

            //////////////////////////
            // Seed global tags
            // =============
            //
            // create some global tags
            //
            var tagStore = services.GetRequiredService<ITagStore>();

            var tagIds = (await Task
                    .WhenAll(new[] {"Work", "Personal", "Grocery List"}
                        .Select(tag => tagStore.Create(new TagCreateData {Name = tag}))))
                .Where(result => result != null)
                .ToList();
            logger.LogInformation($"[Seed] tags");

            //////////////////////////
            // Seed some todos
            // =============
            //
            var todoStore = services.GetRequiredService<ITodoStore>();

            await todoStore.Create(new TodoCreateData {Name = "One Todo"});
            await todoStore.Create(new TodoCreateData
            {
                Name = "Two Todo (tag)",
                Tags = new List<string> {tagIds.First()},
                State = TodoState.Complete
            });
            await todoStore.Create(new TodoCreateData
            {
                Name = "Three Todo (tagged)",
                Tags = tagIds
            });
            logger.LogInformation($"[Seed] todos");


            return services;
        }
    }
}