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
        public AuthoriseAttribute(RightType type, Permission permission = Permission.None, string resourceKey = "id")
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

    public class AuthoriseTodoAttribute : AuthoriseAttribute
    {
        public AuthoriseTodoAttribute(Permission permission)
            : base(RightType.Todo, permission)
        {
        }
    }

    public class AuthoriseUserTodoCollectionAttribute : AuthoriseAttribute
    {
        public AuthoriseUserTodoCollectionAttribute(Permission permission)
            : base(RightType.UserTodoCollection, permission)
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