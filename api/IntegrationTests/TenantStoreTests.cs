using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Authorisation;
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
            _tenantCreateData = new TenantCreateData
            {
                Code = "test.rewire.nz",
                Name = "baba",
                Description = "new one"
            };
        }

        private readonly TenantCreateData _tenantCreateData;

        [Fact]
        public async Task LoadTenant()
        {
            var tenantStore = Get<ITenantStore>();
            var id = await tenantStore.Create(
                UserId,
                NewId(),
                _tenantCreateData,
                Permission.AllAccess,
                CallerCollectionRights.Tenant);

            var tenant = await tenantStore.Get(id);

            Assert.Equal("test.rewire.nz", tenant.Code);
            Assert.Equal("baba", tenant.Name);
            Assert.Equal("new one", tenant.Description);

            await Db.DeleteAsync<Tenant>(id);
        }

        [Fact]
        public async Task UserRemoveUser()
        {
            var tenantStore = Get<ITenantStore>();
            var id = await tenantStore.Create(
                UserId,
                NewId(),
                _tenantCreateData,
                Permission.AllAccess,
                CallerCollectionRights.Tenant);

            var tenant = await tenantStore.Get(id);

            // default is an empty list of users
            Assert.Empty(tenant.User ?? new List<string>());

            // add a user
            var userId = NewId();
            await tenantStore.IncludeUser(id, userId, Permission.AllAccess, null);
            
            tenant = await tenantStore.Get(id);
            Assert.Contains(userId, tenant.User);

            // remove a user
            await tenantStore.RemoveUser(id, userId);
            tenant = await tenantStore.Get(id);
            Assert.DoesNotContain(userId, tenant.User ?? new List<string>());

            await Db.DeleteAsync<Tenant>(id);
        }
    }
}