using System.Reflection;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Domain.Persistence;
using Infrastructure.NoSQL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace App
{
    public static class IocRegistrations
    {
        public static IServiceCollection RegisterIoc(this IServiceCollection services, IHostingEnvironment env)
        {
            /**
             * DymanoDb Registration
             */
            services.AddSingleton<IAmazonDynamoDB>(i => env.IsDevelopment()
                ? new AmazonDynamoDBClient(new AmazonDynamoDBConfig
                {
                    ServiceURL = "http://localhost:8000"
                })
                : new AmazonDynamoDBClient());
            services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

            /**
             * DynamoDb Stores
             */
            services.AddScoped<ITodoStore, TodoStore>();
            services.AddScoped<ITenantStore, TenantStore>();
            services.AddScoped<IUserStore, UserStore>();
            services.AddScoped<ITagStore, TagStore>();

            services.AddSingleton(Assembly.GetEntryAssembly().GetName().Version);

            return services;
        }
    }
}