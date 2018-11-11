using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Domain.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure;
using SemanticLink.AspNetCore;
using Toolkit;

namespace Api.Web
{
    public static class IdentityExtensions
    {
        /// <summary>
        /// <para>
        ///    Add Identity for authenticn. Note as of Auth 2.0, all types needed are regsitered
        ///    under AddAuthentication.
        ///</para>
        /// <para>
        ///  At this stage:
        ///    - use JWT (bearer) tokens
        ///    Currently, it is authenticated or not (no roles or claims)
        /// </para>
        /// <remarks>
        ///    see https://github.com/aspnet/Security/issues/1310
        /// </remarks>
        /// </summary>
        public static IServiceCollection AddAuthenticationWithJwtToken(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var api = configuration.GetSection(ApiDomainSettings.SectionName).Get<ApiDomainSettings>();

            // Needed, well documented as a problem
            // see https://github.com/aspnet/Security/issues/1043
            // see https://dev.to/coolgoose/setting-up-jwt-and-identity-authorizationauthentication-in-asp-net-core-4l45
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims

            var auth0 = configuration.GetSection(Auth0Configuration.SectionName).Get<Auth0Configuration>();

            var domain = $"https://{auth0.Domain}/";

            services
                .AddAuthentication(
                    authenticationOptions =>
                    {
                        /**
                         * Authenticate using AuthenticationOptions.DefaultAuthenticateScheme to set httpContext.User
                         */
                        authenticationOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        ;
                        /**
                         * The default scheme to use with [Authorize] attributed methods.
                         *
                         * NOTE: if you can't get a Context.User, it is because you don't have an [Authorize] attribute
                         *       that loads up the User onto the context.
                         */
                        authenticationOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        /**
                         * The global overriding default when any of above and others are no set. Thereotically,
                         * this is not needed because the two above are set.
                         *
                         * Others that could be set see AuthenticationOptions
                         */
                        authenticationOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    }
                )
                .AddJwtBearer(AuthenticatorDefaults.ExternalAuthenticationSchemeName, options =>
                {
                    // see https://auth0.com/blog/securing-asp-dot-net-core-2-applications-with-jwts/
                    //
                    // As requested when creating it, our API will use RS256 as the algorithm for signing tokens.
                    // Since RS256 uses a private/public key pair, it verifies the tokens against the
                    // public key for our Auth0 account. The ASP.NET Core JWT middleware will handle downloading
                    // the JSON Web Key Set (JWKS) file containing the public key for us, and will use that
                    // to verify the access_token signature.
                    options.Authority = domain;
                    options.Audience = auth0.Audience;


                    // see https://www.jerriepelser.com/blog/accessing-tokens-aspnet-core-2/
                    //
                    // Set to true if you want to get the value of the access_token in one of your
                    // controller actions, you will need to retrieve the AuthenticateInfo. The access_token
                    // will be stored inside the Properties with the key “.Token.access_token”:
                    //
                    // eg for this scheme "JSONWebToken"
                    //
                    //    string accessToken = await HttpContext.GetTokenAsync("access_token");
                    //
/*
                    options.SaveToken = true;
*/

                    options.Challenge =
                        string.Format("{0} realm=\"{1}\" uri={2}",
                            AuthenticatorDefaults.ExternalAuthenticationSchemeName,
                            AuthenticatorDefaults.AuthenticatorAuth0Realm,
                            api.AuthorizationUri());

                    options.Events = new JwtBearerEvents
                    {
                        /**
                         * Read a custom authorization header for scheme name: JSONWebToken
                         *
                         * example:
                         *    Authorization: JSONWebToken eyJ0eXAiOiJKV1QiLCJhbG...
                         *
                         * Default implementation is to look for scheme: Bearer (see https://github.com/aspnet/Security/blob/master/src/Microsoft.AspNetCore.Authentication.JwtBearer/JwtBearerHandler.cs#L63)
                         *
                         * This uses the event OnMessageReceived to process the header and pass it into the pipeline.
                         *   see event trigger: https://github.com/aspnet/Security/blob/master/src/Microsoft.AspNetCore.Authentication.JwtBearer/JwtBearerHandler.cs#L54
                         *
                         *
                         * see https://andrewlock.net/a-look-behind-the-jwt-bearer-authentication-middleware-in-asp-net-core/#readauthorizationheader
                         * related see https://stackoverflow.com/questions/41955117/custom-token-location-for-jwtbearermiddleware
                         */
                        OnMessageReceived = context =>
                        {
                            if (string.IsNullOrEmpty(context.Token))
                            {
                                string authorization = context.Request.Headers["Authorization"];

                                // If no authorization header found, nothing to process further
                                if (string.IsNullOrEmpty(authorization))
                                {
                                    return Task.FromResult(AuthenticateResult.NoResult());
                                }

                                // note the space in the prefix
                                var schemePrefx = $"{AuthenticatorDefaults.ExternalAuthenticationSchemeName} ";
                                if (authorization.StartsWith(schemePrefx, StringComparison.OrdinalIgnoreCase))
                                {
                                    // this then gets picked up further in the pipeline to be processes
                                    context.Token = authorization.Substring(schemePrefx.Length).Trim();
                                }

                                // If no token found, no further work possible
                                if (string.IsNullOrEmpty(context.Token))
                                {
                                    return Task.FromResult(AuthenticateResult.NoResult());
                                }
                            }

                            return Task.CompletedTask;
                        },

                        /**
                         * Once a token has been validated (above) then add the user to the claims.
                         *
                         * The design is that matching the internal id requires a database request and will be
                         * required once per request. It is then cached and used through any actions.
                         *
                         * Checking Id:
                         *
                         *  User.Claims.FindFirst(c => c.Type == UserIdClaimKey).Value
                         */
                        OnTokenValidated = async context =>
                        {
                            // we need to hand in the context Principal at this point as it has not yet 
                            // been loaded on the IHttpContextAssessor required for the UserResolverService
                            var user = await context.HttpContext.RequestServices
                                .GetRequiredService<IUserResolverService>()
                                .GetPrincipleUserAsync(context.Principal);

                            if (!user.Id.IsNullOrWhitespace())
                            {
                                context.Principal.AddIdentityIdToClaims(user.Id);
                            }
                        }
                    };
                });

            return services;
        }
    }
}