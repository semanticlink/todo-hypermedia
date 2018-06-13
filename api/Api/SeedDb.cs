using System;
using System.Runtime.CompilerServices;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation.Enum;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolkit;

namespace Api
{
    /// <summary>
    ///     All code around <see cref="DbInitializer" needs to be in <see cref="Program.Main"/> to avoid
    ///     problems with migrations
    /// </summary>
    public static class SeedDb
    {
        public static IWebHost InitialiseDynamoDb(this IWebHost host, IHostingEnvironment HostingEnvironment)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    if (HostingEnvironment.IsDevelopment())
                    {
                        services.SeedTestData();
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }

            return host;
        }

        /// <summary>
        ///     Creates a tenant, user on the tenant and some todos
        /// </summary>
        public static IServiceProvider SeedTestData(this IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Seeding DynamoDb data");

            var tenantStore = services.GetRequiredService<ITenantStore>();

            var tenantId = tenantStore.Create(new TenantCreateData
                {
                    Code = "rewire.example.nz",
                    Name = "Rewire NZ",
                    Description = "A sample tenant (company/organisation)"
                })
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var userStore = services.GetRequiredService<IUserStore>();

            var user = new IdentityUser {UserName = "test@rewire.nz", Email = "test@rewire.nz"};
            userManager.CreateAsync(user, "Test123!").ConfigureAwait(false);

            // now, we have the identity user, link this into the new user
            userStore.Create(
                    tenantId.ThrowAccessDeniedExceptionIfNull("No tenant provided to create a user"),
                    user.Id.ThrowAccessDeniedExceptionIfNull("No identity provided to create user"))
                .ConfigureAwait(false);

            var todoStore = services.GetRequiredService<ITodoStore>();

            todoStore.Create(new TodoCreateData {Name = "One Todo"}).ConfigureAwait(false);
            todoStore.Create(new TodoCreateData {Name = "Two Todo", State = TodoState.Complete}).ConfigureAwait(false);
            todoStore.Create(new TodoCreateData {Name = "Three Todo"}).ConfigureAwait(false);

            return services;
        }
    }
}