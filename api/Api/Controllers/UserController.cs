using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Web;
using App.RepresentationExtensions;
using App.UriFactory;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Toolkit;
using Toolkit.Representation.LinkedRepresentation;

namespace Api.Controllers
{
    [Route("user")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserStore _userStore;

        public UserController(IUserStore userStore)
        {
            _userStore = userStore;
        }

        [HttpGet("", Name = UserUriFactory.SelfRouteName)]
        public FeedRepresentation Index()
        {
            return new List<User>
                {
                    User.ToUser()
                }
                .ToFeedRepresentation(Url);
        }

        [HttpGet("{id}", Name = UserUriFactory.UserRouteName)]
        public async Task<UserRepresentation> Get(string id)
        {
            return (await _userStore
                    .Get(id))
                .ThrowInvalidDataExceptionIfNull($"User '{id}' not found")
                .ToRepresentation(id, Url);
        }
    }
}