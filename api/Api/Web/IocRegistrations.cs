using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Domain;
using Domain.Persistence;
using Infrastructure.NoSQL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Web
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
        public static IServiceCollection RegisterAppIoc(this IServiceCollection services, IHostingEnvironment env)
        {
            return services
                .RegisterInfrastructure(env.IsDevelopment())
                .RegisterRepositories();
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
             * DynamoDb Registration
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

            return services;
        }


        /// <summary>
        ///     Register all the persistence layer services (eg repositories/stores)
        /// </summary>
        /// <remarks>
        ///     Most of these services will *also* require other services that are not registered in here (eg id generators, time,
        ///     currency)
        /// </remarks>
        public static IServiceCollection RegisterRepositories(this IServiceCollection services)
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