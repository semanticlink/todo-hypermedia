using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.NoSQL;
using Xunit;

namespace IntegrationTests
{
    public class TodoStoreTests
    {
        [Fact]
        public async Task LoadTodo()
        {
            using (var dbProvider = DynamoDbServerTestUtils.CreateDatabase())
            {
                TableNameConstants
                    .Todo
                    .CreateTable(dbProvider.Client)
                    .ConfigureAwait(false);

                var todoStore = new TodoStore(dbProvider.Context);

                var id = await todoStore.Create(new TodoCreateData {Name = "baba"});

                var todo = await todoStore.Get(id);

                Assert.Equal("baba", todo.Name);
            }
        }
    }
}