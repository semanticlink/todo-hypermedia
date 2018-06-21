using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using App.RepresentationExtensions;
using App.UriFactory;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using Toolkit;
using Toolkit.Representation.LinkedRepresentation;

namespace Api.Controllers
{
    [Route("user")]
    public class UserController : Controller
    {
        private readonly IUserStore _userRepository;
        private readonly User _user;

        public UserController(
            IUserStore userRepository,
            User user
        )
        {
            _userRepository = userRepository;
            _user = user;
        }

        [HttpGet("", Name = UserUriFactory.SelfRouteName)]
        public FeedRepresentation Index()
        {
            _user.ThrowObjectNotFoundExceptionIfNull("No users can be found");

            // JWT mapping is broken
           //var userId = User.FindFirst(JwtRegisteredClaimNames.Sub).Value;

            return new List<User>()
                .ToFeedRepresentation(Url);
        }

        [HttpGet("{id}", Name = UserUriFactory.UserRouteName)]
        public async Task<UserRepresentation> Get(string id)
        {
            return (await _userRepository
                    .GetByIdentityId(id))
                .ThrowInvalidDataExceptionIfNull($"User '{id}' not found")
                .ToRepresentation(id, Url);
        }
    }
}