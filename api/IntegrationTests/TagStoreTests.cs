using System.Threading.Tasks;
using Domain.Models;
using Domain.Persistence;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class TagStoreTests : BaseTestProvider
    {
        public TagStoreTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task LoadTag()
        {
            var tagStore = Get<ITagStore>();

            var id = await tagStore.Create(
                UserId,
                NewId(),
                new TagCreateData {Name = "working"},
                Permission.AllAccess,
                null);

            var tag = await tagStore.Get(id);

            Assert.Equal("working", tag.Name);

            await Db.DeleteAsync<Tag>(id);
        }
    }
}