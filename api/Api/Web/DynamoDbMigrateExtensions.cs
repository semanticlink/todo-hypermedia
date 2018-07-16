using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using App;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation;
using Infrastructure.NoSQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolkit;

namespace Api.Web
{
    public static class DynamoDbMigrateExtensions
    {
        public static IApplicationBuilder MigrateDynamoDb(this IApplicationBuilder app)
        {
            app.ApplicationServices
                .MigrateDynamoDb();

            return app;
        }

        private static IServiceProvider MigrateDynamoDb(this IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("[Seed] making database");
                Task.Run(() => { services.GetService<IAmazonDynamoDB>().CreateAllTables().GetAwaiter().GetResult(); });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
            finally
            {
                logger.LogDebug("[Seed] database create complete");
            }

            try
            {
                logger.LogInformation("[Seed] service user");

                // we have added 'Scoped' services, this will return the root scope with them attached
                using (var scopedProvider = services.CreateScope())
                {
                    Task.Run(scopedProvider.ServiceProvider.SeedServiceUser).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the service user.");
            }
            finally
            {
                logger.LogDebug("[Seed] service user complete");
            }

            return services;
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
            var logger = services.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("[Seed] Service user");

            // guard to check everything is up and running
            await services.GetRequiredService<IAmazonDynamoDB>().WaitForAllTables();

            //////////////////////////
            // Seed the root
            // =============
            //
            // Register a service account using our own scheme "service|id"
            //

            // TODO: get from configuration
            var rootUserCreateData = new UserCreateDataRepresentation
            {
                Name = "Service Account",
                Email = "root@rewire.nz"
            };

            var userStore = services.GetRequiredService<IUserStore>();

            var rootUser = await userStore.GetByExternalId(TrustDefaults.KnownRootIdentifier);
            var rootId = rootUser.IsNull()
                ? await userStore.Create(TrustDefaults.KnownRootIdentifier, rootUserCreateData)
                : rootUser.Id;
            logger.LogInformation("[Seed] service user {0}", rootId);

            await services.GetRequiredService<IUserRightStore>()
                .CreateRights(
                    rootId,
                    TrustDefaults.KnownHomeResourceId,
                    new Dictionary<RightType, Permission>
                    {
                        {RightType.Root, Permission.ControlAccess | Permission.CreatorOwner},
                        {RightType.RootTenantCollection, Permission.ControlAccess | Permission.CreatorOwner},
                        {RightType.RootUserCollection, Permission.ControlAccess | Permission.CreatorOwner},
                    });
        }
    }
}