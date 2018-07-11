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
    ///     All code around seeding <see cref="InitialiseDynamoDb"/> needs to be in called from <see cref="Program.Main"/> to avoid
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
            /**
             * Get registered services before going off. Avoids loosing the IServiceProvider
             */
            var logger = services.GetRequiredService<ILogger<Program>>();
            var tenantStore = services.GetRequiredService<ITenantStore>();
            var userStore = services.GetRequiredService<IUserStore>();
            var tagStore = services.GetRequiredService<ITagStore>();
            var todoStore = services.GetRequiredService<ITodoStore>();


            var client = services.GetRequiredService<IAmazonDynamoDB>();

            logger.LogInformation("[Seed] DynamoDb tables");

            await Task.WhenAll(
                TableNameConstants
                    .AllTables
                    .Select(table => table.WaitForActiveTable(client)));

            logger.LogInformation("[Seed] data");

            //////////////////////////
            // Seed a tenant
            // =============
            //
            string tenantId = "";
            try
            {
                tenantId = await tenantStore.Create(new TenantCreateData
                {
                    Code = "rewire.example.nz",
                    Name = "Rewire NZ",
                    Description = "A sample tenant (company/organisation)"
                });
                logger.LogInformation($"[Seed] tenant '{tenantId}'");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            //////////////////////////
            // Seed a user
            // =============
            //
            // Assume a known Auth0 (test) user, register a user and then link to tenant
            //

            // KLUDGE: taken from a precreated user and then decoded JWT through https://jwt.io
            // grab it from the Authorization header in a request
            var knownAuth0Id = "auth0|5b32b696a8c12d3b9a32b138";
            string userId = "";
            try
            {
                userId = await userStore.Create(knownAuth0Id, "test@rewire.nz", "test");
            }
            catch (Exception e)
            {
                if (e.Message.Equals("User already created"))
                {
                    userId = (await userStore.GetByExternalId(knownAuth0Id)).Id;
                }
                else
                {
                    Console.WriteLine(e);
                }
            }
            finally
            {
                logger.LogInformation($"[Seed] user '{userId}'");
            }


            try
            {
                await tenantStore.AddUser(tenantId, userId);
                logger.LogInformation($"[Seed] registered user against tenant '{tenantId}'");
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
            }

            //////////////////////////
            // Seed global tags
            // =============
            //
            // create some global tags
            //
            List<string> tagIds = new List<string>();
            try
            {
                tagIds = (await Task
                        .WhenAll(new[] {"Work", "Personal", "Grocery List"}
                            .Select(tag => tagStore.Create(new TagCreateData {Name = tag}))))
                    .Where(result => result != null)
                    .ToList();
                logger.LogInformation($"[Seed] tags");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            //////////////////////////
            // Seed some todos
            // =============
            //

            try
            {
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return services;
        }
    }
}