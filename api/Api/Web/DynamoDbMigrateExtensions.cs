using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using App;
using Domain;
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
            Log.Info("[Seed] database creating");
            Task.Run(() => { services.GetService<IAmazonDynamoDB>().CreateAllTables().GetAwaiter().GetResult(); });
            Log.Debug("[Seed] database create complete");

            // we have added 'Scoped' services, this will return the root scope with them attached
            using (var scopedProvider = services.CreateScope())
            {
                Task.Run(scopedProvider.ServiceProvider.SeedServiceUser).GetAwaiter().GetResult();
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
            Log.Info("[Seed] service user start");

            // guard to check everything is up and running
            await services.GetRequiredService<IAmazonDynamoDB>().WaitForAllTables();

            //////////////////////////
            // Seed the root
            // =============
            //
            // Register a service account using our own scheme "service|id"
            //

            /**
             * Hand make up these because we want to inject the user with the provisioning user
             */
            var userRightStore = services.GetRequiredService<IUserRightStore>();
            var userStore = new UserStore(
                new User {Id = TrustDefaults.ProvisioningId},
                services.GetRequiredService<IDynamoDBContext>(),
                services.GetRequiredService<IIdGenerator>(),
                userRightStore);


            // TODO: get from configuration
            var rootUserCreateData = new UserCreateData
            {
                Name = "Service Account",
                Email = "root@rewire.nz",
                ExternalId = TrustDefaults.KnownRootIdentifier
            };

            if ((await userStore.GetByExternalId(rootUserCreateData.ExternalId)).IsNull())
            {
                // creat a user with explicit rights
                var rootId = await userStore.Create(
                    null, // skyhook
                    TrustDefaults.KnownHomeResourceId,
                    rootUserCreateData,
                    Permission.ControlAccess | Permission.Get | Permission.Owner,
                    new Dictionary<RightType, Permission>
                    {
                        {RightType.Root, Permission.ControlAccess | Permission.Get},
                        {RightType.RootTenantCollection, Permission.ControlAccess | Permission.Get},
                        {RightType.RootUserCollection, Permission.FullControl},
                    });

                // now set template rights for when new users 'RootUserCollection' are created on the root resource
                var inheritRights = new Dictionary<RightType, Permission>
                {
                    {RightType.Todo, Permission.CreatorOwner},
                    {RightType.TodoTagCollection, Permission.AllAccess},
                    {RightType.TodoCommentCollection, Permission.AllAccess},
                    //
                    {RightType.Tag, Permission.Get},
                    //
                    {RightType.Comment, Permission.FullControl},
                };

                await Task.WhenAll(inheritRights.Select(right =>
                    userRightStore.SetInherit(
                        RightType.RootUserCollection,
                        rootId,
                        TrustDefaults.KnownHomeResourceId,
                        right.Key,
                        right.Value)
                ));
                
                Log.Info("[Seed] service user '{0}'", rootUserCreateData.ExternalId);
            }
            else
            {
                Log.Info("[Seed] service user already exists");
            }
        }
    }
}