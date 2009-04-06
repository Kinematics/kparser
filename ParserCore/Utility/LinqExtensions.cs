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
    static class LinqExtensions
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
    }

    public static class StringExtensions
    {
        public static string FormattedString(this TimeSpan timeSpan, bool forceIncludeHours)
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
    }

}
