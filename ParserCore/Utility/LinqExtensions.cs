using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser
{
    /// <summary>
    /// Static class for adding general utility extension functions
    /// to IEnumerable classes.
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Gets the maximum element of an IEnumerable source based on a provided comparison
        /// delegate and a selection delegate.
        /// </summary>
        /// <typeparam name="T">The type of IEnumerable list.</typeparam>
        /// <typeparam name="C">The type of value to be compared.  Must be IComparable.</typeparam>
        /// <typeparam name="V">The individual element type of the IEnumerable list.</typeparam>
        /// <param name="source">The IEnumerable list.</param>
        /// <param name="comparisonMap">The comparison delegate.</param>
        /// <param name="selectMap">The selection delegate.</param>
        /// <returns>The element V that is determined to have the maximum value of the list.</returns>
        public static V MaxEntry<T, C, V>(this IEnumerable<T> source,
                            Func<T, C> comparisonMap,
                            Func<T, V> selectMap)
        {
            if ((source == null) || (source.Count() == 0))
                return default(V);

            C maxValueC = comparisonMap(source.First());

            if ((maxValueC is IComparable) == false)
                throw new InvalidOperationException("ComparisonMap must return an IComparable type object.");

            V maxElement = selectMap(source.First());

            foreach (T sourceElement in source)
            {
                C value = comparisonMap(sourceElement);
                IComparable<C> cValue = value as IComparable<C>;

                if (cValue.CompareTo(maxValueC) > 0)
                {
                    maxValueC = value;
                    maxElement = selectMap(sourceElement);
                }
            }

            return maxElement;
        }


        /// <summary>
        /// Gets the minimum element of an IEnumerable source based on a provided comparison
        /// delegate and a selection delegate.
        /// </summary>
        /// <typeparam name="T">The type of IEnumerable list.</typeparam>
        /// <typeparam name="C">The type of value to be compared.  Must be IComparable.</typeparam>
        /// <typeparam name="V">The individual element type of the IEnumerable list.</typeparam>
        /// <param name="source">The IEnumerable list.</param>
        /// <param name="comparisonMap">The comparison delegate.</param>
        /// <param name="selectMap">The selection delegate.</param>
        /// <returns>The element V that is determined to have the minimum value of the list.</returns>
        public static V MinEntry<T, C, V>(this IEnumerable<T> source,
                    Func<T, C> comparisonMap,
                    Func<T, V> selectMap)
        {
            if ((source == null) || (source.Count() == 0))
                return default(V);

            C maxValueC = comparisonMap(source.First());

            if ((maxValueC is IComparable) == false)
                throw new InvalidOperationException("ComparisonMap must return an IComparable type object.");

            V maxElement = selectMap(source.First());

            foreach (T sourceElement in source)
            {
                C value = comparisonMap(sourceElement);
                IComparable<C> cValue = value as IComparable<C>;

                if (cValue.CompareTo(maxValueC) < 0)
                {
                    maxValueC = value;
                    maxElement = selectMap(sourceElement);
                }
            }

            return maxElement;
        }

        /// <summary>
        /// The generalized grouping extension method.  It groups lists of adjacently
        /// identical elements together.
        /// see: http://blogs.msdn.com/ericwhite/archive/2008/04/21/the-groupadjacent-extension-method.aspx
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<IGrouping<TKey, TSource>> GroupAdjacent<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            TKey last = default(TKey);
            bool haveLast = false;
            List<TSource> list = new List<TSource>();

            foreach (TSource s in source)
            {
                TKey k = keySelector(s);
                if (haveLast)
                {
                    if (!k.Equals(last))
                    {
                        yield return new GroupOfAdjacent<TSource, TKey>(list, last);
                        list = new List<TSource>();
                        list.Add(s);
                        last = k;
                    }
                    else
                    {
                        list.Add(s);
                        last = k;
                    }
                }
                else
                {
                    list.Add(s);
                    last = k;
                    haveLast = true;
                }
            }
            if (haveLast)
                yield return new GroupOfAdjacent<TSource, TKey>(list, last);
        }

        /// <summary>
        /// A specialized version of the grouping extension that groups adjacent elements
        /// that are within a specified time limit of each other together.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="adjacentTime"></param>
        /// <returns></returns>
        public static IEnumerable<IGrouping<TKey, TSource>> GroupAdjacentByTimeLimit<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, DateTime> keySelector,
            TimeSpan adjacentTime) where TKey : IComparable<DateTime>
        {
            DateTime first = default(DateTime);
            DateTime last = default(DateTime);
            bool haveLast = false;
            List<TSource> list = new List<TSource>();

            foreach (TSource s in source)
            {
                DateTime k = keySelector(s);
                if (haveLast)
                {
                    if (!((k - first) <= adjacentTime))
                    {
                        yield return (IGrouping<TKey, TSource>)(new GroupOfAdjacent<TSource, DateTime>(list, first));
                        list = new List<TSource>();
                        list.Add(s);
                        first = k;
                        last = k;
                    }
                    else
                    {
                        list.Add(s);
                        last = k;
                    }
                }
                else
                {
                    list.Add(s);
                    first = k;
                    last = k;
                    haveLast = true;
                }
            }
            if (haveLast)
                yield return (IGrouping<TKey, TSource>)(new GroupOfAdjacent<TSource, DateTime>(list, first));
        }


        public static IEnumerable<IGrouping<TKey, TSource>> GroupAdjacentByTimeDiffLimit<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, DateTime> keySelector,
            TimeSpan adjacentTime) where TKey : IComparable<DateTime>
        {
            DateTime first = default(DateTime);
            DateTime last = default(DateTime);
            bool haveLast = false;
            List<TSource> list = new List<TSource>();

            foreach (TSource s in source)
            {
                DateTime k = keySelector(s);
                if (haveLast)
                {
                    if (!((k - last) <= adjacentTime))
                    {
                        yield return (IGrouping<TKey, TSource>)(new GroupOfAdjacent<TSource, DateTime>(list, first));
                        list = new List<TSource>();
                        list.Add(s);
                        first = k;
                        last = k;
                    }
                    else
                    {
                        list.Add(s);
                        last = k;
                    }
                }
                else
                {
                    list.Add(s);
                    first = k;
                    last = k;
                    haveLast = true;
                }
            }
            if (haveLast)
                yield return (IGrouping<TKey, TSource>)(new GroupOfAdjacent<TSource, DateTime>(list, first));
        }

        /// <summary>
        /// A specialized version of the grouping extension that groups adjacent elements
        /// that are equal, or that match with a comparer function.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="adjacentTime"></param>
        /// <returns></returns>
        public static IEnumerable<IGrouping<TKey, TSource>> GroupAdjacentWithComparer<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey, bool> keyComparer)
        {
            TKey first = default(TKey);
            TKey last = default(TKey);
            bool haveLast = false;
            List<TSource> list = new List<TSource>();

            foreach (TSource s in source)
            {
                TKey k = keySelector(s);
                if (haveLast)
                {
                    if (k.Equals(last) || (keyComparer(k) == true))
                    {
                        list.Add(s);
                        last = k;
                    }
                    else
                    {
                        yield return new GroupOfAdjacent<TSource, TKey>(list, first);
                        list = new List<TSource>();
                        list.Add(s);
                        first = k;
                        last = k;
                    }
                }
                else
                {
                    list.Add(s);
                    first = k;
                    last = k;
                    haveLast = true;
                }
            }
            if (haveLast)
                yield return new GroupOfAdjacent<TSource, TKey>(list, first);
        }

        /// <summary>
        /// The Zip extension method processes two sequences, matching up each item
        /// in one sequence with a corresponding item in another sequence.
        /// see: http://blogs.msdn.com/ericwhite/archive/2009/07/05/comparing-two-open-xml-documents-using-the-zip-extension-method.aspx
        /// </summary>
        /// <typeparam name="TFirst">The first sequence type</typeparam>
        /// <typeparam name="TSecond">The second sequence type</typeparam>
        /// <typeparam name="TResult">The result type</typeparam>
        /// <param name="first">The first sequence</param>
        /// <param name="second">The second sequence</param>
        /// <param name="func">The lambda that builds the output type from the original sequences</param>
        /// <returns></returns>
        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(
            this IEnumerable<TFirst> first,
            IEnumerable<TSecond> second,
            Func<TFirst, TSecond, TResult> func)
        {
            var ie1 = first.GetEnumerator();
            var ie2 = second.GetEnumerator();

            while (ie1.MoveNext() && ie2.MoveNext())
                yield return func(ie1.Current, ie2.Current);
        }

        /// <summary>
        /// see: http://blogs.msdn.com/ericwhite/archive/2010/02/15/rollup-extension-method-create-running-totals-using-linq-to-objects.aspx
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="seed"></param>
        /// <param name="projection"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> Rollup<TSource, TResult>(
            this IEnumerable<TSource> source,
            TResult seed,
            Func<TSource, TResult, TResult> projection)
        {
            TResult nextSeed = seed;
            foreach (TSource src in source)
            {
                TResult projectedValue = projection(src, nextSeed);
                nextSeed = projectedValue;
                yield return projectedValue;
            }
        }

        /// <summary>
        /// Variant on Rollup to separate values from past elements
        /// instead of aggragate with them.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="projection"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> Separate<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector,
            Func<TSource, TResult, TResult> projection)
        {
            bool haveLast = false;
            TResult last = default(TResult);
            TResult projectedValue = default(TResult);

            foreach (TSource src in source)
            {
                if (haveLast)
                {
                    projectedValue = projection(src, last);
                    last = selector(src);
                }
                else
                {
                    last = selector(src);
                    projectedValue = last;
                    haveLast = true;
                }

                yield return projectedValue;
            }
        }


        /// <summary>
        /// Extension method to add AddRange function to ICollections.
        /// </summary>
        /// <typeparam name="T">The type of object held by the ICollection.</typeparam>
        /// <param name="target">The collection to add items to.</param>
        /// <param name="source">The collection to copy items from.</param>
        /// <returns>Returns the target ICollection to allow continuation functions.</returns>
        public static ICollection<T> AddRange<T>(this ICollection<T> target, IEnumerable<T> source)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (source == null)
                throw new ArgumentNullException("source");
            if (target.IsReadOnly)
                throw new ArgumentException("target is read-only");

            foreach (T item in source)
            {
                target.Add(item);
            }

            return target;
        }
    }


    /// <summary>
    /// A class to hold extensions specifically designed to generate specially formatted strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Default version for formatting timespan strings.
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static string FormattedShortTimeString(this TimeSpan timeSpan)
        {
            return timeSpan.FormattedShortTimeString(false);
        }

        /// <summary>
        /// Format a timespan into a displayable string, with option to force hours to be shown.
        /// Appropriate rounding done on seconds.
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <param name="forceIncludeHours"></param>
        /// <returns></returns>
        public static string FormattedShortTimeString(this TimeSpan timeSpan, bool forceIncludeHours)
        {
            if (timeSpan == null)
                return default(string);

            string formattedTimeSpan = string.Empty;

            double baseSeconds = timeSpan.Seconds + ((double)timeSpan.Milliseconds / 1000);
            int roundedSeconds = (int)Math.Round(baseSeconds);

            if ((forceIncludeHours == true) || (timeSpan.Hours > 0))
            {
                formattedTimeSpan = string.Format("{0:d}:{1,2:d2}:{2,2:d2}",
                    (int)timeSpan.TotalHours, timeSpan.Minutes,
                    roundedSeconds);
            }
            else
            {
                formattedTimeSpan = string.Format("{0:d}:{1,2:d2}",
                    (int)timeSpan.TotalMinutes,
                    roundedSeconds);
            }


            return formattedTimeSpan;
        }

        /// <summary>
        /// Given a list of StringMods and supporting info, fill in the extra detail
        /// required of the StringMods object and add it to the list, while also
        /// appending the text to the stringbuilder.
        /// </summary>
        /// <param name="smList">List of StringMods.  Object that this extension method is tied to.</param>
        /// <param name="sb">The StringBuilder that the text is added to.</param>
        /// <param name="text">The text to be added/modified.</param>
        /// <param name="mods">The base parameter mods passed in (bold/underline/color).</param>
        /// <returns>Returns the completed StringMods object.</returns>
        public static StringMods AddModsAndAppendToSB(this List<StringMods> smList,
            StringBuilder sb, string text, StringMods mods)
        {
            if (sb == null)
                throw new ArgumentNullException("sb");
            if (mods == null)
                throw new ArgumentNullException("mods");
            if (string.IsNullOrEmpty(text))
                return mods;

            mods.Start = sb.Length;
            mods.Length = text.Length;
            sb.Append(text);

            smList.Add(mods);

            return mods;
        }

        /// <summary>
        /// Given a list of StringMods and supporting info, fill in the extra detail
        /// required of the StringMods object and add it to the list, while also
        /// appending the text (with newline) to the stringbuilder.
        /// </summary>
        /// <param name="smList">List of StringMods.  Object that this extension method is tied to.</param>
        /// <param name="sb">The StringBuilder that the text is added to.</param>
        /// <param name="text">The text to be added/modified.</param>
        /// <param name="mods">The base parameter mods passed in (bold/underline/color).</param>
        /// <returns>Returns the completed StringMods object.</returns>
        public static StringMods AddModsAndAppendLineToSB(this List<StringMods> smList,
            StringBuilder sb, string text, StringMods mods)
        {
            if (sb == null)
                throw new ArgumentNullException("sb");
            if (mods == null)
                throw new ArgumentNullException("mods");
            if (string.IsNullOrEmpty(text))
                return mods;

            mods.Start = sb.Length;
            mods.Length = text.Length;
            sb.Append(text + "\n");

            smList.Add(mods);

            return mods;
        }

    }


    /// <summary>
    /// A grouping enumeration class for use by the linq extensions for grouping.
    /// </summary>
    /// <typeparam name="TSource">The object to group.</typeparam>
    /// <typeparam name="TKey">The key to group on.</typeparam>
    public class GroupOfAdjacent<TSource, TKey> : IEnumerable<TSource>, IGrouping<TKey, TSource>
    {
        public TKey Key { get; set; }
        private List<TSource> GroupList { get; set; }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.Generic.IEnumerable<TSource>)this).GetEnumerator();
        }

        System.Collections.Generic.IEnumerator<TSource> System.Collections.Generic.IEnumerable<TSource>.GetEnumerator()
        {
            foreach (var s in GroupList)
                yield return s;
        }

        public GroupOfAdjacent(List<TSource> source, TKey key)
        {
            GroupList = source;
            Key = key;
        }
    }
}
