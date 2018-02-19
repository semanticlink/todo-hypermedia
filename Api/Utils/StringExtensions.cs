using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace TodoApi.Utils
{
    public static class StringExtensions
    {
        /// <summary>
        ///     The standard method on string is not an extension method, so provide a thunk.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrWhitespace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [NotNull]
        public static string IfNullOrWhitespace(this string str, [NotNull] string replacementValue)
        {
            return !string.IsNullOrWhiteSpace(str) ? str : replacementValue;
        }

        /// <summary>
        ///     The string utility that can be used as an extension.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
    }
}