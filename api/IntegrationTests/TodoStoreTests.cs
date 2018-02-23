using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.Db;
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
                var todoStore = new TodoStore(dbProvider.Client, dbProvider.Context);


                var name = await todoStore.BuildOrDescribeTable();

                Assert.Equal(name.TableName, TodoStore.TableName);

                var id = await todoStore.Create(new TodoCreateData {Name = "baba"});
               
                var todo = await todoStore.GetById(id);

                Assert.Equal("baba", todo.Name);
//                var retrievedUser = await dbProvider.Context.LoadAsync(user);

//                Assert.NotNull(retrievedUser);
//                Assert.Equal(user.UserName, retrievedUser.UserName);
//                Assert.Equal(user.NormalizedUserName, retrievedUser.NormalizedUserName);
            }
        }
    }
}