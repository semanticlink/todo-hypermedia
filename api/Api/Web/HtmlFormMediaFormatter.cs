using System;
using System.Collections.Generic;
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
        private ILogger Log { get; }

        private const string ResourceHtml = @"<html>
    <head>
      <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8""/>
      <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
      <title>Resource</title>
      {0}
    </head>
    <body>
      <div id=""app"">Intialising ...</div>
      {1}
    </body>
</html>";

        private readonly List<string> scripts = new List<string>();

        public HtmlFormMediaFormatter(ApiClientSettings configuration, ILogger log)
        {
            Log = log;
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedMediaTypes.Add(MediaTypeNames.Text.Html);

            scripts.AddRange(configuration.ToScriptsHref());
            Log.DebugFormat("Script loaded for html representation: {0}", scripts.ToCsvString(script => script));
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
            return context.HttpContext.Response.WriteAsync(this.ToHtml(context));
        }

        /// <summary>
        ///     Current implementation supports <see cref = "LinkedRepresentation" /> which is on just a wrapper
        ///     around a resource that when the client recieves it then comes back to the server for the JSON
        ///     represntation.
        /// </summary>
        private string ToHtml(OutputFormatterWriteContext context)
        {
            if (context.Object is LinkedRepresentation)
            {
                var feed = context.Object as LinkedRepresentation;


                return string.Format(ResourceHtml, ToHtmlLinks(feed), ToScripts(scripts));
            }
            else
            {
                Log.Error("Unsupported representation");
            }

            return string.Empty;
        }

        private string ToScripts(List<string> scripts)
        {
            if (scripts != null)
            {
                return scripts.ToString(
                    "",
                    "",
                    "\n      ",
                    script => $@"<script src=""{script}""></script> ");
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