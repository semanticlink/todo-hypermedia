using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.NoSQL;
using Xunit;

namespace IntegrationTests
{
    public class UserRightsPlayTests : BaseProvider
    {
        private readonly Func<DynamoDbServerTestUtils.DisposableDatabase, Task<UserRightsStore>> MakeStore =
            async dbProvider =>
            {
                await TableNameConstants
                    .UserRight
                    .CreateTable(dbProvider.Client);

                return new UserRightsStore(dbProvider.Context);
            };

        [Fact]
        public async Task LoadUserRight()
        {
            var store = await MakeStore(DbProvider);

            var userId = IdGenerator.New();
            var resourceId = IdGenerator.New();

            var id = await store.Create(userId, resourceId, RightType.Todo, Permission.Get);

            var userRights = await store.Get(userId, resourceId);


            await store.CreateRights(
                userId,
                resourceId,
                new Dictionary<RightType, Permission>
                {
                    {RightType.Comment, Permission.AllAccess},
                    {RightType.Todo, Permission.Delete},
                },
                new InheritForm
                {
                    ResourceId = resourceId,
                    Type = RightType.Comment,
                    InheritedTypes = new List<RightType>
                    {
                        RightType.Tag,
                        RightType.TenantTodoCollection
                    }
                }
            );

            await DbProvider.Context.DeleteAsync<UserRight>(id);
        }

        [Fact]
        public async Task CreateUserRightWithNoInheritance()
        {
            var store = await MakeStore(DbProvider);

            // seed the tenant owner
            var ownerId = IdGenerator.New();
            var tenantUserId = IdGenerator.New();

            // seed the top level tenant resource
            var resourceId = IdGenerator.New();

            await store.CreateInherit(RightType.Tenant, ownerId, resourceId, RightType.Tenant,
                Permission.FullCreatorOwner);
            await store.CreateInherit(RightType.Tenant, ownerId, resourceId, RightType.Tenant,
                Permission.FullCreatorOwner);
        }
    }
}