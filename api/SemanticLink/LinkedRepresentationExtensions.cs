using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SemanticLink.Toolkit;

namespace SemanticLink
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
    
    
    namespace Toolkit
    {
        /// <summary>
        ///     These extensions are here so that this library is pretty much self contained and also they aren't
        ///     picked up by the IDE and collide with other libraries. Convenient but not elegant
        /// </summary>
        public static class ExceptionExtensions
        {
            [NotNull]
            public static string ThrowInvalidDataExceptionIfNullOrWhiteSpace(this string str, string message)
            {
                return ThrowInvalidDataExceptionIf(str, string.IsNullOrWhiteSpace, message);
            }

            public static T ThrowInvalidDataExceptionIf<T>(this T anObject, Func<T, bool> predicate, string message)
            {
                if (predicate(anObject))
                {
                    throw new InvalidDataException(message);
                }

                return anObject;
            }
        }
    }
}