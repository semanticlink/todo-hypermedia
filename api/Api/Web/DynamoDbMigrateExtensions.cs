using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using App;
using Domain.Models;
using Domain.Persistence;
using Infrastructure.NoSQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Toolkit;

namespace Api.Web
{
    public static class DynamoDbMigrateExtensions
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public static IApplicationBuilder MigrateDynamoDb(this IApplicationBuilder app)
        {
            app.ApplicationServices
                .MigrateDynamoDb();

            return app;
        }

        private static void MigrateDynamoDb(this IServiceProvider services)
        {
            try
            {
                Log.Info("[Seed] making database");
                Task.Run(() => { services.GetService<IAmazonDynamoDB>().CreateAllTables().GetAwaiter().GetResult(); });
            }
            catch (Exception ex)
            {
                Log.ErrorExceptionFormat(ex, "An error occurred while seeding the database.");
            }
            finally
            {
                Log.Debug("[Seed] database create complete");
            }

            try
            {
                Log.Info("[Seed] service user");

                // we have added 'Scoped' services, this will return the root scope with them attached
                using (var scopedProvider = services.CreateScope())
                {
                    Task.Run(scopedProvider.ServiceProvider.SeedServiceUser).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                Log.ErrorExceptionFormat(ex, "An error occurred while seeding the service user.");
            }
            finally
            {
                Log.Debug("[Seed] service user complete");
            }
        }


        /// <summary>
        /// <para>Ensures there is a service user with root of trust.</para>
        ///
        /// <para>
        ///    This user is identified via <see cref="TrustDefaults.KnownRootIdentifier"/>
        /// </para>
        ///
        /// <para>
        ///    The user user has <see cref="Permission.ControlAccess"/> | <see cref="Permission.CreatorOwner"/> on:
        ///
        /// <list type="bullet">
        ///    <item><see cref="RightType.Root"/> which is resource <see cref="TrustDefaults.KnownHomeResourceId"/></item>
        ///    <item><see cref="RightType.RootTenantCollection"/></item>
        ///    <item><see cref="RightType.RootUserCollection"/></item>
        /// </list>
        /// </para>
        /// </summary>
        private static async Task SeedServiceUser(this IServiceProvider services)
        {
            Log.Info("[Seed] Service user");

            // guard to check everything is up and running
            await services.GetRequiredService<IAmazonDynamoDB>().WaitForAllTables();

            //////////////////////////
            // Seed the root
            // =============
            //
            // Register a service account using our own scheme "service|id"
            //

            // TODO: get from configuration
            var rootUserCreateData = new UserCreateData
            {
                Name = "Service Account",
                Email = "root@rewire.nz"
            };

            var userStore = services.GetRequiredService<IUserStore>();

            var rootUser = await userStore.GetByExternalId(TrustDefaults.KnownRootIdentifier);
            var rootId = TrustDefaults.KnownRootIdentifier;

            if (rootUser != null && rootUser.Id.IsNullOrWhitespace())
            {
                rootId = await userStore.Create(
                    TrustDefaults.ProvisioningId,
                    TrustDefaults.KnownHomeResourceId,
                    rootUserCreateData,
                    Permission.ControlAccess | Permission.Get,
                    new Dictionary<RightType, Permission>
                    {
                        {RightType.Root, Permission.ControlAccess | Permission.Get},
                        {RightType.RootTenantCollection, Permission.ControlAccess | Permission.Get},
                        {RightType.RootUserCollection, Permission.FullControl},
                    });
            }

            Log.Info("[Seed] service user {0}", rootId);
        }
    }
}