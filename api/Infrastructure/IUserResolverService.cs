using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Models;

namespace Infrastructure
{
    public interface IUserResolverService
    {
        /// <summary>
        ///    Grabs the external <see cref="Claim"/> Id and resolves that to the internal <see cref="User"/>.
        /// </summary>
        Task<User> GetPrincipleUserAsync(ClaimsPrincipal user);

        /// <summary>
        ///    This looks inside the current <see cref="HttpContext.User"/> that is resolved as a <see cref="ClaimsPrincipal"/>
        ///    and grabs the external <see cref="Claim"/> Id and resolves that to the internal <see cref="User"/>.
        /// </summary>
        Task<User> GetUserAsync();

        /// <summary>
        ///    This looks inside the current <see cref="HttpContext.User"/> that is resolved as a <see cref="ClaimsPrincipal"/>
        ///    and grabs the external <see cref="Claim"/> Id and resolves that to the internal <see cref="User"/>.
        /// </summary>
        User GetUser();
    }
}