using System.Linq;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.NoSQL;
using Toolkit;
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
                await TableNameConstants
                    .Todo
                    .CreateTable(dbProvider.Client);
                await TableNameConstants
                    .Tag
                    .CreateTable(dbProvider.Client);

                var tagStore = new TagStore(dbProvider.Context);

                var todoStore = new TodoStore(dbProvider.Context, tagStore);

                var id = await todoStore.Create(new TodoCreateData {Name = "baba"});

                var todo = await todoStore.Get(id);

                Assert.Equal("baba", todo.Name);

                await todoStore.Delete(id);
            }
        }

        [Fact]
        public async Task Tagging()
        {
            using (var dbProvider = DynamoDbServerTestUtils.CreateDatabase())
            {
                await TableNameConstants
                    .Todo
                    .CreateTable(dbProvider.Client);
                await TableNameConstants
                    .Tag
                    .CreateTable(dbProvider.Client);

                var tagStore = new TagStore(dbProvider.Context);
                var todoStore = new TodoStore(dbProvider.Context, tagStore);

                var s = await Task
                    .WhenAll(new[] {"Work", "Play"}
                        .Select(async tag => await tagStore.Create(new TagCreateData {Name = tag})));

                var tagIds = s
                    .Where(result => result != null)
                    .ToList();


                var id = await todoStore.Create(new TodoCreateData {Name = "Todo with tags", Tags = tagIds});

                var todo = await todoStore.Get(id);

                Assert.Equal("Todo with tags", todo.Name);

                tagIds.ForEach(tag => Assert.Contains(tag, todo.Tags));

                await Task.WhenAll(tagIds.Select(tag => tagStore.Delete(tag)));

                await todoStore.Delete(id);
            }
        }
    }
}