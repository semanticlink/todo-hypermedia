using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.NoSQL;
using Xunit;

namespace IntegrationTests
{
    public class TenantStoreTests
    {
        [Fact]
        public async Task LoadTenant()
        {
            using (var dbProvider = DynamoDbServerTestUtils.CreateDatabase())
            {
                await TableNameConstants
                    .Tenant
                    .CreateTable(dbProvider.Client);

                var todoStore = new TenantStore(dbProvider.Context);

                var code = "test.rewire.nz";
                var tenantCreateData = new TenantCreateData
                {
                    Code = code,
                    Name = "baba"
                };
                var id = await todoStore.Create(tenantCreateData);

                var tenant = await todoStore.Get(id);

                Assert.Equal(code, tenant.Code);
                Assert.Equal("baba", tenant.Name);
            }
        }
    }
}