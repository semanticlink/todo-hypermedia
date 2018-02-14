using System;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Representation;
using TodoApi.RepresentationExtensions;
using TodoApi.UriFactory;

namespace TodoApi.Controllers
{
    [Route("/")]
    public class HomeController : Controller
    {
        private readonly Version _version;

        public HomeController(Version version)
        {
            _version = version;
        }

        [HttpGet("", Name = HomeUriFactory.SelfRouteName)]
        public ApiRepresentation GetApi()
        {
            var apiVersion = new ApiVersion
            {
                Version = _version.ToString()
            };

            return apiVersion
                .ToRepresentation(Url);
        }
    }
}