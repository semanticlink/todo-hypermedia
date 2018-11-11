using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Toolkit;
using Microsoft.Extensions.Logging;
using SemanticLink;
using MediaType = SemanticLink.MediaType;

namespace Api.Web
{
    /// <summary>
    ///     Support http requests with the mime type 'text/uri-list'
    /// </summary>
    /// <seealso cref="https://tools.ietf.org/html/rfc2483#section-5" />
    public class UriListInputFormatter : IInputFormatter
    {
//        private static readonly ILog Log = LogManager.GetLogger(typeof(JpegFormatter));
        private readonly string UriListMediaType = MediaType.UriList;


        public bool CanRead(InputFormatterContext context)
        {
            return context.HttpContext.Request.ContentType == UriListMediaType;
        }


        public async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            IServiceProvider serviceProvider = context.HttpContext.RequestServices;
            var logger = serviceProvider.GetService(typeof(ILogger<UriListInputFormatter>)) as ILogger;

            using (var reader = new StreamReader(context.HttpContext.Request.Body))
            {
                // Read the request body as a string
                var stringContent = await reader
                    .ReadToEndAsync()
                    .ConfigureAwait(continueOnCapturedContext: false);

                // Split the string using the required '\r\n\', but fall back to '\n' only for
                // down-level implementations.
                var uris = stringContent
                    .Split(new[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries)
                    .Where(uri => !uri.IsNullOrWhitespace()) // ignore empty values
                    .Where(uri => !uri.StartsWith("#")); // the hash must be at the start of the line

                var modelType = context.ModelType;

                if (modelType == typeof(string[]))
                {
                    return await InputFormatterResult.SuccessAsync(uris.ToArray());
                }

                throw new NotSupportedException();
            }
        }
    }

    public class UriListOutputFormatter : IOutputFormatter
    {
        private readonly string UriListMediaType = MediaType.UriList;

        public bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            return context.HttpContext.Response.ContentType == UriListMediaType;
        }

        public Task WriteAsync(OutputFormatterWriteContext context)
        {
            IServiceProvider serviceProvider = context.HttpContext.RequestServices;
            var logger = serviceProvider.GetService(typeof(ILogger<UriListOutputFormatter>)) as ILogger;

            var response = context.HttpContext.Response;

            var buffer = new StringBuilder();
            if (context.Object is FeedRepresentation)
            {
                var feed = context.Object as FeedRepresentation;
                if (feed?.Items != null)
                {
                    feed?.Items?.ForEach(item => { buffer.AppendFormat($"{item.Id}\r\n"); });
                }
                else
                {
                    logger.LogInformation("Empty feed");
                }
            }
            else
            {
                logger.LogError("");
            }

            return response.WriteAsync(buffer.ToString());
        }
    }
}