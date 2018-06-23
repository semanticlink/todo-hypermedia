using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Amazon.DynamoDBv2;
using Api.Web;
using App;
using Infrastructure.mySql;
using Infrastructure.NoSQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Serialization;
using Toolkit;

namespace Api
{
    public class Startup
    {
        private IHostingEnvironment HostingEnvironment { get; }
        private IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment env, IConfiguration config)
        {
            HostingEnvironment = env;
            Configuration = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddDbContext<ApplicationIdentityDbContext>()
                .AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    // cross-reference to use the userId used in the JWT
                    // see https://github.com/aspnet/Security/issues/1043
                    options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
                })
                .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc(options =>
            {
                // see https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-2.1#browsers-and-content-negotiation
                options.RespectBrowserAcceptHeader = true; // false by default
            });

            services
                .AddAuthenticationWithJwtToken(Configuration)
                .AddMvcCore(options =>
                {
                    options.RespectBrowserAcceptHeader = true;
                    options.ReturnHttpNotAcceptable = true;

                    // map execeptions to http status codes
                    options.Filters.Add(typeof(ExceptionFilter));

                    // Important: InputFormatters are used only when [FromBody] is used 
                    // in the parameter's list of the action
                    options.InputFormatters.Add(new FormUrlEncodedMediaFormatter());

                    // Content-negotitation output types
                    options.OutputFormatters.Add(new HtmlFormMediaFormatter());

                    options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                })
                .AddJsonFormatters(s => s.ContractResolver = new DefaultContractResolver())
                .AddXmlDataContractSerializerFormatters();

            services
                .RegisterIoc(HostingEnvironment)
                .AddTodoCors();

            services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; });
        }

        public void Configure(
            IApplicationBuilder app,
            ILoggerFactory loggerFactory,
            ApplicationIdentityDbContext db)
        {
            /**
             * Note: this block MUST be before app.UseMvc();
             */
            loggerFactory.AddConsole();


            if (HostingEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }


            /**
             * Vary headers are needed so that the back and forward buttons work ie, that we don't
             * get cache poisioning—in this case the back button would return the json representation
             * than the html representation.
             *
             * see https://docs.microsoft.com/en-us/aspnet/core/performance/caching/middleware?view=aspnetcore-2.1
             */
            app.Use(async (context, next) =>
            {
                context.Response.Headers[HeaderNames.Vary] = new[] {"Accept", "Accept-Encoding"};

                await next();
            });

            /**
             * Handler for error pages to return content negotiated pages. For example, 401 can now
             * return an text/html page for authenticating.
             */
            app.UseStatusCodePagesWithReExecute("/error/{0}");
            
            app
                .UseTodoCors()
                
                /*
                 *
                 * Auth 2.0 now only has a single authenticatio middleware and invokes
                 *  based on registration in 'AddAuthentication' in ConfigureServices
                 *
                 * see https://github.com/aspnet/Security/issues/1310
                 */
                .UseAuthentication()
                
                
                .UseMvc()
                // requires a dynamoDb instance - see readme for setup in docker
                .MigrateDynamoDb()
                // requires a mysql instance - see readme for setup in docker
                .MigrateIdentityDb(db);
        }
    }

    public static class MigrateDynamoDbExtensions
    {
        public static IApplicationBuilder MigrateDynamoDb(this IApplicationBuilder app)
        {
            var client = app.ApplicationServices.GetService<IAmazonDynamoDB>();

            new[]
                {
                    TableNameConstants.Todo,
                    TableNameConstants.Tenant,
                    TableNameConstants.Tag,
                }
                .ForEach(table => table.CreateTable(client).ConfigureAwait(false));


            return app;
        }
    }
}