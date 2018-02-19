using System.Reflection;
using Domain.Models;
using Domain.Persistence;
using Infrastructure.Db;
using Microsoft.Extensions.DependencyInjection;

namespace App
{
    public static class IocRegistrations
    {
        public static IServiceCollection RegisterIoc(this IServiceCollection services)
        {
            services.AddScoped<ITodoRepository, TodoRepository>();
            services.AddScoped<ITenantRepository, TenantRepository>();
            
            services.AddSingleton(Assembly.GetEntryAssembly().GetName().Version);
            
            /**
             * Currently, this is the logged in user
             */
            services.AddSingleton(new User {Id = 1});

            return services;
        }
    }
}