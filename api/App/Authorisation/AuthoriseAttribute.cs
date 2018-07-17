using Domain.Models;
using Microsoft.AspNetCore.Authorization;

namespace App.Authorisation
{
    /// <summary>
    ///     Base class that enforces the policy name (a serialised string) as required by <see cref="IAuthorizationPolicyProvider"/>
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
    public abstract class AuthoriseAttribute : AuthorizeAttribute
    {
        protected AuthoriseAttribute(RightType type, Permission permission = Permission.None, string resourceKey = "id")
        {
            // here's the magic of a delimited string
            Policy = PolicyName.Make(type, permission, resourceKey).Serialise();
        }
    }

    public class AuthorizeRootUserCollectionAttribute : AuthoriseAttribute
    {
        public AuthorizeRootUserCollectionAttribute(Permission permission)
            : base(RightType.RootUserCollection, permission, ResourceKey.Root)
        {
        }
    }
}