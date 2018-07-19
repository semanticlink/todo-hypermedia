﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Api.Authorisation;
using App;
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
                var services = scope.ServiceProvider;
                try
                {
                    if (hostingEnvironment.IsDevelopment())
                    {
                        Log.Info("[Seed] test data");
                        Task.Run(() => services.SeedData()).GetAwaiter().GetResult();
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorExceptionFormat(ex, "An error occurred while seeding the test data.");
                }
                finally
                {
                    Log.Debug("[Seed] test data complete");
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
            var tenantStore = services.GetRequiredService<ITenantStore>();
            var userStore = services.GetRequiredService<IUserStore>();
            var tagStore = services.GetRequiredService<ITagStore>();
            var todoStore = services.GetRequiredService<ITodoStore>();


            // ensure the database is up and tables are created
            var client = services.GetRequiredService<IAmazonDynamoDB>();
            await client.WaitForAllTables();


            Log.Info("[Seed] sample data");

            //////////////////////////
            // Authentication
            // ==============
            //

            // A precreated user (in third-party system) [or decoded JWT through https://jwt.io
            // grab it from the Authorization header in a request]
            var knownAuth0Id = "auth0|5b32b696a8c12d3b9a32b138";


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
                ExternalId = "auth0|5b32b696a8c12d3b9a32b138"
            };

            string userId = "";
            try
            {
                // TODO: this should be through the API front door
                userId = await userStore.Create(
                    TrustDefaults.KnownRootIdentifier,
                    TrustDefaults.KnownHomeResourceId,
                    userData,
                    Permission.FullControl,
                    CallerCollectionRights.User
                );
            }
            catch (InvalidOperationException e)
            {
                userId = (await userStore.GetByExternalId(knownAuth0Id)).Id;
            }
            finally
            {
                Log.Info($"[Seed] user '{userId}'");
            }


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
            string tenantId = "";
            try
            {
                tenantId = await tenantStore.Create(
                    TrustDefaults.KnownRootIdentifier,
                    TrustDefaults.KnownHomeResourceId,
                    tenantCreateData,
                    Permission.FullControl,
                    CallerCollectionRights.Tenant);

                Log.Info($"[Seed] created tenant '{tenantId}'");
            }
            catch (InvalidOperationException e)
            {
                Log.Debug("Tenant alredy exists");

                tenantId = (await tenantStore.GetByCode(tenantCreateData.Code)).Id;
            }

            //////////////////////////
            // Add user to tenant
            // ==================
            //
            try
            {
                if (!await tenantStore.IsRegisteredOnTenant(tenantId, userId))
                {
                    await tenantStore.IncludeUser(tenantId, userId, Permission.Get, CallerCollectionRights.Tenant);
                }

                Log.Info($"[Seed] registered user against tenant '{tenantId}'");
            }
            catch (Exception e)
            {
                Log.ErrorExceptionFormat(e, "");
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
                Log.InfoFormat("[Seed] tags: [{0}]", tagIds.ToCsvString(tagId => tagId));
            }
            catch (Exception e)
            {
                Log.ErrorExceptionFormat(e, "Error creating global tags");
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


                Log.InfoFormat("[Seed] todos: [{0}]", ids.ToCsvString(id => id));
            }
            catch (Exception e)
            {
                Log.ErrorExceptionFormat(e, "Error creating tooos");
            }
        }
    }
}