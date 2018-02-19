using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using NLog.Fluent;
using Toolkit;

namespace Api.Web
{
    /// <summary>
    ///     <para>
    ///         Provide support for http status results along with a status message
    ///         (aka a reason phrase).
    ///     </para>
    ///     <para>
    ///         Note: that http/2 does not support a status code description (reason phrase).
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The standard <see cref = "StatusCodeResult" /> class doesn't allow the
    ///         status message to be set. Semantically for REST APIs it makes sense to
    ///         provide a short message along with failure codes for the client. It is
    ///         better not to use the body has this would introduce a new semantic into
    ///         the http request.
    ///     </para>
    /// </remarks>
    /// <seealso cref = "StatusCodeResult" />
    /// <seealso cref = "https://stackoverflow.com/questions/40040794/how-to-set-statusdescription-from-kestrel-server" />
    public class StatusCodeAndReasonResult : ActionResult
    {
        public StatusCodeAndReasonResult(int statusCode, string reasonPhrase)
        {
            this.ReasonPhrase = reasonPhrase;
            this.StatusCode = statusCode;
        }

        public int StatusCode { get; }
        public string ReasonPhrase { get; }

        public override void ExecuteResult(ActionContext context)
        {
            var httpResponse = context
                .HttpContext
                .Features
                .Get<IHttpResponseFeature>();
            if (!httpResponse.HasStarted)
            {
                if (!this.ReasonPhrase.IsNullOrWhitespace())
                {
                    httpResponse.ReasonPhrase = this.ReasonPhrase;
                }

                httpResponse.StatusCode = this.StatusCode;
                context.HttpContext.Response.StatusCode = this.StatusCode;
            }
            else
            {
                Log.Warn("Failed to set http response status as the response has already started");
            }
        }
    }
}