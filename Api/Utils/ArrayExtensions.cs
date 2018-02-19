using System;
using System.Linq;

namespace TodoApi.Utils
{
    public static class ArrayExtensions
    {
        /// <summary>
        ///     Given an array, make a new array which is the original array
        ///     with the given items appended. This is logically an addition
        ///     style operation.
        /// </summary>
        public static T[] Concatenate<T>(this T[] array, params T[] items)
        {
            var thisLength = array != null ? array.Length : 0;
            var itemsLength = items != null ? items.Length : 0;
            var result = new T[thisLength + itemsLength];

            // Copy the 'this' items (if there are any)
            if (thisLength > 0)
            {
                Array.Copy(array, result, thisLength);
            }

            // Copy the rValue items (if there are any)
            if (itemsLength > 0)
            {
                Array.Copy(items, 0, result, thisLength, itemsLength);
            }

            return result;
        }

        /// <summary>
        ///     Remove null entries from an array.
        /// </summary>
        public static T[] RemoveNulls<T>(this T[] array)
            where T : class
        {
            return array.Any(item => item == null)
                ? array.Where(item => item != null).ToArray()
                : array;
        }
    }
}