using System.Linq;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Persistence;
using Infrastructure.NoSQL;
using Microsoft.Extensions.DependencyInjection;
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
            var userId = IdGenerator.New();
            Register(services => { services.AddTransient(ctx => new User {Id = userId}); });
            var todoStore = Get<ITodoStore>();


            var id = await todoStore.Create(new TodoCreateData {Name = "baba"});

            var todo = await todoStore.Get(id);

            Assert.Equal("baba", todo.Name);

            await todoStore.Delete(id);
        }

        [Fact]
        public async Task Tagging()
        {
            var userId = IdGenerator.New();
            Register(services => { services.AddTransient(ctx => new User {Id = userId}); });
            var todoStore = Get<ITodoStore>();
            var tagStore = Get<ITagStore>();


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