using System;
using System.Linq;
using System.Threading.Tasks;

namespace Toolkit.Representation.LinkedRepresentation
{
    public static class LinkedRepresentationExtensions
    {
        public static async Task<string> GetLinkUri<T>(this Task<T> r, string relationshipName)
            where T : LinkedRepresentation
        {
            return (await r.ConfigureAwait(false))
                .GetLinkUri(relationshipName);
        }

        public static string GetLinkUri<T>(this T r, string relationshipName)
            where T : LinkedRepresentation
        {
            return r
                .Links
                .FirstOrDefault(l => l.Rel.Equals(relationshipName, StringComparison.InvariantCultureIgnoreCase))
                ?.HRef
                .ThrowInvalidDataExceptionIfNullOrWhiteSpace($"Link with relationship '{relationshipName}' not found");
        }
    }
}