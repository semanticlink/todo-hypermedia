﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Toolkit
{
    public delegate string ToString<in T>(T anObject);

    public static class EnumerableExtensions
    {
        /// <summary>
        ///     Similar to string.Join(',') and where every item is quoted
        /// </summary>
        /// <example>
        /// [1,2,3] => '1', '2', '3'
        /// <code>
        ///    ids.ToQuotedCsvString(id => id)
        /// </code>
        /// </example>
        public static string ToQuotedCsvString<T>(
            this IEnumerable<T> items)
        {
            return items.ToString(string.Empty, string.Empty, ", ", s => $"'{s}'");
        }

        /// <summary>
        ///     Similar to string.Join(',') but with a little more.
        /// </summary>
        /// <example>
        /// [1,2,3] => 1, 2, 3
        /// <code>
        ///    ids.ToCsvString(id => id)
        /// </code>
        /// </example>
        /// <seealso cref="ToCsvString{T}(System.Collections.IEnumerable,Toolkit.ToString{T})"/>
        public static string ToCsvString<T>(
            this IEnumerable<T> items,
            ToString<T> formatter)
        {
            return items.ToString(string.Empty, string.Empty, ", ", formatter);
        }

        /// <summary>
        ///     Similar to string.Join(',') but with a little more.
        /// </summary>
        /// <example>
        /// psuedo code list of objects:
        /// 
        ///     [ {name:1}, {name:2}, {name:3} ] => 1, 2, 3
        ///
        /// <code>
        ///    models.ToCsvString&lt;T&gt;(m => m.name)
        /// </code>
        /// </example>
        /// <seealso cref="ToCsvString{T}(System.Collections.Generic.IEnumerable{T},Toolkit.ToString{T})"/>
        public static string ToCsvString<T>(
            this IEnumerable items,
            ToString<T> formatter)
        {
            return items.ToString(string.Empty, string.Empty, ", ", formatter);
        }


        /// <summary>
        ///     Convert a single item to either an enumerable with a single item, or no items if the object is null.
        /// </summary>
        public static IEnumerable<T> ToEnumerable<T>(this T singleItem) where T : class
        {
            if (singleItem != null)
            {
                yield return singleItem;
            }
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> items) where T : class
        {
            return items == null || !items.Any();
        }

        public static void ForEach<T>(
            this IEnumerable<T> items,
            Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }

        /// <summary>
        ///     If a collection (emable is null) then return an empty collection.
        /// </summary>
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> collection)
        {
            return collection ?? new T[] { };
        }


        /// <summary>
        ///     A function very similar to string.Join(), but the items are
        ///     a delegate and a prefix and suffix are supported.
        /// </summary>
        public static string ToString<T>(
            this IEnumerable<T> items,
            string prefix,
            string suffix,
            string seperator,
            ToString<T> formatter)
        {
            var str = new StringBuilder();
            if (!string.IsNullOrEmpty(prefix))
            {
                str.Append(prefix);
            }

            if (items != null)
            {
                var isTheFirstItem = true;
                foreach (var item in items)
                {
                    if (isTheFirstItem)
                    {
                        isTheFirstItem = false;
                    }
                    else
                    {
                        str.Append(seperator);
                    }

                    str.Append(formatter(item));
                }
            }

            if (!string.IsNullOrEmpty(suffix))
            {
                str.Append(suffix);
            }

            return str.ToString();
        }


        /// <summary>
        ///     A function very similar to string.Join(), but the items are
        ///     a delegate and a prefix and suffix are supported.
        /// </summary>
        public static string ToString<T>(
            this IEnumerable items,
            string prefix,
            string suffix,
            string seperator,
            ToString<T> formatter)
        {
            var str = new StringBuilder();
            if (!string.IsNullOrEmpty(prefix))
            {
                str.Append(prefix);
            }

            if (items != null)
            {
                var isTheFirstItem = true;
                foreach (var item in items)
                {
                    if (isTheFirstItem)
                    {
                        isTheFirstItem = false;
                    }
                    else
                    {
                        str.Append(seperator);
                    }

                    str.Append(formatter((T) item));
                }
            }

            if (!string.IsNullOrEmpty(suffix))
            {
                str.Append(suffix);
            }

            return str.ToString();
        }
    }
}