using System.Linq;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation;
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
            RegisterUser();
            var userStore = Get<IUserStore>();

            var identityId = "auth0|349874545";
            var externalUser = new UserCreateDataRepresentation
            {
                Name = "fred",
                Email = ""
            };

            var id = await userStore.Create(identityId, externalUser);

            var user = await userStore.Get(id);

            Assert.Contains(identityId, user.ExternalIds);

            user = await userStore.GetByExternalId(identityId);
            Assert.Equal(id, user.Id);

            Assert.NotEqual(id, user.ExternalIds.First());

            await userStore.Delete(id);
        }
    }
}