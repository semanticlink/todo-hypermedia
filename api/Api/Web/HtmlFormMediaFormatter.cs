using System;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Toolkit;
using Toolkit.Representation.LinkedRepresentation;

namespace Api.Web
{
    public class HtmlFormMediaFormatter : TextOutputFormatter
    {
        // TODO: make CDN address injectable
        private const string ResourceHtml = @"<html>
    <head>
      <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8""/>
      <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
      <title>Resource</title>
      {0}
    </head>
    <body>
      <div id=""app"">Intialising ...</div>
      <script src=""http://localhost:8080/dist/api.js""></script>
    </body>
</html>";


        public HtmlFormMediaFormatter()
        {
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedMediaTypes.Add(MediaTypeNames.Text.Html);
        }

        /// <summary>
        ///     This allows rendering the representations in a browser (with javascript enabled). Because a browser
        ///     will allow HTML before JSON having all types available as HTML will
        ///     result in the other data representations trying to be rendered as HTML.
        /// </summary>
        protected override bool CanWriteType(Type type)
        {
            return typeof(LinkedRepresentation).IsAssignableFrom(type);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            IServiceProvider serviceProvider = context.HttpContext.RequestServices;
            var logger = serviceProvider.GetService(typeof(ILogger<UriListInputFormatter>)) as ILogger;

            return context.HttpContext.Response.WriteAsync(this.ToHtml(context, logger));
        }

        /// <summary>
        ///     Current implementation supports <see cref = "LinkedRepresentation" /> which is on just a wrapper
        ///     around a resource that when the client recieves it then comes back to the server for the JSON
        ///     represntation.
        /// </summary>
        private string ToHtml(OutputFormatterWriteContext context, ILogger logger)
        {
            if (context.Object is LinkedRepresentation)
            {
                var feed = context.Object as LinkedRepresentation;


                return string.Format(ResourceHtml, ToHtmlLinks(feed));
            }
            else
            {
                logger.LogError("Unsupported representation");
            }

            return string.Empty;
        }


        private string ToHtmlLinks(LinkedRepresentation r)
        {
            if (r != null && r.Links != null)
            {
                return r.Links.ToString(
                    "",
                    "",
                    "\n      ",
                    link => $@"<link rel=""{link.Rel}"" href=""{link.HRef}"" />");
            }

            return string.Empty;
        }
    }
}