using App.UriFactory;
using Domain.LinkRelations;
using Domain.Models;
using Domain.Representation;
using Toolkit.LinkRelations;
using Toolkit.Representation.LinkedRepresentation;

namespace App.RepresentationExtensions
{
    public static class ApiRepresentationExtensions
    {
        public static ApiRepresentation ToRepresentation(this ApiVersion api, Microsoft.AspNetCore.Mvc.IUrlHelper url)
        {
            return new ApiRepresentation
            {
                Links = new[]
                {
                    // root of the api
                    url.MakeHomeUri().MakeWebLink(IanaLinkRelation.Self),
                    
                    url.MakeAuthenticateUri().MakeWebLink(CustomLinkRelation.Authenticate),
                    
                    url.MakeHomeTenantsUri().MakeWebLink(CustomLinkRelation.Tenants),
                },
                Version = api.Version
            };
        }
    }
}