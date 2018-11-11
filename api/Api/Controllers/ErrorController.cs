using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Domain.Representation;
using SemanticLink;

namespace Api.Controllers
{
    [Route("error")]
    public class ErrorController : Controller
    {
        /// <summary>
        ///    Error page allowing for content negotiation 
        /// </summary>
        /// <remarks>
        ///    We need this if we are browsing the api through the browser and receieve a 401, for example. The html
        ///     version will return the rich html version that will allow the user to authenticate beyond 'Basic'.
        /// </remarks>
        /// <seealso cref="Api.Web.HtmlFormMediaFormatter"/>
        [HttpGet("{statusCode}")]
        public ErrorRepresentation Index(int statusCode)
        {
            var f = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            var uri = UriHelper.BuildAbsolute(Request.Scheme, Request.Host, f.OriginalPathBase, f.OriginalPath);
            return ErrorRepresentation.MakeSparse(uri);
        }
    }
}