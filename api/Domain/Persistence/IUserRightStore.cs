using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Persistence
{
    public interface IUserRightStore
    {
        Task<string> SetRight(
            string userId,
            string resourceId,
            RightType type,
            Permission permission);

        Task<string> SetInherit(
            RightType inheritType,
            string userId,
            string resourceId,
            RightType rightType,
            Permission permission);

        Task CreateRights(
            string userId,
            string resourceId,
            IDictionary<RightType, Permission> granted,
            InheritForm resource = null);

        void Update(string userId, string resourceId, RightType rightType, Permission permission);
        Task<UserRight> Get(string userId, string resourceId, RightType rightType);
        Task<IEnumerable<UserRight>> Get(string userId, string resourceId);
        Task RemoveRight(string userId, string resourceId, RightType type);
    }
}