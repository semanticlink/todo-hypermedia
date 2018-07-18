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
    ///     seed test data and the controllers (where it should be exculsively).
    /// </remarks>
    public static class CallerCollectionRights
    {
        public static readonly IDictionary<RightType, Permission> User = new Dictionary<RightType, Permission>
        {
            {RightType.RootUserCollection, Permission.Get},
            {RightType.RootTagCollection, Permission.Get | Permission.Post},
            {RightType.UserTodoCollection, Permission.FullControl},
            {RightType.TenantUserCollection, Permission.FullControl},
            {RightType.TodoCommentCollection, Permission.FullControl},
            {RightType.TodoTagCollection, Permission.FullControl},
            {RightType.TagTodoCollection, Permission.FullControl},
        };

        public static readonly IDictionary<RightType, Permission> Tenant = new Dictionary<RightType, Permission>
        {
            {RightType.RootTenantCollection, Permission.Get},
            {RightType.RootUserCollection, Permission.FullControl},
        };

        public static readonly IDictionary<RightType, Permission> Todo = new Dictionary<RightType, Permission>
        {
            {RightType.TodoTagCollection, Permission.FullControl},
            {RightType.TodoCommentCollection, Permission.FullControl},
        };

        public static readonly IDictionary<RightType, Permission> Tag = new Dictionary<RightType, Permission>
        {
            {RightType.TagTodoCollection, Permission.Get | Permission.Patch},
        };

        public static readonly IDictionary<RightType, Permission> Comment = new Dictionary<RightType, Permission>
        {
        };
    }
}