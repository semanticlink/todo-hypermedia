using System.Threading.Tasks;
using Domain.Models;
using Domain.Persistence;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class UserRightStoreTests : BaseTestProvider
    {
        public UserRightStoreTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task LoadUserRight()
        {
            var store = Get<IUserRightStore>();

            var userId = NewId();
            var resourceId = NewId();

            var id = await store.SetRight(userId, resourceId, RightType.Todo, Permission.Get);

            var userRights = await store.Get(userId, resourceId, RightType.Todo);
            Assert.NotNull(userRights);

            Assert.Equal(Permission.Get, userRights.Rights);
            Assert.Equal(RightType.Todo, userRights.Type);
            await Context.DeleteAsync<UserRight>(id);
        }

        [Fact]
        public async Task CreateUserRightWithNoInheritance()
        {
            var store = Get<IUserRightStore>();

            // seed the tenant owner
            var ownerId = NewId();
            var tenantUserId = NewId();

            // seed the top level tenant resource
            var resourceId =  NewId();

            await store.SetInherit(RightType.Tenant, ownerId, resourceId, RightType.Tenant,
                Permission.FullCreatorOwner);
            await store.SetInherit(RightType.Tenant, ownerId, resourceId, RightType.Tenant,
                Permission.FullCreatorOwner);
        }
    }
}