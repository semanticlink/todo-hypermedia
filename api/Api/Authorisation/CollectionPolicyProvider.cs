using System.Threading.Tasks;
using App;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Api.Authorisation
{
    /// <summary>
    /// <para>
    ///     A policy creator that binds an <see cref="AuthoriseAttribute"/> through to requirements. The practical reason for using
    ///     this mechanism is that policy are based are resource <see cref="RightType"/> and that we want to be able
    ///     specify <see cref="Permission"/> in the attributed method.
    ///</para>
    /// <para>
    ///    The policy also ensures:
    /// <list type="bullet">
    /// <item>user is authenticated</item>
    /// <item>user is using our 'jwt' <see cref="AuthenticatorDefaults.ExternalAuthenticationSchemeName"/></item>
    /// </list>
    /// </para>
    /// </summary>
    /// <remarks>
    /// see https://github.com/aspnet/AuthSamples/blob/master/samples/CustomPolicyProvider/Authorization/MinimumAgePolicyProvider.cs
    /// </remarks>
    /// <example>
    ///     AuthorizeRootUserCollection(Permission.Get)] -- RootUserCollection rights requiring Get permissions
    ///     AuthorizeRootUserCollection(Permission.Get, "id")] -- Same as above + looks for the resource using "id" param from the query
    /// </example>
    /// <seealso cref="AuthoriseAttribute"/>
    public class CollectionPolicyProvider : IAuthorizationPolicyProvider
    {
        public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

        public CollectionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            // ASP.NET Core only uses one authorization policy provider, so if the custom implementation
            // doesn't handle all policies (including default policies, etc.) it should fall back to an
            // alternate provider.
            //
            // In this sample, a default authorization policy provider (constructed with options from the 
            // dependency injection container) is used if this custom provider isn't able to handle a given
            // policy name.
            //
            // If a custom policy provider is able to handle all expected policy names then, of course, this
            // fallback pattern is unnecessary.
            FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();


        /// <summary>
        ///     Policies are looked up by string name, so expect 'parameters' to be encoded (embedded) in the policy names (errr-don't blame me).
        /// </summary>
        /// <param name="encodedPolicyName">A colon ':' delimited string of three values [<see cref="RightType>"/>:<see cref="Permission"/>:<see cref="string"/>]</param>
        /// <remarks>
        ///    For Encoding and Decoding see <see cref="PolicyName"/>
        /// </remarks>
        public Task<AuthorizationPolicy> GetPolicyAsync(string encodedPolicyName)
        {
            // here's the other end of the magic to decode the string with the requirement details
            var requirementDetails = PolicyName.Deserialise(encodedPolicyName);

            // If the policy name doesn't match the format expected by this policy provider,
            // try the fallback provider. If no fallback provider is used, this would return 
            // Task.FromResult<AuthorizationPolicy>(null) instead.
            if (requirementDetails == null)
            {
                return FallbackPolicyProvider.GetPolicyAsync(encodedPolicyName);
            }

            var policy = new AuthorizationPolicyBuilder();

            policy.RequireAuthenticatedUser();

            // Set multiple bearer tokens. This pairs with .AddAuthententication to expose
            // multiple www-authenticate headers on a 401
            // 
            // see https://stackoverflow.com/questions/49694383/use-multiple-jwt-bearer-authentication
            // 
            policy.AuthenticationSchemes.Add(AuthenticatorDefaults.ExternalAuthenticationSchemeName);

            // now we can hand in the requirements from the attribute into the policy which what we really want to do
            policy.AddRequirements(
                new HasPermissionsOnResourceRequirement(requirementDetails.Type, requirementDetails.Access,
                    requirementDetails.ResourceKey));

            return Task.FromResult(policy.Build());
        }
    }
}