﻿using System;
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
        /// <summary>
        ///    Add Identity for authenticn. At this stage:
        ///    - persisted in MySql (via entity framework)
        ///    - use JWT (bearer) tokens
        ///    Currently, it is authenticated or not (no roles or claims)
        /// 
        /// </summary>
        public static IServiceCollection AddJwtTokenAuthentication(
            this IServiceCollection services,
            IConfiguration Configuration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims
            services
                .AddAuthentication(
                    sharedOptions =>
                    {
                        sharedOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        sharedOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                        sharedOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    }
                )
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.IncludeErrorDetails = true;
                    options.Challenge = $"{JwtBearerDefaults.AuthenticationScheme} realm=\"api\", rel=\"authenticate\"";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = Configuration["JwtIssuer"],
                        ValidAudience = Configuration["JwtIssuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtKey"])),
                        ClockSkew = TimeSpan.Zero // remove delay of token when expire
                    };
                });
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

        public static string GenerateJwtToken(
            this IConfiguration configuration,
            string email,
            IdentityUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
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