using Domain.Models;
using Microsoft.AspNetCore.Authorization;

namespace Api.Authorisation
{
    /// <summary>
    ///     Generic class that enforces the policy name (a serialised string) as required by <see cref="IAuthorizationPolicyProvider"/>
    /// </summary>
    /// <remarks>
    ///    The <see cref="AuthoriseAttribute.Policy"/> is a string that we encode/decode between the attribute and the policy creation
    /// </remarks>
    /// <example>
    ///  public class AuthorizeRootUserCollectionAttribute : AuthoriseAttribute
    ///  {
    ///      public AuthorizeRootUserCollectionAttribute(Permission permission = Permission.None, string resourceKey = "id")
    ///          : base(RightType.RootUserCollection, permission, resourceKey)
    ///      {
    ///      }
    ///  }
    /// </example>
    public class AuthoriseAttribute : AuthorizeAttribute
    {
        public AuthoriseAttribute(
            RightType type, 
            Permission permission = Permission.None,
            string resourceKey = ResourceKey.Id)
        {
            // here's the magic of a delimited string
            Policy = PolicyName.Make(type, permission, resourceKey).Serialise();
        }
    }

    public class AuthoriseRootUserCollectionAttribute : AuthoriseAttribute
    {
        public AuthoriseRootUserCollectionAttribute(Permission permission)
            : base(RightType.RootUserCollection, permission, ResourceKey.Root)
        {
        }
    }

    public class AuthoriseRootTenantCollectionAttribute : AuthoriseAttribute
    {
        public AuthoriseRootTenantCollectionAttribute(Permission permission)
            : base(RightType.RootTenantCollection, permission, ResourceKey.Root)
        {
        }
    }

    public class AuthoriseTenantUserCollectionAttribute : AuthoriseAttribute
    {
        public AuthoriseTenantUserCollectionAttribute(Permission permission)
            : base(RightType.TenantUserCollection, permission)
        {
        }
    }

    public class AuthoriseTodoAttribute : AuthoriseAttribute
    {
        public AuthoriseTodoAttribute(Permission permission)
            : base(RightType.Todo, permission)
        {
        }
    }

    public class AuthoriseUserTenantTodoCollectionAttribute : AuthoriseAttribute
    {
        public AuthoriseUserTenantTodoCollectionAttribute(Permission permission, string resourceKey = "tenantId")
            : base(RightType.TenantTodoCollection, permission, resourceKey)
        {
        }
    }

    public class AuthoriseUserAttribute : AuthoriseAttribute
    {
        public AuthoriseUserAttribute(Permission permission)
            : base(RightType.User, permission)
        {
        }
    }


    public class AuthoriseTagAttribute : AuthoriseAttribute
    {
        public AuthoriseTagAttribute(Permission permission, string resourceKey = ResourceKey.Id)
            : base(RightType.TodoTagCollection, permission, resourceKey)
        {
        }
    }

    public class AuthoriseTodoTagCollectionAttribute : AuthoriseAttribute
    {
        public AuthoriseTodoTagCollectionAttribute(Permission permission, string resourceKey = ResourceKey.Id)
            : base(RightType.TodoTagCollection, permission, resourceKey)
        {
        }
    }

    /// <summary>
    ///     Wrapper around Authorize so that we can audit forms that they just need authentication
    /// </summary>
    public class AuthoriseFormAttribute : AuthorizeAttribute
    {
    }

    /// <summary>
    ///     Wrapper around Authorize so that we can audit forms that they just need authentication
    /// </summary>
    public class AuthoriseRedirectAttribute : AuthorizeAttribute
    {
    }
}