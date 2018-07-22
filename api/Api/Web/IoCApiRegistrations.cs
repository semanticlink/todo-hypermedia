using System.Reflection;
using Api.Authorisation;
using App;
using Domain.Models;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Web
{
    public static class IoCApiRegistrations
    {
        /// <summary>
        ///     These services should only be registered in the context of a server because they require the
        ///     <see cref="HttpContext"/>.
        /// </summary>
        /// <remarks>
        ///    Test libraries will need to create each of these registrations for themselves.
        /// </remarks>
        public static IServiceCollection RegisterApiIoC(this IServiceCollection services)
        {
            /**
             * Register up the user off the context of the Bearer --> Identity (external) --> User (internal)
             */
            // see https://github.com/aspnet/Hosting/issues/793 (TODO: clarify)
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IUserResolverService, UserResolverService>();
            services.AddScoped<User>(context => context.GetService<IUserResolverService>().GetUser());

            // Version from the assmembly (displayed on home resource)
            services.AddSingleton(Assembly.GetEntryAssembly().GetName().Version);
            

            ////////////////////////////////
            // Authorisation
            //

            // Replace the default authorization policy provider with our own
            // custom provider which can return authorization policies for given
            // policy names (instead of using the default policy provider)
            services.AddSingleton<IAuthorizationPolicyProvider, CollectionPolicyProvider>();

            // Register the authorisation handlers across resource types
            services.AddSingleton<IAuthorizationHandler, HasPermissionsHandler>();


            return services;
        }
    }
}