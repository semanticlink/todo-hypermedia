using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Api.Authorisation;
using App;
using Domain;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation.Enum;
using Infrastructure.NoSQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Toolkit;

namespace Api.Web
{
    /// <summary>
    ///     All code around seeding <see cref="DynamoDbSeedTestData"/> needs to be in called from <see cref="Program.Main"/> to avoid
    ///     problems with migrations
    /// </summary>
    public static class DynamoDbSeedTestDataExtensions
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

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
                if (hostingEnvironment.IsDevelopment())
                {
                    Task.Run(() => scope.ServiceProvider.SeedData()).GetAwaiter().GetResult();
                }

                Log.Debug("[Seed] test data complete");
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
            var context = services.GetRequiredService<IDynamoDBContext>();
            var idGenerator = services.GetRequiredService<IIdGenerator>();
            var userRightsStore = services.GetRequiredService<IUserRightStore>();

            /**
             * Setup the provisioning user
             */
            var provisioningUser = new User {Id = TrustDefaults.ProvisioningId};

            /**
             * Hand make up these because we want to inject the user with the provisioning user
             */
            var tenantStore = new TenantStore(provisioningUser, context, idGenerator, userRightsStore);
            var userStore = new UserStore(provisioningUser, context, idGenerator, userRightsStore);
            var tagStore = new TagStore(provisioningUser, context, idGenerator, userRightsStore);
            var todoStore = new TodoStore(provisioningUser, context, idGenerator, userRightsStore, tagStore);


            // ensure the database is up and tables are created
            await services.GetRequiredService<IAmazonDynamoDB>().WaitForAllTables();


            Log.Info("[Seed] create sample data");

            //////////////////////////
            // Authentication
            // ==============
            //

            // A precreated user (in third-party system) [or decoded JWT through https://jwt.io
            // grab it from the Authorization header in a request]
            var knownAuth0Id = "auth0|5b32b696a8c12d3b9a32b138";

            var rootUser = (await userStore.GetByExternalId(TrustDefaults.KnownRootIdentifier))
                .ThrowConfigurationErrorsExceptionIfNull(() => "Root user has not been configured");


            //////////////////////////
            // Seed a user
            // =============
            //
            // Assume a known Auth0 (test) user, register a user and then link to tenant
            //

            var userData = new UserCreateData
            {
                Email = "test@rewire.nz",
                Name = "test",
                ExternalId = knownAuth0Id
            };

            // create seeed data if the user doesn't exist
            if ((await userStore.GetByExternalId(userData.ExternalId)).IsNull())
            {
                Log.Info($"[Seed] user {userData.Email}");

                var userId = await userStore.Create(
                    rootUser.Id,
                    TrustDefaults.KnownHomeResourceId,
                    userData,
                    Permission.FullControl | Permission.Owner,
                    CallerCollectionRights.User);


                //////////////////////////
                // Seed a tenant
                // =============
                //

                var tenantCreateData = new TenantCreateData
                {
                    Code = "rewire.example.nz",
                    Name = "Rewire NZ",
                    Description = "A sample tenant (company/organisation)"
                };

                Log.Info($"[Seed] tenant '{tenantCreateData.Code}'");

                var tenantId = await tenantStore.Create(
                    rootUser.Id,
                    TrustDefaults.KnownHomeResourceId,
                    tenantCreateData,
                    Permission.FullControl | Permission.Owner,
                    CallerCollectionRights.Tenant);


                //////////////////////////
                // Add user to tenant
                // ==================
                //
                if (!await tenantStore.IsRegisteredOnTenant(tenantId, userId))
                {
                    await tenantStore.IncludeUser(
                        tenantId,
                        userId,
                        Permission.Get | Permission.Owner,
                        CallerCollectionRights.Tenant);
                }

                Log.Info($"[Seed] registered user '{userData.Email}' against tenant '{tenantCreateData.Code}'");

                //////////////////////////
                // Seed global tags
                // =============
                //
                // create some global tags
                //
                var tagIds = (await Task.WhenAll(
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

                Log.InfoFormat("[Seed] tags: [{0}]", tagIds.ToCsvString(tagId => tagId));

                //////////////////////////
                // Seed some todos
                // =============
                //

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
                        Permission.FullControl | Permission.Owner,
                        CallerCollectionRights.Todo)));


                Log.InfoFormat("[Seed] todos: [{0}]", ids.ToCsvString(id => id));
            }
            else
            {
                Log.Debug("[Seed] test data already setup");
            }
        }
    }
}