using Domain.Models;
using Xunit;

namespace MicroTests
{
    public class UserRightsTests
    {
        [Theory]
        [InlineData("no rights", null, Permissions.AllAccess, false)]
        [InlineData("creator", Permissions.FullCreatorOwner, Permissions.Creator, true)]
        [InlineData("get allowed on full control", Permissions.FullControl, Permissions.Get, true)]
        [InlineData("get not allowed on post only", Permissions.Post, Permissions.Get, false)]
        [InlineData("post not allowed on get only", Permissions.GetInheritPermissions, Permissions.Post, false)]
        [InlineData("delete not allowed on get/post", Permissions.Get | Permissions.Post, Permissions.Delete, false)]
        public void AllowCheck(string test, Permissions allocatedRights, Permissions requiredRights, bool allow)
        {
            Assert.Equal(allow, new UserRights {Rights = allocatedRights}.Allow(requiredRights));
        }
    }
}