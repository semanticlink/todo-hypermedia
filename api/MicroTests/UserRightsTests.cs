using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            Assert.Equal(allow, new UserRight {Rights = allocatedRights}.IsAllowed(requiredRights));
        }
    }

    public class DiffTest
    {
        [Theory]
        [ClassData(typeof(ListData))]
        public void Bla(string desc, IList<int> left, IList<int> right, IList<int> add, IList<int> remove)
        {
            var intersect = left.Intersect(right).ToList();
            var toAdd = right.Except(intersect).ToList();
            var toRemove = left.Except(intersect).ToList();

            Assert.Equal(add, toAdd);
            Assert.Equal(remove, toRemove);
        }
    }

    public class ListData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return
                new object[] {"same", new List<int> {1}, new List<int> {1}, new List<int> { }, new List<int> { }};
            yield return
                new object[] {"add one", new List<int> {1}, new List<int> {1, 2}, new List<int> {2}, new List<int> { }};
            yield return
                new object[]
                    {"remove one", new List<int> {1, 2}, new List<int> {2}, new List<int> { }, new List<int> {1}};
            yield return
                new object[]
                {
                    "add and remove one", new List<int> {1, 2}, new List<int> {2, 3}, new List<int> {3},
                    new List<int> {1}
                };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}