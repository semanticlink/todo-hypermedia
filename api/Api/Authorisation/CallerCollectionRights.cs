using System.Collections.Generic;
using Api.Web;
using Domain.Models;

namespace Api.Authorisation
{
    /// <summary>
    ///     Default rights for collections. By being here they should be easier to audit.
    /// </summary>
    /// <remarks>
    ///    Currently, these are here because they are shared between <see cref="DynamoDbSeedTestDataExtensions"/> which
    ///     seed test data and the controllers (where it should be exclusively).
    /// </remarks>
    public static class CallerCollectionRights
    {
        /// <summary>
        ///     This template/profile for the creation of a user in the context of the root user
        /// </summary>
        public static readonly IDictionary<RightType, Permission> Root = new Dictionary<RightType, Permission>
        {
            {RightType.RootTagCollection, Permission.Get | Permission.Post},
        };

        /// <summary>
        ///     This template/profile for the creation of a user in the context of the root user
        /// </summary>
        public static readonly IDictionary<RightType, Permission> User = new Dictionary<RightType, Permission>
        {
            {RightType.RootUserCollection, Permission.Get},
            {RightType.RootTagCollection, Permission.Get | Permission.Post},
            //
            {RightType.UserTodoCollection, Permission.FullControl},
            {RightType.UserTenantCollection, Permission.FullControl},
            //
            {RightType.Tenant, Permission.Get | Permission.Post},
            {RightType.TenantUserCollection, Permission.Get | Permission.Post},
            {RightType.TenantTodoCollection, Permission.FullControl},
            //
            {RightType.TodoCommentCollection, Permission.FullControl},
            {RightType.TodoTagCollection, Permission.FullControl},
            //
            {RightType.TagTodoCollection, Permission.FullControl},
        };

        public static readonly IDictionary<RightType, Permission> Tenant = new Dictionary<RightType, Permission>
        {
            {RightType.RootUserCollection, Permission.FullControl},
            //
            {RightType.TenantUserCollection, Permission.Get | Permission.Post},
            {RightType.TenantTodoCollection, Permission.Get | Permission.Post},
        };

        public static readonly IDictionary<RightType, Permission> Todo = new Dictionary<RightType, Permission>
        {
            {RightType.TodoTagCollection, Permission.FullControl},
            {RightType.TodoCommentCollection, Permission.FullControl},
        };

        public static readonly IDictionary<RightType, Permission> Tag = new Dictionary<RightType, Permission>
        {
            {RightType.TodoTagCollection, Permission.Get | Permission.Patch},
            //
            {RightType.TagTodoCollection, Permission.Get | Permission.Patch},
        };

        public static readonly IDictionary<RightType, Permission> Comment = new Dictionary<RightType, Permission>
        {
        };
    }
}