using Domain.Models;
using Xunit;

namespace MicroTests
{
    public class UserRightsTests
    {
        [Theory]
        [InlineData("no rights", null, Permission.AllAccess, false)]
        [InlineData("creator", Permission.FullCreatorOwner, Permission.Creator, true)]
        [InlineData("get allowed on full control", Permission.FullControl, Permission.Get, true)]
        [InlineData("get not allowed on post only", Permission.Post, Permission.Get, false)]
        [InlineData("post not allowed on get only", Permission.GetInheritPermissions, Permission.Post, false)]
        [InlineData("delete not allowed on get/post", Permission.Get | Permission.Post, Permission.Delete, false)]
        public void AllowCheck(string test, Permission allocatedRights, Permission requiredRights, bool allow)
        {
            Assert.Equal(allow, new UserRight {Rights = allocatedRights}.Allow(requiredRights));
        }
    }
}