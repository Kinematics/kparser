using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser.Utility
{
    /// <summary>
    /// A collection abstraction for lists of TimeIntervalSets for a given player.
    /// </summary>
    public class PlayerTimeIntervalSets
    {
        public readonly string PlayerName;
        public List<TimeIntervalSet> TimeIntervalSets = new List<TimeIntervalSet>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="playerName">Name of the player associated with these sets.</param>
        public PlayerTimeIntervalSets(string playerName)
        {
            PlayerName = playerName;
        }

        /// <summary>
        /// Determine whether the specified set contains the specified point in time.
        /// </summary>
        /// <param name="setName">The name of the TimeIntervalSet to query.</param>
        /// <param name="checkTime">The timestamp that we want to determine is included in the set.</param>
        /// <returns>True if the timestamp is contained within the specified set, otherwise false.</returns>
        public bool Contains(string setName, DateTime checkTime)
        {
            TimeIntervalSet checkSet = TimeIntervalSets.FirstOrDefault(s => s.SetName == setName);

            if (checkSet == null)
                return false;

            return checkSet.Contains(checkTime);
        }

        /// <summary>
        /// Add a new TimeIntervalSet to the collection.
        /// Will not add a set that contains no intervals.
        /// If the set name matches an existing set, it will merge the sets.
        /// </summary>
        /// <param name="intervalSet">The set to add.</param>
        public void AddIntervalSet(TimeIntervalSet intervalSet)
        {
            if (intervalSet == null)
                return;

            if (intervalSet.TimeIntervals.Count == 0)
                return;

            var existingSet = TimeIntervalSets.FirstOrDefault(s => s.SetName == intervalSet.SetName);

            if (existingSet == null)
            {
                TimeIntervalSets.Add(intervalSet);
            }
            else
            {
                existingSet.AddRange(intervalSet);
            }
        }

        /// <summary>
        /// Get a named interval set.
        /// </summary>
        /// <param name="setName">The name of the set to get.</param>
        /// <returns>Returns the interval set, if found.  Otherwise null.</returns>
        public TimeIntervalSet GetIntervalSet(string setName)
        {
            return TimeIntervalSets.FirstOrDefault(s => s.SetName == setName);
        }
    }

    /// <summary>
    /// A class of contained time intervals, with an identifying name.
    /// Time intervals are stored in a hash set.
    /// </summary>
    public class TimeIntervalSet
    {
        public readonly string SetName;
        public HashSet<TimeInterval> TimeIntervals = new HashSet<TimeInterval>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="setName">The name describing the intervals this set contains.</param>
        public TimeIntervalSet(string setName)
        {
            SetName = setName;
        }

        /// <summary>
        /// Add a new time interval to the set.
        /// </summary>
        /// <param name="newInterval">The interval to add.</param>
        /// <returns>Returns true if successfully added.  False if it failed/was a duplicate.</returns>
        public bool Add(TimeInterval newInterval)
        {
            return TimeIntervals.Add(newInterval);
        }

        /// <summary>
        /// Add a list of new time intervals to the set.
        /// </summary>
        /// <param name="intervalList">The list of time intervals to add.</param>
        public void AddRange(List<TimeInterval> intervalList)
        {
            if (intervalList == null)
                return;

            foreach (var interval in intervalList)
            {
                TimeIntervals.Add(interval);
            }
        }

        /// <summary>
        /// Add a list of another set's time intervals to this set.
        /// </summary>
        /// <param name="intervalSet">The set to merge with this one.</param>
        public void AddRange(TimeIntervalSet intervalSet)
        {
            if (intervalSet == null)
                return;

            foreach (var interval in intervalSet.TimeIntervals)
            {
                TimeIntervals.Add(interval);
            }
        }

        /// <summary>
        /// Determine whether the specified DateTime is contained within
        /// any time interval in this set.
        /// </summary>
        /// <param name="checkTime">The timestamp to query.</param>
        /// <returns>Returns true if one of the time intervals includes the specified
        /// timestamp; otherwise false.</returns>
        public bool Contains(DateTime checkTime)
        {
            return TimeIntervals.Any(i => i.Contains(checkTime));
        }

        /// <summary>
        /// Determine the total sum of all the time interval durations.
        /// </summary>
        /// <returns>Returns a raw duration TimeSpan for the total duration of the set.</returns>
        public TimeSpan TotalDuration()
        {
            TimeSpan totalDuration = TimeSpan.Zero;

            foreach (var interval in TimeIntervals)
            {
                totalDuration += interval.Duration;
            }

            return totalDuration;
        }
    }

    /// <summary>
    /// Class defining a particular time interval.
    /// </summary>
    public class TimeInterval
    {
        #region Properties
        public readonly DateTime StartTime;
        public readonly TimeSpan Duration;
        public readonly DateTime EndTime;

        public static readonly TimeInterval Zero = new TimeInterval(DateTime.MinValue, TimeSpan.Zero);
        #endregion

        #region Construction
        public TimeInterval(DateTime startTime, TimeSpan duration)
        {
            if (duration < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("duration", duration, "Duration must be a positive value.");

            StartTime = startTime;
            Duration = duration;
            EndTime = startTime.Add(duration);
        }

        public TimeInterval(DateTime startTime, DateTime endTime)
        {
            if (endTime < startTime)
                throw new ArgumentOutOfRangeException();

            StartTime = startTime;
            EndTime = endTime;
            Duration = endTime - startTime;
        }
        #endregion

        #region Equality checks
        public override bool Equals(object obj)
        {
            TimeInterval other = obj as TimeInterval;

            if (other == null)
                return false;

            if (this == obj)
                return true;

            if ((other.StartTime == this.StartTime) &&
                (other.EndTime == this.EndTime) &&
                (other.Duration == this.Duration))
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return this.StartTime.GetHashCode() ^ this.EndTime.GetHashCode();
        }
        #endregion

        #region Functionality
        /// <summary>
        /// Check to see whether this time interval contains a specified point in time.
        /// </summary>
        /// <param name="checkTime">The point in time to check.</param>
        /// <returns>True if found, otherwise false.</returns>
        public bool Contains(DateTime checkTime)
        {
            return ((checkTime >= StartTime) && (checkTime <= EndTime));
        }

        /// <summary>
        /// Returns the intersection of two TimeIntervals (this one and
        /// the 'other' specified one).
        /// </summary>
        /// <param name="other">The TimeInterval to intersect with.</param>
        /// <returns>Returns the intersection of the two time intervals, or
        /// TimeInterval.Zero if they do not overlap.</returns>
        public TimeInterval Intersection(TimeInterval other)
        {
            // Zero case
            if ((this.StartTime > other.EndTime) ||
                (this.EndTime < other.StartTime))
                return TimeInterval.Zero;

            // Other wholely contained
            if ((this.StartTime <= other.StartTime) &&
                (this.EndTime >= other.EndTime))
                return new TimeInterval(other.StartTime, other.EndTime);

            // This wholely contained
            if ((this.StartTime >= other.StartTime) &&
                (this.EndTime <= other.EndTime))
                return new TimeInterval(this.StartTime, this.EndTime);

            // Overlapping.  Return the matching bits.
            if (this.StartTime > other.StartTime)
                return new TimeInterval(this.StartTime, other.EndTime);
            else
                return new TimeInterval(other.StartTime, this.EndTime);
        }
        #endregion

    }
}
