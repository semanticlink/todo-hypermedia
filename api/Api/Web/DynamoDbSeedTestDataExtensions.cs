using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Extensions.Configuration;
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
            IHostingEnvironment hostingEnvironment,
            ILogger log)
        {
            app.ApplicationServices.DynamoDbSeedTestData(hostingEnvironment, log);
            return app;
        }

        public static IWebHost DynamoDbSeedTestData(
            this IWebHost host,
            IHostingEnvironment hostingEnvironment,
            ILogger log)
        {
            host.Services.DynamoDbSeedTestData(hostingEnvironment, log);
            return host;
        }

        private static void DynamoDbSeedTestData(this IServiceProvider app,
            IHostingEnvironment hostingEnvironment, ILogger log)
        {
            // we have added 'Scoped' services, this will return the root scope with them attached
            using (var scope = app.CreateScope())
            {
                if (hostingEnvironment.IsDevelopment() || true)
                {
                    log.Debug("Seed test data");
                    Task.Run(() => scope.ServiceProvider.SeedData(log)).GetAwaiter().GetResult();
                }

                log.Debug("[Seed] test data complete");
            }
        }

        /// <summary>
        ///     Creates a tenant, user on the tenant and some todos with tags
        /// </summary>
        public static async Task SeedData(this IServiceProvider services, ILogger log)
        {
            /**
             * Get registered services.
             */
            var context = services.GetRequiredService<IDynamoDBContext>();
            var idGenerator = services.GetRequiredService<IIdGenerator>();
            var userRightsStore = services.GetRequiredService<IUserRightStore>();
            var configuration = services.GetRequiredService<IConfiguration>();

            /**
             * Setup the provisioning user
             */
            var provisioningUser = new User {Id = TrustDefaults.ProvisioningId};

            /**
             * Hand make up these because we want to inject the user with the provisioning user
             */
            var tenantStore = new TenantStore(
                provisioningUser,
                context,
                idGenerator,
                userRightsStore,
                services.GetRequiredService<ILogger<TenantStore>>());

            var userStore = new UserStore(
                provisioningUser,
                context,
                idGenerator,
                userRightsStore,
                services.GetRequiredService<ILogger<UserStore>>());

            var tagStore = new TagStore(
                provisioningUser,
                context,
                idGenerator,
                userRightsStore,
                services.GetRequiredService<ILogger<TagStore>>());

            var todoStore = new TodoStore(
                provisioningUser,
                context,
                idGenerator,
                userRightsStore,
                tagStore,
                tenantStore,
                services.GetRequiredService<ILogger<TodoStore>>());

            // ensure the database is up and tables are created
            await services.GetRequiredService<IAmazonDynamoDB>()
                .WaitForAllTables(log);


            log.Info("[Seed] create sample data");

            //////////////////////////
            // Authentication
            // ==============
            //

            // A precreated user (in third-party system) [or decoded JWT through https://jwt.io
            // grab it from the Authorization header in a request]
            var knownAuth0Id = configuration.GetSection("TestSeedUser").Value;

            log.DebugFormat("[Seed] found seed user '{0}'", knownAuth0Id);

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
                Email = "test-1@semanticlink.io",
                Name = "test",
                ExternalId = knownAuth0Id
            };

            // create seeed data if the user doesn't exist
            if ((await userStore.GetByExternalId(userData.ExternalId)).IsNull())
            {
                log.Info($"[Seed] user {userData.Email}");

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
                    Code = "rewire.semanticlink.io",
                    Name = "Rewire",
                    Description = "A sample tenant (company/organisation)"
                };

                log.Info($"[Seed] tenant '{tenantCreateData.Code}'");

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

                log.Info($"[Seed] registered user '{userData.Email}' against tenant '{tenantCreateData.Code}'");

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

                log.InfoFormat("[Seed] tags: [{0}]", tagIds.ToCsvString(tagId => tagId));

                /////////////////////////////////////
                // Seed a named todo list
                // ======================
                //

                var todoCreateData = new TodoCreateData
                {
                    Parent = tenantId,
                    Name = "Shopping Todo List",
                    Type = TodoType.List
                };

                var todoListId = await todoStore.Create(
                    userId,
                    tenantId,
                    todoCreateData,
                    Permission.FullControl | Permission.Owner,
                    CallerCollectionRights.Todo);

                log.InfoFormat("[Seed] todo list [{0}]", todoListId);

                //////////////////////////
                // Seed some todos
                // ===============
                //

                var createTodoDatas = new List<TodoCreateData>
                {
                    new TodoCreateData
                    {
                        Name = "One Todo",
                        Parent = todoListId,
                        Type = TodoType.Item
                    },
                    new TodoCreateData
                    {
                        Name = "Two Todo (tag)",
                        Tags = new List<string> {tagIds.First()},
                        State = TodoState.Complete,
                        Parent = todoListId,
                        Type = TodoType.Item 
                    },
                    new TodoCreateData
                    {
                        Name = "Three Todo (tagged)",
                        Tags = tagIds,
                        Parent = todoListId,
                        Type = TodoType.Item
                    }
                };

                var ids = await Task.WhenAll(createTodoDatas
                    .Select(data => todoStore.Create(
                        userId,
                        userId,
                        data,
                        Permission.FullControl | Permission.Owner,
                        CallerCollectionRights.Todo)));


                log.InfoFormat("[Seed] todos: [{0}]", ids.ToCsvString(id => id));
            }
            else
            {
                log.Debug("[Seed] test data already setup");
            }
        }
    }
}