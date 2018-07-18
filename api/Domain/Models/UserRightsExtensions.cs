using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public static class UserRightsExtensions
    {
        public static bool IsAllowed(this UserRight userRight, Permission permission)
        {
            var tryParse = Enum.TryParse<Permission>(userRight.Rights.ToString(), out var userPerms);
            return tryParse && userPerms.HasFlag(permission);
        }

        public static IDictionary<RightType, Permission> MakeCreateRights( 
            this RightType type,
            Permission permission,
            IDictionary<RightType, Permission> contextPermissions)
        {
            
            var rights = new Dictionary<RightType, Permission>
            {
                {type, permission}
                
            };

            if (contextPermissions != null)
            {
                foreach (var aPermission in contextPermissions)
                {
                    rights.Add(aPermission.Key, aPermission.Value);
                }
            }

            return rights;
        }
    }
    
 
}