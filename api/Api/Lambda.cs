using System;
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