using System.Collections.Generic;
using JetBrains.Annotations;

namespace Toolkit
{
    public static class ListExtensions
    {
        /// <summary>
        ///     Clone a list. This list needs to be shallow (by value)
        /// </summary>
        public static List<T> Clone<T>([CanBeNull] this List<T> list) where T : class
        {
            return new List<T>(list ?? new List<T>());
        }
    }
}