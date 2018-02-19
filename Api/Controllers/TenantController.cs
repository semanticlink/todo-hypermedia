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
        private readonly ITenantRepository _tenantRepository;

        public TenantController(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        [HttpGet("{id:long}", Name = TenantUriFactory.SelfRouteName)]
        public TenantRepresentation Get(long id)
        {
            return _tenantRepository
                .Get(id)
                .ToRepresentation(Url);
        }
    }
}