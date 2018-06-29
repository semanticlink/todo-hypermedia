using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Infrastructure.mySql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Api.Web
{
    public static class IdentityExtensions
    {
        public const string Auth0AuthenticationSchemeName = "Auth0";
        
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

            services
                .AddAuthentication(
                    AuthenticationOptions =>
                    {
                        /**
                         * Authenticate using AuthenticationOptions.DefaultAuthenticateScheme to set httpContext.User
                         */
                        AuthenticationOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

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
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.IncludeErrorDetails = true;
                    // TODO: inject api hosting address
                    options.Challenge =
                        $"{JwtBearerDefaults.AuthenticationScheme} realm=\"auth0\", rel=authenticate, uri=http://localhost:5000/";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = configuration["JwtIssuer"],
                        ValidAudience = configuration["JwtIssuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtKey"])),
                        ClockSkew = TimeSpan.Zero // remove delay of token when expire
                    };
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.IncludeErrorDetails = true;
                    // TODO: inject api hosting address
                    options.Challenge =
                        $"{JwtBearerDefaults.AuthenticationScheme} realm=\"api\", rel=authenticate, uri=http://localhost:5000/";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = configuration["JwtIssuer"],
                        ValidAudience = configuration["JwtIssuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtKey"])),
                        ClockSkew = TimeSpan.Zero // remove delay of token when expire
                    };
                })
                ;

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

        public static string GenerateJwtToken(this IConfiguration configuration,
            string userId,
            string email,
            IdentityUser user)
        {
            var claims = new List<Claim>
            {
                /**
                 * Unique user identifier
                 *
                 * see https://tools.ietf.org/html/rfc7519#section-4.1.2
                 */
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                /**
                 * 
                 *
                 * see http://tools.ietf.org/html/rfc7519#section-4
                 */
                new Claim(JwtRegisteredClaimNames.Email, email),
                /**
                 * Unique identifier for the JWT. This is handed through from the signin service
                 *
                 * see https://tools.ietf.org/html/rfc7519#section-4.1.7
                 *
                 * NOTE: currently there is no need to include this
                 */
                new Claim(JwtRegisteredClaimNames.Jti, user.Id),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(configuration["JwtExpireDays"]));

            var token = new JwtSecurityToken(
                configuration["JwtIssuer"],
                configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}