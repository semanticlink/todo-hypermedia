﻿using System.Threading.Tasks;
using App;
using Domain.Models;
using Domain.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using NLog;
using Toolkit;

namespace Api.Authorisation
{
    /// <summary>
    ///     Permissions will get the internal id from the claims set via authentication and then look for the
    ///     <see cref="UserRight"/> based on the resource id (from the RouteData param specified in the
    ///     <see cref="AuthoriseAttribute"/> and matched against the <see cref="Permission"/> also see
    ///     on the <see cref="AuthoriseAttribute"/>.
    /// </summary>
    public class HasPermissionsHandler : AuthorizationHandler<HasPermissionsOnResourceRequirement>
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly IUserRightStore _userRightStore;

        public HasPermissionsHandler(IUserRightStore userRightStore)
        {
            _userRightStore = userRightStore;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            HasPermissionsOnResourceRequirement requirement)
        {
            // drop through the handler to continue through to others
            // on the way, if the user is found and has allowed access set the Succeeded
            // there is no explicit failure so that other handlers can also have their say
            if (context.Resource is AuthorizationFilterContext authContext)
            {
                // you can grab the id of the resource based on the keyname in the route, or if it
                // is root use the known resource id
                var resourceId = requirement.ResourceKeyInUri != ResourceKey.Root
                    ? authContext.RouteData.Values[requirement.ResourceKeyInUri]?.ToString()
                    : TrustDefaults.KnownHomeResourceId;

                // get the user Id from the claims that already setup
                var userId = authContext.HttpContext.User.GetIdentityId();

                // if there is no authenticated user return onto other handlers/requirements
                if (!userId.IsNullOrWhitespace() && resourceId != null)
                {
                    var rights = await _userRightStore.Get(userId, resourceId, requirement.Type);

                    // does the user have the access rights?
                    if (rights.IsNotNull() && rights.IsAllowed(requirement.Access))
                    {
                        // yup, set for later use in the pipeline
                        context.Succeed(requirement);
                    }
                    else
                    {
                        Log.Trace(
                            "User {0} does not have permission {1} on resource {2}",
                            userId,
                            requirement.Access,
                            resourceId);
                    }
                }
                else
                {
                    Log.DebugFormat("Requirement could not be matched to external user on resource '{0}'", resourceId);
                }
            }
            else
            {
                Log.Error("Authorisation could not proceed as the context could not be loaded");
            }
        }
    }
}