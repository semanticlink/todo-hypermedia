﻿using Api.Web;
using Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Morcatko.AspNetCore.JsonMergePatch;
using SemanticLink.AspNetCore;
using Toolkit;
using AuthenticatorDefaults = SemanticLink.AspNetCore.AuthenticatorDefaults;

namespace Api
{
    public class Startup
    {
        private IHostingEnvironment HostingEnvironment { get; }
        private IConfiguration Configuration { get; }
        private ILogger<Startup> Log { get; }

        public Startup(IHostingEnvironment env, IConfiguration config, ILogger<Startup> log)
        {
            HostingEnvironment = env;
            Configuration = config;
            Log = log;
        }

        public void ConfigureServices(IServiceCollection services)
        {
      
            Log.Debug("[Init] Configure services");

            services
                // see https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-2.1#browsers-and-content-negotiation
                .AddMvc(options => options.RespectBrowserAcceptHeader = true) // default: false
                /**
                 * Add HTTP Patch support for JSON Merge Patch (performs partial resource updates but not JSON Patch that is part of AspNetCore)
                 * mime type: application/merge-patch+json
                 * see https://tools.ietf.org/html/rfc7396
                 */
                .AddJsonMergePatch();

            services
                .AddAuthenticationWithJwtToken(Configuration)
                .AddAuthorization(options =>
                {
                    // this is required for [Authorize] and [AllowAnonymous]. If we only used our custom policy
                    // attributes we wouldn't
                    options.DefaultPolicy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        // add our scheme here (instead of JwtBearerDefaults.AuthenticationScheme)
                        .AddAuthenticationSchemes(AuthenticatorDefaults.ExternalAuthenticationSchemeName)
                        .Build();
                    /**
                     * DO NOT set policies here. See IocRegistration
                     *
                     * This is addeded via Custom Policy Authorization Policies. This avoid loading policies/requirements manually and can
                     * be attributed a little more readably and auditably.
                     * 
                     * see https://docs.microsoft.com/en-us/aspnet/core/security/authorization/iauthorizationpolicyprovider?view=aspnetcore-2.1
                     *
                     * Note: custom policies override the default policy
                     * 
                     * Below is what you might tempted to do (but instead look in App.Authorization):
    
                    options.AddPolicy(
                        PolicyName.RootUserCollection,
                        policy =>
                        {
                            policy.Requirements.Add(new HasPermissionsOnResourceRequirement(
                                RightType.RootUserCollection, Permission.FullControl));
                            policy.RequireAuthenticatedUser();
                            policy.AuthenticationSchemes.Add(AuthenticatorDefaults.ExternalAuthenticationSchemeName);
                        });
                    */
                });

            services.AddMvcCore(options =>
                {
                    options.RespectBrowserAcceptHeader = true;
                    options.ReturnHttpNotAcceptable = true;

                    // map execeptions to http status codes
                    options.Filters.Add(typeof(ExceptionFilter));

                    // Important: InputFormatters are used only when [FromBody] is used 
                    // in the parameter's list of the action
                    options.InputFormatters.Add(new FormUrlEncodedMediaFormatter());
                    options.InputFormatters.Add(new UriListInputFormatter());


                    // Content-negotitation output types
                    options.OutputFormatters.Add(new HtmlFormMediaFormatter(
                        Configuration.GetSection(ApiClientSettings.SectionName).Get<ApiClientSettings>(),
                        Log));
/*
                    // add in when support is required
                    options.OutputFormatters.Add(new UriListOutputFormatter());
*/

                    options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                })
                .AddJsonFormatters(s => s.ContractResolver = new DefaultContractResolver())
                .AddXmlDataContractSerializerFormatters();

            services
                .RegisterAppIoc(HostingEnvironment)
                .RegisterApiIoC()
                /**
                 *  Add http cache headers. 
                 *
                 *  see https://github.com/KevinDockx/HttpCacheHeaders
                 * 
                 *  Based on this library:
                 *
                 *  - there is no cache profiles
                 *  - see each cache has to be wired up on attributes across: HttpCacheExpiration & HttpCacheValidation
                 *  - etags are set via its own hash (want to override this in the future because we can get this from the database)
                 *
                 * This library sets headers:
                 *
                 *     - Cache-Control ([private|public],[no-cache|no-store],max-age,must-revalidate (see https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.9)
                 *     - ETag
                 *     - Expires
                 *     - Last-Modified
                 *     - Vary
                 * 
                 *  see for caching information https://developers.google.com/web/fundamentals/performance/optimizing-content-efficiency/http-caching
                 * 
                 *  So in the Controllers, currently the main stratgies:
                 * 
                 *  - Collections: no caching
                 *  - Items: low caching
                 *  - Readonly Collections/Items: cacheable
                 *  - Virtual Resources: no caching
                 *
                 * Here are some base strategies that will need to be altered depending on the nature of the resource:
                 *
                 *  Collections
                 *         Cache-Control: private,max-age=60
                 *
                 *         [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
                 *         [HttpCacheValidation(NoCache = true)]
                 * 
                 *  Items
                 *         Cache-Control: private,max-age=60
                 *
                 *         [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
                 *         [HttpCacheValidation(NoCache = true)]
                 * 
                 *  Readonly
                 *         Cache-Control: public,max-age=3600
                 *
                 *         [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDurection.Long)]
                 *         [HttpCacheValidation]
                 * 
                 *  Virtual Resources
                 *         Cache-Control: private,max-age=60
                 *
                 *         [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
                 *         [HttpCacheValidation(NoCache = true)]
                 *
                 * WARNING: class attributes ARE NOT OVERRIDDEN by method attributes
                 *         Therefore, add attributes per method rather than per class
                 * 
                 */
                .AddHttpCacheHeaders()
                .AddTodoCors();

            services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; });
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {

            Log.Debug("[Init] Configure");

            if (HostingEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            /**
             * Handler for error pages to return content negotiated pages. For example, 401 can now
             * return an text/html page for authenticating.
             */
            app.UseStatusCodePagesWithReExecute("/error/{0}");

            app
                .UseTodoCors()
                /*
                 *
                 * Auth 2.0 now only has a single authentication middleware and invokes
                 *  based on registration in 'AddAuthentication' in ConfigureServices
                 *
                 * see https://github.com/aspnet/Security/issues/1310
                 */
                .UseAuthentication()

                // paried with .AddHttpCacheHeaders middleware to the request pipeline
                .UseHttpCacheHeaders()
                .UseMvc()

                // requires a dynamoDb instance - see readme for setup in docker
                .MigrateDynamoDb(Log)
                .DynamoDbSeedTestData(HostingEnvironment, Log);
        }
    }
}