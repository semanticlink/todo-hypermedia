using System.Linq;
using System.Threading.Tasks;
using Infrastructure.NoSQL;
using Xunit;

namespace IntegrationTests
{
    public class UserStoreTests
    {
        [Fact]
        public async Task LoadUser()
        {
            using (var dbProvider = DynamoDbServerTestUtils.CreateDatabase())
            {
                await TableNameConstants
                    .User
                    .CreateTable(dbProvider.Client);
                
                var userStore = new UserStore(dbProvider.Context);

                var id = await userStore.Create("auth0|349874545", "fred", "");
//                await userStore.Create("auth0|5b32b696a8c12d3b9a32b138", "fred2", "");

                var user = await userStore.Get(id);

                Assert.Contains("auth0|349874545", user.ExternalIds);

                user = (await userStore.GetByExternalId("auth0|349874545"));
                Assert.Equal(id, user.Id);
                
                Assert.NotEqual(id, user.ExternalIds.First());

                await userStore.Delete(id);
            }
        }
    }
}