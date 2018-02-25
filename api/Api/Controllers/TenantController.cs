using System.Threading.Tasks;
using App.RepresentationExtensions;
using App.UriFactory;
using Domain.Persistence;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("tenant/")]
    public class TenantController : Controller
    {
        private readonly ITenantStore _tenantRepository;

        public TenantController(ITenantStore tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        [HttpGet("{id}", Name = TenantUriFactory.SelfRouteName)]
        public async Task<TenantRepresentation> Get(string id)
        {
            return (await _tenantRepository
                    .Get(id))
                .ToRepresentation(Url);
        }
    }
}