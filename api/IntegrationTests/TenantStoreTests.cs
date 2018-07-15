using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Persistence;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class TenantTestProviderTests : BaseTestProvider
    {
        public TenantTestProviderTests(ITestOutputHelper output) : base(output)
        {
        }

        private readonly Func<ITenantStore, Task<string>> Create = async store =>
        {
            var tenantCreateData = new TenantCreateData
            {
                Code = "test.rewire.nz",
                Name = "baba",
                Description = "new one"
            };
            return await store.Create(tenantCreateData);
        };

        [Fact]
        public async Task LoadTenant()
        {
            var tenantStore = Get<ITenantStore>();
            var id = await Create(tenantStore);
            var tenant = await tenantStore.Get(id);

            Assert.Equal("test.rewire.nz", tenant.Code);
            Assert.Equal("baba", tenant.Name);
            Assert.Equal("new one", tenant.Description);

            await Context.DeleteAsync<Tenant>(id);
        }

        [Fact]
        public async Task UserRemoveUser()
        {
            var tenantStore = Get<ITenantStore>();
            var id = await Create(tenantStore);
            var tenant = await tenantStore.Get(id);

            // default is an empty list of users
            Assert.Empty(tenant.User ?? new List<string>());

            // add a user
            var userId = Guid.NewGuid().ToString();
            await tenantStore.AddUser(id, userId);
            tenant = await tenantStore.Get(id);
            Assert.Contains(userId, tenant.User);

            // remove a user
            await tenantStore.RemoveUser(id, userId);
            tenant = await tenantStore.Get(id);
            Assert.DoesNotContain(userId, tenant.User ?? new List<string>());

            await Context.DeleteAsync<Tenant>(id);
        }
    }
}