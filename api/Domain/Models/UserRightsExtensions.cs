using System;

namespace Domain.Models
{
    public static class UserRightsExtensions
    {
        public static bool Allow(this UserRight userRight, Permission permission)
        {
            var tryParse = Enum.TryParse<Permission>(userRight.Rights.ToString(), out var userPerms);
            return tryParse && userPerms.HasFlag(permission);
        }
    }
}