using App.UriFactory;
using Domain.LinkRelations;
using Domain.Models;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using Toolkit.LinkRelations;
using Toolkit.Representation.LinkedRepresentation;

namespace App.RepresentationExtensions
{
    public static class ApiRepresentationExtensions
    {
        public static ApiRepresentation ToRepresentation(this ApiVersion api, IUrlHelper url)
        {
            return new ApiRepresentation
            {
                Links = new[]
                {
                    // root of the api
                    url.MakeHomeUri().MakeWebLink(IanaLinkRelation.Self),
                    
                    url.MakeAuthenticateUri().MakeWebLink(CustomLinkRelation.Authenticate),
                    
                    url.MakeUserCollectoinUri().MakeWebLink(CustomLinkRelation.Me),
                    
                    url.MakeHomeTenantsUri().MakeWebLink(CustomLinkRelation.Tenants),
                },
                Version = api.Version
            };
        }
    }
}