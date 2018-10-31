using System.Linq;
using System.Threading.Tasks;
using Api.Authorisation;
using Domain.Models;
using Domain.Persistence;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class TodoStoreTests : BaseTestProvider
    {
        public TodoStoreTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task LoadTodo()
        {
            var todoStore = Get<ITodoStore>();

            var s = await todoStore.Create(
                UserId,
                UserId,
                new TodoCreateData {Name = "baba", Parent = NewId()},
                Permission.AllAccess,
                CallerCollectionRights.Todo);
            var id = s;

            var todo = await todoStore.Get(id);

            Assert.Equal("baba", todo.Name);

            await todoStore.Delete(id);
        }

        [Fact]
        public async Task Tagging()
        {
            var todoStore = Get<ITodoStore>();
            var tagStore = Get<ITagStore>();

            var tagIds = (await Task
                    .WhenAll(new[] {"Work", "Play"}
                        .Select(async tag => await tagStore.Create(
                            UserId,
                            NewId(),
                            new TagCreateData {Name = tag},
                            Permission.AllAccess,
                            CallerCollectionRights.Tag))))
                .Where(result => result != null)
                .ToList();


            var s = await todoStore.Create(UserId, UserId,
                new TodoCreateData {Name = "Todo with tags", Parent = NewId(), Tags = tagIds},
                Permission.AllAccess,
                CallerCollectionRights.Todo);
            var id = s;

            var todo = await todoStore.Get(id);

            Assert.Equal("Todo with tags", todo.Name);

            tagIds.ForEach(tag => Assert.Contains(tag, todo.Tags));

            await todoStore.Delete(id);
            await Task.WhenAll(tagIds.Select(tag => Db.DeleteAsync<Tag>(tag)));
        }
    }
}