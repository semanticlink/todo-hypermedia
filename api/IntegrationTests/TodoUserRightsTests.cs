using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Domain.Models;
using Domain.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class TodoUserRightsTests : BaseTestProvider
    {
        public TodoUserRightsTests(ITestOutputHelper output) : base(output)
        {
        }

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

            var userId = NewId();

            Register(services => { services.AddTransient(ctx => new User {Id = userId}); });

            var userRightStore = Get<IUserRightStore>();
            var todoStore = Get<ITodoStore>();

            var contextResourceId = NewId();

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
            var userRights = await Context.ScanAsync<UserRight>(new List<ScanCondition>
                {
                    new ScanCondition(nameof(UserRight.UserId), ScanOperator.Equal, userId)
                })
                .GetRemainingAsync();
            var userInheritRights = await Context.ScanAsync<UserInheritRight>(new List<ScanCondition>
                {
                    new ScanCondition(nameof(UserInheritRight.UserId), ScanOperator.Equal, userId)
                })
                .GetRemainingAsync();

            Assert.Equal(2, userRights.Count);
            Assert.Empty(userInheritRights);

            // Access?
            var resourceRights = await userRightStore.Get(userId, todoId, RightType.Todo);
            Assert.Equal(allow, resourceRights.isAllowed(requiredPermission));

            // Clean up
            await Task.WhenAll(userRights.Select(right => Context.DeleteAsync<UserRight>(right.Id)));
            await Task.WhenAll(userInheritRights.Select(right =>
                Context.DeleteAsync<UserInheritRight>(right.Id)));
            await Context.DeleteAsync<Todo>(todoId);
        }
    }
}