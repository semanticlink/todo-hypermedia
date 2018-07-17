﻿using System.Reflection;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using App.Authorisation;
using Domain;
using Domain.Models;
using Domain.Persistence;
using Infrastructure.NoSQL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace App
{
    public static class IocRegistrations
    {
        /// <summary>
        ///     All the services that are required for the Api to run. This is a wrapper bundling all the
        ///     services so that the bundling is in the same file as the groupings of registrations. 
        /// </summary>
        /// <remarks>
        ///     Test libraries will replicate this bundling for themselves picking and choosing as needed.
        /// </remarks>
        public static IServiceCollection RegisterApiIoc(this IServiceCollection services, IHostingEnvironment env)
        {
            return services
                .RegisterInfrastructure(env.IsDevelopment())
                .RegisterApiServices()
                .RegisterRespositories();
        }

        /// <summary>
        ///     Register the infrastructure/integration points for the system (eg databases, mail)
        /// </summary>
        /// <param name="services"></param>
        /// <param name="isDevelopment">Set to true to point at the locally deployed database</param>
        public static IServiceCollection RegisterInfrastructure(
            this IServiceCollection services,
            bool isDevelopment)
        {
            /**
             * DymanoDb Registration
             */
            services.AddSingleton<IAmazonDynamoDB>(i => isDevelopment
                ? new AmazonDynamoDBClient(new AmazonDynamoDBConfig
                {
                    ServiceURL = "http://localhost:8000" // TODO: inject
                })
                : new AmazonDynamoDBClient());
            services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

            /**
             * Register an Id generator for use through repositories (and tests)
             */
            services.AddSingleton<IIdGenerator, IdGenerator>();


            /**
             * Register the authorisation handlers across resource types
             */
            services.AddTransient<IAuthorizationHandler, HasPermissionsHandler>();

            return services;
        }

        /// <summary>
        ///     These services should only be registered in the context of a server because they require the
        ///     <see cref="HttpContext"/>.
        /// </summary>
        /// <remarks>
        ///    Test libraries will need to create each of these registrations for themselves.
        /// </remarks>
        public static IServiceCollection RegisterApiServices(this IServiceCollection services)
        {
            /**
             * Register up the user off the context of the Bearer --> Identity (external) --> User (internal)
             */
            // see https://github.com/aspnet/Hosting/issues/793 (TODO: clarify)
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<UserResolverService, UserResolverService>();
            services.AddSingleton<User>(context => context.GetService<UserResolverService>().GetUser());
//            services.AddScoped<User>(context => new User());

            // Version from the assmembly (displayed on home resource)
            services.AddSingleton(Assembly.GetEntryAssembly().GetName().Version);
            
            
            ////////////////////////////////
            // Authorisation
            //
            
            // Replace the default authorization policy provider with our own
            // custom provider which can return authorization policies for given
            // policy names (instead of using the default policy provider)
            services.AddSingleton<IAuthorizationPolicyProvider, CollectionPolicyProvider>();

            // As always, handlers must be provided for the requirements of the authorization policies
            services.AddSingleton<IAuthorizationHandler, HasPermissionsHandler>();


            return services;
        }

        /// <summary>
        ///     Register all the persistence layer services (eg repositories/stores)
        /// </summary>
        /// <remarks>
        ///    Most of these services will *also* require other services that are not registered in here (eg id generators, time, currency)
        /// </remarks>
        public static IServiceCollection RegisterRespositories(this IServiceCollection services)
        {
            /**
             * DynamoDb Stores
             */
            services.AddScoped<ITodoStore, TodoStore>();
            services.AddScoped<ITenantStore, TenantStore>();
            services.AddScoped<IUserStore, UserStore>();
            services.AddScoped<ITagStore, TagStore>();
            services.AddSingleton<IUserRightStore, UserRightStore>();

            return services;
        }
    }
}