using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Api.Authorisation;
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
             * Get registered services.
             */
            var logger = services.GetRequiredService<ILogger<Program>>();
            var tenantStore = services.GetRequiredService<ITenantStore>();
            var userStore = services.GetRequiredService<IUserStore>();
            var tagStore = services.GetRequiredService<ITagStore>();
            var todoStore = services.GetRequiredService<ITodoStore>();


            // ensure the database is up and tables are created
            var client = services.GetRequiredService<IAmazonDynamoDB>();
            await client.WaitForAllTables();


            logger.LogInformation("[Seed] sample data");

            //////////////////////////
            // Authentication
            // ==============
            //

            // KLUDGE: taken from a precreated user and then decoded JWT through https://jwt.io
            // grab it from the Authorization header in a request
            var knownAuth0Id = "auth0|5b32b696a8c12d3b9a32b138";


            //////////////////////////
            // Seed a user
            // =============
            //
            // Assume a known Auth0 (test) user, register a user and then link to tenant
            //

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
                    CallerCollectionRights.User
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


            //////////////////////////
            // Seed a tenant
            // =============
            //
            string tenantId = "";
            try
            {
                var tenantCreateData = new TenantCreateData
                {
                    Code = "rewire.example.nz",
                    Name = "Rewire NZ",
                    Description = "A sample tenant (company/organisation)"
                };

                tenantId = await tenantStore.Create(
                    TrustDefaults.KnownRootIdentifier,
                    TrustDefaults.KnownHomeResourceId,
                    knownAuth0Id,
                    tenantCreateData,
                    Permission.FullControl,
                    CallerCollectionRights.Tenant);

                logger.LogInformation($"[Seed] created tenant '{tenantId}'");
            }
            catch (Exception e)
            {
                logger.LogError(e, "");
            }

            //////////////////////////
            // Add uesr to tenant
            // ==================
            //
            try
            {
                await tenantStore.IncludeUser(
                    tenantId,
                    userId,
                    Permission.Get);

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
                tagIds = (await Task.WhenAll(
                        new[] {"Work", "Personal", "Grocery List"}
                            .Select(tag => tagStore.Create(
                                userId,
                                TrustDefaults.KnownHomeResourceId,
                                new TagCreateData {Name = tag},
                                Permission.Get,
                                CallerCollectionRights.Tag)
                            )))
                    .Where(result => result != null)
                    .ToList();
                logger.LogInformation($"[Seed] tags: [{0}]", tagIds.ToCsvString(tagId => tagId));
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
                var createTodoDatas = new List<TodoCreateData>
                {
                    new TodoCreateData {Name = "One Todo"},
                    new TodoCreateData
                    {
                        Name = "Two Todo (tag)",
                        Tags = new List<string> {tagIds.First()},
                        State = TodoState.Complete
                    },
                    new TodoCreateData
                    {
                        Name = "Three Todo (tagged)",
                        Tags = tagIds
                    }
                };

                var ids = await Task.WhenAll(createTodoDatas
                    .Select(data => todoStore.Create(
                        userId,
                        data,
                        Permission.FullControl,
                        CallerCollectionRights.Todo)));


                logger.LogInformation($"[Seed] todos: [{0}]", ids.ToCsvString(id => id));
            }
            catch (Exception e)
            {
                logger.LogError(e, "");
            }
        }
    }
}