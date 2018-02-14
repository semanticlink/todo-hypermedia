using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using TodoApi.Db;
using TodoApi.Web;

namespace TodoApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TodoContext>(opt => opt.UseInMemoryDatabase("TodoList"));
            services.AddMvcCore(options =>
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

            services.AddScoped<ITodoRepository, TodoRepository>();
            services.AddSingleton(Assembly.GetEntryAssembly().GetName().Version);

            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            /**
             * Note: this block MUST be before app.UseMvc();
             */
            loggerFactory.AddConsole();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseMvc();
        }
    }
}