using System.IO;
using Amazon.DynamoDBv2;
using Api.Web;
using App;
using Infrastructure.mySql;
using Infrastructure.NoSQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;

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
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            services
                .AddJwtTokenAuthentication(Configuration)
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
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                        Path.Combine(Directory.GetCurrentDirectory(), "../../client/dist")),
                    RequestPath = "/dist"
                });
            }
            else
            {
                app.UseExceptionHandler("/error");
                // TODO: app.UseStaticFiles();
            }

            app
                .UseTodoCors()
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

            TableNameConstants
                .Todo
                .CreateTable(client)
                .ConfigureAwait(false);

            TableNameConstants
                .Tenant
                .CreateTable(client)
                .ConfigureAwait(false);

            return app;
        }
    }
}