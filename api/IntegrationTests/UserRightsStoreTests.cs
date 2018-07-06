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

            Assert.NotNull(await store.Get(userId, resourceId));

            await DbProvider.Context.DeleteAsync<UserRights>(id);
        }

        [Fact]
        public void something()
        {
            var j = Permissions.Get;
            Assert.Equal(1L, (int)j);

            j = Permissions.Get | Permissions.Put;
            Assert.Equal(3, (int)j);
        }
    }
}