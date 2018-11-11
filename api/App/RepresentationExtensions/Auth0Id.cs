using System.Collections.Generic;
using System.Linq;
using App.UriFactory;
using Domain.LinkRelations;
using Microsoft.AspNetCore.Mvc;
using SemanticLink;

namespace App.RepresentationExtensions
{
    /// <summary>
    ///     A class to be able to read the Auth0 Id format and take in the the domain and spit out something
    ///     semi sensible
    /// <example>
    ///    auth0|5b32b696a8c12d3b9a32b138
    ///    rel = auth0
    ///    uri = https://somesubdomain.auth0.com/5b32b696a8c12d3b9a32b138
    ///    title = externalId
    /// </example> 
    /// </summary>
    /// 
    public class Auth0Id
    {
        private const char Delimiter = '|';
        private const string Rel = CustomLinkRelation.Authenticator;

        private string Id { get; set; }
        private string Title { get; set; }

        /// <summary>
        ///     Very simple parser on bar delimited
        /// </summary>
        /// <param name="id"></param>
        private void Parse(string id)
        {
            var provider = id.Split(Delimiter);

            if (provider.Length == 2)
            {
                Id = provider.Last();
                Title = provider.First();
            }
            else
            {
                // TODO logging
                // we don't have a format we recognise
            }
        }

        public WebLink MakeWebLink(string id, IUrlHelper url)
        {
            Parse(id);
            return Id.MakeUserAuthenticator(Title, url).MakeWebLink(Rel, Title);
        }

        public WebLink[] MakeWebLinks(List<string> ids, IUrlHelper url)
        {
            return ids
                .Select(id => MakeWebLink(id, url))
                .ToArray();
        }
    }
}