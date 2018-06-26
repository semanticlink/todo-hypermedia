using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        public static IWebHost InitialiseDynamoDb(this IWebHost host, IHostingEnvironment hostingEnvironment)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    if (hostingEnvironment.IsDevelopment())
                    {
                        services.SeedTestData().ConfigureAwait(false);
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
        ///     Creates a tenant, user on the tenant and some todos with tags
        /// </summary>
        public static async Task<IServiceProvider> SeedTestData(this IServiceProvider services)
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

            var user = new IdentityUser {UserName = "test@rewire.nz", Email = "test@rewire.nz"};

            if (!userManager.Users.Any(u => u.UserName.Equals(user.UserName)))
            {
                var result = await userManager.CreateAsync(user, "Test123!");
                if (result.Succeeded)
                {
                    await tenantStore.AddUser(tenantId, user.Id);
                }
            }

            var tagStore = services.GetRequiredService<ITagStore>();

            // create some global tags

            var tagIds = (await Task
                    .WhenAll(new[] {"Work", "Personal", "Grocery List"}
                        .Select(tag => tagStore.Create(new TagCreateData {Name = tag}))))
                .Where(result => result != null)
                .ToList();

            var todoStore = services.GetRequiredService<ITodoStore>();

            await todoStore.Create(new TodoCreateData {Name = "One Todo"});
            await todoStore.Create(new TodoCreateData
            {
                Name = "Two Todo (tag)",
                Tags = new List<string> {tagIds.First()},
                State = TodoState.Complete
            });
            await todoStore.Create(new TodoCreateData
            {
                Name = "Three Todo (tagged)",
                Tags = tagIds
            });

            return services;
        }
    }
}