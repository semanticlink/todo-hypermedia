using Domain.Models;
using Microsoft.AspNetCore.Authorization;

namespace App.Authorisation
{
    /// <summary>
    ///     This requirement checks if the user with claim issued by your Auth0 tenant is present and if they have
    ///     access to that perms for that resource
    /// </summary>
    /// <remarks>
    ///    code from https://auth0.com/docs/quickstart/backend/aspnet-core-webapi/01-authorization#configure-the-sample-project
    /// </remarks>
    public class HasPermissionsOnResourceRequirement : IAuthorizationRequirement
    {
        public RightType Type { get; }
        public string ResourceKeyInUri { get; }
        public Permission Access { get; }

        public HasPermissionsOnResourceRequirement(
            RightType rightType,
            Permission access = Permission.None,
            string resourceKeyInUri = "id")
        {
            Type = rightType;
            ResourceKeyInUri = resourceKeyInUri;
            Access = access;
        }
    }
}