using System;
using System.ComponentModel;

namespace Domain.Models
{
    [Flags]
    public enum Permission : uint
    {
        None = 0,
        View = Get,

        /// <summary>
        ///     <see cref="Creator"/> | <see cref="Owner"/>
        /// </summary>
        CreatorOwner = Creator | Owner,

        /// <summary>
        ///     <see cref="AllAccess"/> | <see cref="ControlAccess"/>
        /// </summary>
        FullControl = AllAccess | ControlAccess,

        /// <summary>
        ///     <see cref="Get"/> | <see cref="Put"/> | <see cref="Post"/> | <see cref="Delete"/> | <see cref="Patch"/>
        /// </summary>
        AllAccess = Get | Put | Post | Delete | Patch,

        /// <summary>
        ///     <see cref="GetPermissions"/> | <see cref="PutPermissions"/> | <see cref="GetInheritPermissions"/> | <see cref="PutInheritPermissions"/>
        /// </summary>
        ControlAccess = GetPermissions | PutPermissions | GetInheritPermissions | PutInheritPermissions,

        /// <summary>
        ///     <see cref="FullControl"/> | <see cref="CreatorOwner"/>
        /// </summary>
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
        ///   The right to get authorisation permissions (ie read permissions through the api)
        /// </summary>
        GetPermissions = 1 << 16,

        /// <summary>
        ///   The right to put authorisation permissions (ie update/set permissions through the api)
        /// </summary>
        /// <remarks>
        ///   Logically all objects have permissions, so there is no post action.
        /// </remarks>
        PutPermissions = 1 << 17,

        /// <summary>
        ///   The right to set inheritable authorisation permissions (ie read permission templates through the api)
        /// </summary>
        /// <seealso cref="UserInheritRight"/>
        GetInheritPermissions = 1 << 18,

        /// <summary>
        ///   The right to put inheritable authorisation permissions (ie update/set permissions templates through the api)
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