using System;
using System.IO;
using JetBrains.Annotations;

namespace SemanticLink
{
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