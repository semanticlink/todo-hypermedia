using System;

namespace Domain.Models
{
    public static class UserRightsExtensions
    {
        public static bool Allow(this UserRights userRights, Permissions permissions)
        {
            var tryParse = Enum.TryParse<Permissions>(userRights.Rights.ToString(), out var userPerms);
            return tryParse && userPerms.HasFlag(permissions);
        }
    }
}