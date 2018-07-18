using System;
using System.ComponentModel;

namespace Domain.Models
{
    [Flags]
    public enum Permission : uint
    {
        None = 0,
        View = Get,
        CreatorOwner = Creator | Owner,
        FullControl = AllAccess | ControlAccess,
        AllAccess = Get | Put | Post | Delete | Patch,
        ControlAccess = GetPermissions | PutPermissions | GetInheritPermissions | PutInheritPermissions,
        FullCreatorOwner = FullControl | CreatorOwner,

        /// <summary>
        ///   Get content from the object
        /// </summary>
        [Description("Access to read the resource")]
        Get = 1 << 0,

        Put = 1 << 1,
        Post = 1 << 2,

        /// <summary>
        ///   Delete the resource or part of the resource
        /// </summary>
        Delete = 1 << 3,
        
        /// <summary>
        ///   Patch the resource
        /// </summary>
        /// <remarks>
        ///   Logically a patch is merely a partial update (compared a full update of PUT)
        /// </remarks>
        Patch = Put,

        /// <summary>
        ///   The right to get authorisation permissions
        /// </summary>
        GetPermissions = 1 << 16,

        /// <summary>
        ///   The right to put authorisation permissions
        /// </summary>
        /// <remarks>
        ///   Logically all objects have permissions, so there is no post action.
        /// </remarks>
        PutPermissions = 1 << 17,

        /// <summary>
        ///   The right to set inheritable authorisation permissions
        /// </summary>
        /// <seealso cref="UserInheritRight"/>
        GetInheritPermissions = 1 << 18,

        /// <summary>
        ///   The right to put inheritable authorisation permissions
        /// </summary>
        /// <seealso cref="UserInheritRight"/>
        PutInheritPermissions = 1 << 19,

        /// <summary>
        ///   A flag to indicate that this right was granted by inheritance
        ///   when the resource was created. When a right is explicitly set
        ///   this flag will be cleared.
        /// </summary>
        Inherited = 1 << 24,

        /// <summary>
        ///   An owner of an object can delegate rights to another
        ///   party, without having to have rights to grant to them.
        /// </summary>
        Owner = 1 << 29,
        Creator = 1 << 30,
    }
}