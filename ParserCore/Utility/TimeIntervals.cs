using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser.Utility
{

    public class PlayerTimeIntervalSets
    {
        public readonly string PlayerName;
        public List<TimeIntervalSet> TimeIntervalSets = new List<TimeIntervalSet>();

        public PlayerTimeIntervalSets(string playerName)
        {
            PlayerName = playerName;
        }

        public bool Contains(string setName, DateTime checkTime)
        {
            TimeIntervalSet checkSet = TimeIntervalSets.FirstOrDefault(s => s.SetName == setName);

            if (checkSet == null)
                return false;

            return checkSet.Contains(checkTime);
        }

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

        public TimeIntervalSet GetIntervalSet(string setName)
        {
            return TimeIntervalSets.FirstOrDefault(s => s.SetName == setName);
        }
    }

    public class TimeIntervalSet
    {
        public readonly string SetName;
        public HashSet<TimeInterval> TimeIntervals = new HashSet<TimeInterval>();

        public TimeIntervalSet(string setName)
        {
            SetName = setName;
        }

        public bool Add(TimeInterval newInterval)
        {
            return TimeIntervals.Add(newInterval);
        }

        public void AddRange(List<TimeInterval> intervalList)
        {
            if (intervalList == null)
                return;

            foreach (var interval in intervalList)
            {
                TimeIntervals.Add(interval);
            }
        }

        public void AddRange(TimeIntervalSet intervalSet)
        {
            if (intervalSet == null)
                return;

            foreach (var interval in intervalSet.TimeIntervals)
            {
                TimeIntervals.Add(interval);
            }
        }

        public bool Contains(DateTime checkTime)
        {
            return TimeIntervals.Any(i => i.Contains(checkTime));
        }

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
        /// <param name="checkTime"></param>
        /// <returns></returns>
        public bool Contains(DateTime checkTime)
        {
            return ((checkTime >= StartTime) && (checkTime <= EndTime));
        }

        /// <summary>
        /// Returns the intersection of two TimeIntervals (this one and
        /// the 'other' specified one).
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
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

            // Overlapping
            if (this.StartTime > other.StartTime)
                return new TimeInterval(this.StartTime, other.EndTime);
            else
                return new TimeInterval(other.StartTime, this.EndTime);
        }
        #endregion

    }
}
