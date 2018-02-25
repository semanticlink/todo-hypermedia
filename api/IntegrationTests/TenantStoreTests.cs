using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.NoSQL;
using Xunit;

namespace IntegrationTests
{
    public class TenanttoreTests
    {
        [Fact]
        public async Task LoadTodo()
        {
            using (var dbProvider = DynamoDbServerTestUtils.CreateDatabase())
            {
                TableNameConstants
                    .Tenant
                    .CreateTable(dbProvider.Client)
                    .ConfigureAwait(false);

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