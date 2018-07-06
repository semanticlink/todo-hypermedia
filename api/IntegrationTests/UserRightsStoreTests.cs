using System;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.NoSQL;
using Xunit;

namespace IntegrationTests
{
    public class UserRightsStoreTests : BaseProvider
    {
        private readonly Func<DynamoDbServerTestUtils.DisposableDatabase, Task<UserRightsStore>> MakeStore =
            async dbProvider =>
            {
                await TableNameConstants
                    .UserRights
                    .CreateTable(dbProvider.Client);

                return new UserRightsStore(dbProvider.Context);
            };

        [Fact]
        public async Task LoadUserRight()
        {
            var store = await MakeStore(DbProvider);

            var userId = IdGenerator.New();
            var resourceId = IdGenerator.New();

            var id = await store.Create(userId, resourceId, ResourceType.Todo, Permissions.Get);

            var userRights = await store.Get(userId, resourceId);
            Assert.NotNull(userRights);

            Assert.Equal(Permissions.Get, userRights.Rights);
            Assert.Equal(ResourceType.Todo, userRights.Type);
            await DbProvider.Context.DeleteAsync<UserRights>(id);
        }
    }
}