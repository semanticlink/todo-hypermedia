using System.Threading.Tasks;
using Domain.Models;
using Domain.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Toolkit;

namespace App.Authorisation
{
    /// <summary>
    ///     Looks inside the scope of the JSON Web Token Authorization Header
    /// <example>
    ///
    ///     Payload (data) of the JWT:
    ///     
    ///     <code>
    ///     {
    ///         "iss": "https://rewire-sample.au.auth0.com/",
    ///         "sub": "auth0|5b32b696a8c12d3b9a32b138",
    ///         "aud": [
    ///             "todo-rest-test",
    ///             "https://rewire-sample.au.auth0.com/userinfo"
    ///         ],
    ///         "iat": 1530411996,
    ///         "exp": 1530419196,
    ///         "azp": "3CYUtb8Uf9NxwesvBJAs2gNjqYk3yfZ8",
    ///         "scope": "openid profile"
    ///     }
    ///     </code>
    /// </example>
    /// </summary>
    /// <remarks>
    ///    code from https://auth0.com/docs/quickstart/backend/aspnet-core-webapi/01-authorization#configure-the-sample-project
    /// </remarks>
    public class HasPermissionsHandler : AuthorizationHandler<HasPermissionsOnResourceRequirement>
    {
        private readonly IUserRightStore _userRightStore;
        private readonly UserResolverService _userResolverService;

        public HasPermissionsHandler(IUserRightStore userRightStore, UserResolverService userResolverService)
        {
            _userRightStore = userRightStore;
            _userResolverService = userResolverService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            HasPermissionsOnResourceRequirement requirement)
        {
            // pattern matching is cool. if you can't do this, use context.Resource as AuthorizationFilterContext before and check for not null
            // see https://stackoverflow.com/questions/48386853/asp-net-core-identity-authorization-using-parameter-for-team-membership
            if (context.Resource is AuthorizationFilterContext authContext)
            {
                // you can grab the id of the resource based on the keyname in the route, or if it is root use the known resource id
                var resourceId = requirement.ResourceKeyInUri != ResourceKey.Root
                    ? authContext.RouteData.Values[requirement.ResourceKeyInUri]?.ToString()
                    : TrustDefaults.KnownHomeResourceId;
                
                var user = await _userResolverService.GetUserAsync();

                // if there is no authenticated user return onto other handlers/requirements
                if (user.Id.IsNullOrWhitespace() || resourceId == null)
                {
                    return;
                }

                var rights = await _userRightStore.Get(user.Id, resourceId, requirement.Type);

                // does the user have the rights
                if (rights.IsNotNull() && rights.Allow(requirement.Access))
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}