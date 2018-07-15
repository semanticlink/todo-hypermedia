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
using Toolkit;

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
                var logger = services.GetRequiredService<ILogger<Program>>();
                try
                {
                    logger.LogInformation("[Seed] service user");
                    Task.Run(() => services.SeedServiceUser()).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while seeding the service user.");
                }
                finally
                {
                    logger.LogDebug("[Seed] service user complete");
                }

                try
                {
                    if (hostingEnvironment.IsDevelopment())
                    {
                        logger.LogInformation("[Seed] test data");
                        Task.Run(() => services.SeedTestData()).GetAwaiter().GetResult();
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

            return host;
        }

        /// <summary>
        ///     Ensures there is a service user with root of trust
        /// </summary>
        public static async Task SeedServiceUser(this IServiceProvider services)
        {
            /**
             * Get registered services before going off. Avoids loosing the IServiceProvider
             */
            var logger = services.GetRequiredService<ILogger<Program>>();
            var userStore = services.GetRequiredService<IUserStore>();
            var rightStore = services.GetRequiredService<IUserRightStore>();

            var client = services.GetRequiredService<IAmazonDynamoDB>();

            logger.LogInformation("[Seed] Service user");

            await Task.WhenAll(
                TableNameConstants
                    .AllTables
                    .Select(table => table.WaitForActiveTable(client)));

            //////////////////////////
            // Seed the root
            // =============
            //
            // Register a service account using our own scheme "service|id"
            //

            var knownRootId = "service|root01";
            var knownHomeResourceId = "0000000000000000000000000";
            string rootId = "";

            try
            {
                var rootUser = await userStore.GetByExternalId(knownRootId);
                rootId = rootUser.IsNull()
                    ? await userStore.Create(knownRootId, "root@rewire.nz", "Service Account")
                    : rootUser.Id;
                logger.LogInformation("[Seed] service user {0}", rootId);
            }
            catch (Exception e)
            {
                logger.LogError(e, "");
            }

            await rightStore.CreateRights(
                rootId,
                knownHomeResourceId,
                new Dictionary<RightType, Permission>
                {
                    {RightType.Root, Permission.ControlAccess | Permission.CreatorOwner},
                    {RightType.RootTenantCollection, Permission.ControlAccess | Permission.CreatorOwner},
                    {RightType.RootUserCollection, Permission.ControlAccess | Permission.CreatorOwner},
                });
        }

        /// <summary>
        ///     Creates a tenant, user on the tenant and some todos with tags
        /// </summary>
        public static async Task SeedTestData(this IServiceProvider services)
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
                    logger.LogError(e, "");
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