using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using App;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation;
using Domain.Representation.Enum;
using Infrastructure.NoSQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolkit;

namespace Api.Web
{
    /// <summary>
    ///     All code around seeding <see cref="DynamoDbSeedTestData"/> needs to be in called from <see cref="Program.Main"/> to avoid
    ///     problems with migrations
    /// </summary>
    public static class DynamoDbSeedTestDataExtensions
    {
        public static IApplicationBuilder DynamoDbSeedTestData(
            this IApplicationBuilder app,
            IHostingEnvironment hostingEnvironment)
        {
            app.ApplicationServices.DynamoDbSeedTestData(hostingEnvironment);
            return app;
        }

        public static IWebHost DynamoDbSeedTestData(this IWebHost host, IHostingEnvironment hostingEnvironment)
        {
            host.Services.DynamoDbSeedTestData(hostingEnvironment);
            return host;
        }

        private static void DynamoDbSeedTestData(
            this IServiceProvider app,
            IHostingEnvironment hostingEnvironment)
        {
            // we have added 'Scoped' services, this will return the root scope with them attached
            using (var scope = app.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                try
                {
                    if (hostingEnvironment.IsDevelopment())
                    {
                        logger.LogInformation("[Seed] test data");
                        Task.Run(() => services.SeedData()).GetAwaiter().GetResult();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while seeding the test data.");
                }
                finally
                {
                    logger.LogDebug("[Seed] test data complete");
                }
            }
        }

        /// <summary>
        ///     Creates a tenant, user on the tenant and some todos with tags
        /// </summary>
        private static async Task SeedData(this IServiceProvider services)
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
            await client.WaitForAllTables();


            logger.LogInformation("[Seed] sample data");

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
                logger.LogError(e, "");
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

            var userData = new UserCreateDataRepresentation
            {
                Email = "test@rewire.nz",
                Name = "test"
            };

            string userId = "";
            try
            {
                // TODO: this should be through the API front door
                userId = await userStore.Create(
                    TrustDefaults.KnownRootIdentifier,
                    TrustDefaults.KnownHomeResourceId,
                    knownAuth0Id,
                    userData,
                    Permission.FullControl,
                    new Dictionary<RightType, Permission>()
                );
            }
            catch (Exception e)
            {
                if (e.Message.Equals("User already created"))
                {
                    userId = (await userStore.GetByExternalId(knownAuth0Id)).Id;
                }
                else
                {
                    logger.LogError(e, "");
                    throw;
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
                logger.LogError(e, "");
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
                logger.LogError(e, "");
            }
        }
    }
}