using System.Linq;
using System.Threading.Tasks;
using Api.Authorisation;
using App;
using Domain.Models;
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

            var identityId = "auth0|349874545";
            var data = new UserCreateData
            {
                Name = "fred",
                Email = "",
                ExternalId = identityId
            };

            var id = await userStore.Create(
                TrustDefaults.KnownRootIdentifier,
                TrustDefaults.KnownHomeResourceId,
                data,
                Permission.FullControl,
                CallerCollectionRights.User
            );

            var user = await userStore.Get(id);

            Assert.Contains(identityId, user.ExternalIds);

            user = await userStore.GetByExternalId(identityId);
            Assert.Equal(id, user.Id);

            Assert.NotEqual(id, user.ExternalIds.First());

            await userStore.Delete(id);
        }
    }
}