using System.Threading.Tasks;
using Api.Authorisation;
using Domain.Models;
using Domain.Persistence;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class TodoListStoreTests : BaseTestProvider
    {
        public TodoListStoreTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task LoadTodoList()
        {
            var tenantStore = Get<ITenantStore>();
            var todoListStore = Get<ITodoListStore>();

            var tenantId = await tenantStore.Create(
                UserId,
                NewId(),
                new TenantCreateData
                {
                    Code = "test.rewire.nz",
                    Name = "baba",
                    Description = "new one"
                },
                Permission.AllAccess,
                CallerCollectionRights.Tenant);

            var tenant = await tenantStore.Get(tenantId);


            var id = await todoListStore.Create(
                UserId,
                UserId,
                new TodoListCreateData
                {
                    Name = "My baba list",
                    Tenant = tenant.Id
                },
                Permission.AllAccess,
                CallerCollectionRights.Todo);

            var todoList = await todoListStore.Get(id);

            Assert.Equal("My baba list", todoList.Name);

            await todoListStore.Delete(id);

            await Db.DeleteAsync<Tenant>(tenantId);
        }
    }
}