using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Cors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Api.Web
{
    /// <summary>
    ///     A CORS to the application.
    /// </summary>
    /// <remarks>
    ///     The API should allow any origin, whereas there is a case for only
    ///     allowing the web site access to the web sites internal API.
    /// </remarks>
    /// <see cref="https://docs.microsoft.com/en-us/aspnet/core/security/cors" />
    public static class CorsMiddlewareExtensions
    {
        public const string TodoCorsPolicyName = "TodoCors";

        /// <summary>
        ///     This registration needs to be added because the .AddMvcCore
        ///     doesn't add it by default. It works without it when using .AddMvc.
        /// </summary>
        /// <seealso cref="https://stackoverflow.com/questions/39265215/exception-when-trying-to-enable-cors-globally" />
        public static IServiceCollection AddTodoCors(this IServiceCollection services)
        {
            services.TryAddTransient<CorsAuthorizationFilter, CorsAuthorizationFilter>();

            return services.AddCors(options =>
            {
                options.AddPolicy(TodoCorsPolicyName,
                    builder =>
                    {
                        builder
                            .AllowAnyOrigin()

                            //
                            // Note: PUT and DELETE are not 'simple' methods 
                            .AllowAnyMethod()

                            //
                            .AllowCredentials()

                            //
                            .WithMethods("GET", "HEAD", "PUT", "POST", "DELETE", "PATCH", "OPTIONS")

                            //
                            .WithHeaders(
                                "Accept",
                                "X-Requested-With",
                                "Content-Type",
                                "X-Csrf-Token",
                                "Authorization",
                                "Cache-Control")

                            //
                            .WithExposedHeaders(
                                "Allow",
                                "Location",
                                "WWW-Authenticate",
                                "x-amzn-remapped-www-authenticate")

                            //
                            .SetPreflightMaxAge(TimeSpan.FromDays(1.0));


                        // at this point, we aren't going to configure up allowed origins to be specific. 
                        builder.AllowAnyOrigin();
                    });
            });
        }

        /// <summary>
        ///     Add a CORS policy - ensure this goes before app.UseMvc
        /// </summary>
        public static IApplicationBuilder UseTodoCors(this IApplicationBuilder app)
        {
            return app.UseCors(TodoCorsPolicyName);
        }
    }
}