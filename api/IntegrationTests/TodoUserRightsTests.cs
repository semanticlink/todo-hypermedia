using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Domain.Models;
using Infrastructure.NoSQL;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class TodoUserRightsTests : BaseTestProvider
    {
        public TodoUserRightsTests(ITestOutputHelper output) : base(output)
        {
        }

        private readonly Func<DynamoDbServerTestUtils.DisposableDatabase, string, Task<Tuple<UserRightStore, TodoStore>>
            >
            MakeStores =
                async (dbProvider, userId) =>
                {
                    var user = new User{ Id = userId};
                    
                    await TableNameConstants
                        .UserRight
                        .CreateTable(dbProvider.Client);
                    await TableNameConstants
                        .Todo
                        .CreateTable(dbProvider.Client);
                    await TableNameConstants
                        .Tag
                        .CreateTable(dbProvider.Client);

                    var userRightStore = new UserRightStore(dbProvider.Context);
                    return new Tuple<UserRightStore, TodoStore>(
                        userRightStore,
                        new TodoStore(dbProvider.Context, userRightStore, user)
                    );
                };

        [Theory]
        [InlineData(Permission.Get, Permission.Get, Permission.Post, false)]
        [InlineData(Permission.Get, Permission.Get, Permission.Get, true)]
        public async Task LoadUserRight(
            Permission explicitRights,
            Permission callerRights,
            Permission requiredPermission,
            bool allow)
        {
            // Setup 
            //   - user
            //   - todo collection context
            //   - todo (that we want to created)

            var userId = IdGenerator.New();

            var stores = await MakeStores(DbProvider, userId);
            var userRightStore = stores.Item1;
            var todoStore = stores.Item2;


            var contextResourceId = IdGenerator.New();

            var createData = new TodoCreateData
            {
                Name = "Test"
            };

            /////////////////////////////////////////////////////////////
            // Do the creation of a todo with the rights we want
            //
            var todoId = await todoStore.Create(
                contextResourceId,
                createData,
                explicitRights,
                new Dictionary<RightType, Permission>
                {
                    {RightType.UserTodoCollection, callerRights}
                });


            // Results
            //  - find number of user rights and inherited rights
            //  - then, would the user be granted access?
            var userRights = await DbProvider.Context.ScanAsync<UserRight>(new List<ScanCondition>
                {
                    new ScanCondition(nameof(UserRight.UserId), ScanOperator.Equal, userId)
                })
                .GetRemainingAsync();
            var userInheritRights = await DbProvider.Context.ScanAsync<UserInheritRight>(new List<ScanCondition>
                {
                    new ScanCondition(nameof(UserInheritRight.UserId), ScanOperator.Equal, userId)
                })
                .GetRemainingAsync();

            Assert.Equal(2, userRights.Count);
            Assert.Equal(0, userInheritRights.Count);

            // Access?
            var resourceRights = await userRightStore.Get(userId, todoId, RightType.Todo);
            Assert.Equal(allow, resourceRights.Allow(requiredPermission));

            // Clean up
            await Task.WhenAll(userRights.Select(right => DbProvider.Context.DeleteAsync<UserRight>(right.Id)));
            await Task.WhenAll(userInheritRights.Select(right =>
                DbProvider.Context.DeleteAsync<UserInheritRight>(right.Id)));
            await DbProvider.Context.DeleteAsync<Todo>(todoId);
        }
    }
}