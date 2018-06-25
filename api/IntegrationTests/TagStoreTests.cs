using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.NoSQL;
using Xunit;

namespace IntegrationTests
{
    public class TagStoreTests
    {
        [Fact]
        public async Task LoadTag()
        {
            using (var dbProvider = DynamoDbServerTestUtils.CreateDatabase())
            {
                await TableNameConstants
                    .Tag
                    .CreateTable(dbProvider.Client);

                var tagStore = new TagStore(dbProvider.Context);

                var id = await tagStore.Create(new TagCreateData {Name = "working"});

                var tag = await tagStore.Get(id);

                Assert.Equal("working", tag.Name);

                await tagStore.Delete(id);
            }
        }
    }
}