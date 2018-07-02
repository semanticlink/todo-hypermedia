using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;
using Infrastructure.mySql;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Web
{
    public static class IdentityExtensions
    {
        /// <summary>
        ///     Authentication sscheme for against external integrations such as Auth0 that return a JSON Web Token (JWT).
        ///     see https://tools.ietf.org/html/rfc7519
        /// </summary>
        /// <remarks>
        ///    We are not calling JWT across the wire as to avoid confusion with Java Web Tokens
        /// </remarks>
        public const string Auth0AuthenticationSchemeName = "JSONWebToken";

        /// <summary>
        /// <para>
        ///    Add Identity for authenticn. Note as of Auth 2.0, all types needed are regsitered
        ///    under AddAuthentication.
        ///</para>
        /// <para>
        ///  At this stage:
        ///    - persisted in MySql (via entity framework)
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
            // Needed, well documented as a problem
            // see https://github.com/aspnet/Security/issues/1043
            // see https://dev.to/coolgoose/setting-up-jwt-and-identity-authorizationauthentication-in-asp-net-core-4l45
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims


            var auth0 = configuration.GetSection(Auth0Configuration.SectionName).Get<Auth0Configuration>();

            var domain = $"https://{auth0.Domain}/";

            services
                .AddAuthentication(
                    AuthenticationOptions =>
                    {
                        /**
                         * Authenticate using AuthenticationOptions.DefaultAuthenticateScheme to set httpContext.User
                         */
                        AuthenticationOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        ;
                        /**
                         * The default scheme to use with [Authorize] attributed methods.
                         *
                         * NOTE: if you can't get a Context.User, it is because you don't have an [Authorize] attribute
                         *       that loads up the User onto the context.
                         */
                        AuthenticationOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        /**
                         * The global overriding default when any of above and others are no set. Thereotically,
                         * this is not needed because the two above are set.
                         *
                         * Others that could be set see AuthenticationOptions
                         */
                        AuthenticationOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    }
                )
                .AddJwtBearer(Auth0AuthenticationSchemeName, options =>
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

                    // TODO: inject api hosting address and uri construction
                    options.Challenge =
                        $"{Auth0AuthenticationSchemeName} realm=\"api\", uri=http://localhost:5000/authenticate";

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
                                var schemePrefx = $"{Auth0AuthenticationSchemeName} ";
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
                        }
                    };
                });
/*

            // see https://auth0.com/docs/quickstart/backend/aspnet-core-webapi/01-authorization#configure-the-sample-project
            // allows profiles on [Authorize]
            services.AddAuthorization(options =>
            {
                // eg [Authorize("profile")]
                options.AddPolicy("profile", policy =>
                    policy.Requirements.Add(new HasScopeRequirement("profile", domain)));
            });

            // register the scope authorization handler
            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
*/

            return services;
        }

        /// <summary>
        ///     A wrapper around ensuring the identity (mysql/entity) is upgraded correctly
        /// </summary>
        public static IApplicationBuilder MigrateIdentityDb(
            this IApplicationBuilder app,
            ApplicationIdentityDbContext db)
        {
            db.Database.EnsureCreated();
            return app;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///    code from https://auth0.com/docs/quickstart/backend/aspnet-core-webapi/01-authorization#configure-the-sample-project
    /// </remarks>
    public class HasScopeRequirement : IAuthorizationRequirement
    {
        public string Issuer { get; }
        public string Scope { get; }

        public HasScopeRequirement(string scope, string issuer)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        }
    }

    /// <summary>    ///     Looks inside the scope of the JSON Web Token Authorization Header
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
    ///         "scope": "openid profile read:todo"
    ///     }
    ///     </code>
    /// </example>
    /// </summary>
    /// <remarks>
    ///    code from https://auth0.com/docs/quickstart/backend/aspnet-core-webapi/01-authorization#configure-the-sample-project
    /// </remarks>
    public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
    {
        private const string Scope = "scope";

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            HasScopeRequirement requirement)
        {
            // If user does not have the scope claim, get out of here
            if (!context.User.HasClaim(c => c.Type == Scope && c.Issuer == requirement.Issuer))
            {
                return Task.CompletedTask;
            }

            // Split the scopes string into an array
            var scopes = context.User
                .FindFirst(c => c.Type == Scope && c.Issuer == requirement.Issuer)
                .Value
                .Split(' ');

            // Succeed if the scope array contains the required scope
            if (scopes.Any(s => s == requirement.Scope))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}