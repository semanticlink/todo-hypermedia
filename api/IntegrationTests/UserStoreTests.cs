using System.Linq;
using System.Threading.Tasks;
using Domain.Persistence;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class UserStoreTests : BaseTestProvider
    {
        public UserStoreTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task LoadUser()
        {
            var userStore = Get<IUserStore>();

            var id = await userStore.Create("auth0|349874545", "fred", "");

            var user = await userStore.Get(id);

            Assert.Contains("auth0|349874545", user.ExternalIds);

            user = await userStore.GetByExternalId("auth0|349874545");
            Assert.Equal(id, user.Id);

            Assert.NotEqual(id, user.ExternalIds.First());

            await userStore.Delete(id);
        }
    }
}