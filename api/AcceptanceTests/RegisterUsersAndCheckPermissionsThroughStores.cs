using Api.Web;
using IntegrationTests;
using Xunit;
using Xunit.Abstractions;

namespace AcceptanceTests
{
    public class RegisterUsersAndCheckPermissionsThroughStores : BaseTestProvider
    {
        public RegisterUsersAndCheckPermissionsThroughStores(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async void Users()
        {
            await ServiceProvider.SeedServiceUser(Log);
            await ServiceProvider.SeedData(Log);
        }
    }
}