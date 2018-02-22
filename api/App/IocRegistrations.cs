using System.Reflection;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Domain.Models;
using Domain.Persistence;
using Infrastructure.Db;
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
             * Entity Framework Repositories
             */
            services.AddScoped<ITodoRepository, TodoRepository>();
            services.AddScoped<ITenantRepository, TenantRepository>();

            /**
             * DynamoDb Stores
             */
            services.AddScoped<TodoStore>();

            services.AddSingleton(Assembly.GetEntryAssembly().GetName().Version);




            /**
             * Currently, this is the logged in user
             */
            services.AddSingleton(new User {Id = 1});

            return services;
        }
    }
}