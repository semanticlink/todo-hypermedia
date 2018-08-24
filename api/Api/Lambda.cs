using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

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
        /// <summary>
        /// The builder has configuration, logging and Amazon API Gateway already configured. The startup class
        /// needs to be configured in this method using the UseStartup() method.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Init(IWebHostBuilder builder)
        {
            builder
                .UseStartup<Startup>()
                .ConfigureLogging(logging =>
                {
                    // uses section 'Lambda.Logging' in appsettings.json
                    // this actually looks to be added by default
                    // logging.AddLambdaLogger();

                    // setup log levels for other
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                })
                // don't forget this little sucker, it will give you access to the
                // IServer through IoC for later in the app is needed
                .UseApiGateway();
        }
    }
}