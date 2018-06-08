using System.Threading.Tasks;
using App.RepresentationExtensions;
using App.UriFactory;
using Domain.Persistence;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using Toolkit;

namespace Api.Controllers
{
    [Route("user")]
    public class AccountController : Controller
    {
        private readonly IUserStore _userRepository;

        public AccountController(
            IUserStore userRepository
        )
        {
            _userRepository = userRepository;
        }

        [HttpGet("{id}", Name = UserUriFactory.SelfRouteName)]
        public async Task<UserRepresentation> GetUsers(string id)
        {
            return (await _userRepository
                    .GetByIdentityId(id))
                .ThrowInvalidDataExceptionIfNull($"User '{id}' not found")
                .ToRepresentation(id, Url);
        }
    }
}