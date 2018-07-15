using System.Reflection;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Domain.Persistence;
using Infrastructure.NoSQL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace App
{
    public static class IocRegistrations
    {
        public static IServiceCollection RegisterIoc(this IServiceCollection services, IHostingEnvironment env)
        {
            return services
                .RegisterInfrastructure(env.IsDevelopment())
                .RegisterServices()
                .RegisterRespositories();
        }

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

            return services;
        }

        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            /**
             * Register up the user off the context of the Bearer --> Identity (external) --> User (internal)
             */
            // see https://github.com/aspnet/Hosting/issues/793 (TODO: clarify)
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<UserResolverService, UserResolverService>();
            services.AddScoped(context => context.GetService<UserResolverService>().GetUser());

            // Version from the assmembly (displayed on home resource)
            services.AddSingleton(Assembly.GetEntryAssembly().GetName().Version);

            return services;
        }

        public static IServiceCollection RegisterRespositories(this IServiceCollection services)
        {
            /**
             * DynamoDb Stores
             */
            services.AddScoped<ITodoStore, TodoStore>();
            services.AddScoped<ITenantStore, TenantStore>();
            services.AddScoped<IUserStore, UserStore>();
            services.AddScoped<ITagStore, TagStore>();
            services.AddScoped<IUserRightStore, UserRightStore>();

            return services;
        }
    }
}