using System;
using System.ComponentModel;

namespace Domain.Models
{
    /// <summary>
    ///     The right type is used as a key/id in the database so that each resource can
    ///     be uniquely identified.
    /// </summary>
    public enum RightType
    {
        //////////////////////////////////////////////////////////////////////
        // 
        //  Root (Home)
        //  ===========
        //

        Root = 100,

        [Description("Tenants")] RootTenantCollection = 101,
        [Description("Users")] RootUserCollection = 102,
        [Description("Tags")] RootTagCollection = 103,

        //////////////////////////////////////////////////////////////////////
        // 
        //  User
        //  ====
        //

        User = 200,

        [Description("Tenants")] UserTenantCollection = 201,
        [Description("Todos")] UserTodoCollection = 202,

        //////////////////////////////////////////////////////////////////////
        // 
        //  Tenant
        //  ======
        //

        Tenant = 300,

        [Description("Users")] TenantUserCollection = 301,
        [Description("Todos")] TenantTodoCollection = 302,

        //////////////////////////////////////////////////////////////////////
        // 
        //  Todo
        //  ====
        //

        Todo = 400,

        [Description("Tags")] TodoTagCollection = 401,
        [Description("Comments")] TodoCommentCollection = 402,
        //////////////////////////////////////////////////////////////////////
        // 
        //  Tag
        //  ===
        //

        /// <summary>
        ///     A tag is a shared resource and as such at the moment
        ///     it is open to everyone, so no perms are set
        /// </summary>
        Tag = 500,

        [Description("Todos")] TagTodoCollection = 501,

        //////////////////////////////////////////////////////////////////////
        // 
        //  Comment
        //  =======
        //

        Comment = 600,
    }
}