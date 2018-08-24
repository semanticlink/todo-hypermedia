using System;
using AWS.Logger.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using Toolkit;

namespace Api

{
    /// <summary>
    /// This class extends from APIGatewayProxyFunction which contains the method FunctionHandlerAsync which is the 
    /// actual Lambda function entry point. The Lambda handler field should be set to
    /// 
    /// TodoApi::TodoApi.LambdaEntryPoint::FunctionHandlerAsync
    /// </summary>
    /// <remarks>
    ///
    /// see https://www.jerriepelser.com/blog/aspnet-core-aws-lambda-serverless-application/
    ///
    /// **DEBUGGING**
    ///
    ///     To help debug this can you add the environment variable LAMBDA_NET_SERIALIZER_DEBUG with a
    ///     value of true to the Lambda function. This will cause the raw JSON coming into the
    ///     Lambda function be written into CloudWatch logs for the Lambda function. Once you post
    ///     the RAW JSON I'll try and debug what is going on.
    /// 
    ///     ref: https://github.com/aws/aws-lambda-dotnet/issues/168#issuecomment-332897736
    /// 
    /// </remarks>
    public class LambdaEntryPoint : Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    {
        private static readonly NLog.ILogger Log = NLogBuilder
            .ConfigureNLog("Config/NLog.aws.config")
            .GetCurrentClassLogger();

        /// <summary>
        /// The builder has configuration, logging and Amazon API Gateway already configured. The startup class
        /// needs to be configured in this method using the UseStartup() method.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Init(IWebHostBuilder builder)
        {
            try
            {
                Log.Debug("[Init] starting");
                builder
                    .UseStartup<Startup>()
                    .ConfigureLogging(logging =>
                    {
                        // uses section 'Lambda.Logging' in appsettings.json
                        // this actually looks to be added by default
                        // logging.AddLambdaLogger();

                        // remove all other loggers. This means that we get all applicaiton
                        // logging going through NLog. In practice, it means that this app uses
                        // NLog and all others that don't also go to the same logging 'files'
                        //logging.ClearProviders();
                        // setup log levels for other
                        logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);

                        //logging.AddProvider(new AWSLoggerProvider());
                        // In startup we will set the aspnetcore ILogger to NLog implementation
                        // see Startup.Configure
                    })
                    // TODO: can we set it here?
                    .UseNLog()
                    // don't forget this little sucker, it will give you access to the
                    // IServer through IoC for later in the app is needed
                    .UseApiGateway();
            }
            catch (Exception ex)
            {
                Log.ErrorExceptionFormat(ex, "[Init] stopped program because of exception");
                throw;
            }
            finally
            {
                Log.Debug("[Init] shutting down");
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }
    }
}