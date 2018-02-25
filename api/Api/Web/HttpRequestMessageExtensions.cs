﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Web
{
    /// <summary>
    ///     Extension helpers for a application to return simple result codes
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        public static CreatedResult MakeCreated(
            this string uri,
            HttpRequest request,
            string status = "")
        {
            return new CreatedResult(
                uri,
                new
                {
                    message = "The resource has been created",
                    status,
                    id = uri
                });
        }

        public static CreatedResult MakeCreated(this string uri)
        {
            return new CreatedResult(
                uri,
                new
                {
                    message = "The resource has been created",
                    id = uri
                });
        }

        public static async Task<NoContentResult> MakeNoContent(this Task task)
        {
            return new NoContentResult();
        }

        public static RedirectResult MakeRedirect(this string uri)
        {
            return new RedirectResult(uri, permanent: false /* not permanent */);
        }
    }
}