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