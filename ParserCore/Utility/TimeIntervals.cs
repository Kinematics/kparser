﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WaywardGamers.KParser.Utility
{
    /// PlayerTimeIntervalSets contains a list of TimeIntervalSets for a given player.
    /// TimeIntervalSet contains a list of time intervals for a given action.
    /// TimeInterval marks a period of time with defined start and end points.

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

    /// <summary>
    /// A class allowing for the construction of PlayerTimeIntervalSets from
    /// a given dataset.
    /// </summary>
    public static class CollectTimeIntervals
    {
        #region Buff lists
        /// <summary>
        /// Get the list of spells/ability buffs that affect accuracy.
        /// </summary>
        public static List<string> AccuracyBuffNames
        {
            get
            {
                return new List<string>()
                {
                    Resources.ParsedStrings.Focus,
                    Resources.ParsedStrings.Aggressor,
                    Resources.ParsedStrings.Sharpshot,
                    Resources.ParsedStrings.Souleater,
                    Resources.ParsedStrings.DiabolicEye,
                    Resources.ParsedStrings.RngRoll,
                    Resources.ParsedStrings.Hasso,
                    Resources.ParsedStrings.Yonin,
                    Resources.ParsedStrings.Innin,
                    Resources.ParsedStrings.Madrigal1,
                    Resources.ParsedStrings.Madrigal2,
                    Resources.ParsedStrings.StalwartTonic,
                    Resources.ParsedStrings.StalwartGambir
                };
            }
        }

        /// <summary>
        /// Get the list of spells/ability buffs that affect accuracy.
        /// </summary>
        public static List<string> AccuracyDefBuffNames
        {
            get
            {
                return new List<string>()
                {
                    Resources.ParsedStrings.Mambo1,
                    Resources.ParsedStrings.Mambo2,
                    Resources.ParsedStrings.NinRoll,
                    Resources.ParsedStrings.Dodge,
                    Resources.ParsedStrings.Yonin,
                    Resources.ParsedStrings.Innin,
                    Resources.ParsedStrings.Aggressor,
                };
            }
        }

        /// <summary>
        /// Get the list of spells/ability buffs that affect attack.
        /// </summary>
        public static List<string> AttackBuffNames
        {
            get
            {
                return new List<string>()
                {
                    Resources.ParsedStrings.Minuet1,
                    Resources.ParsedStrings.Minuet2,
                    Resources.ParsedStrings.Minuet3,
                    Resources.ParsedStrings.Minuet4,
                    Resources.ParsedStrings.DrkRoll,
                    Resources.ParsedStrings.Berserk,
                    Resources.ParsedStrings.Warcry,
                    Resources.ParsedStrings.LastResort,
                    Resources.ParsedStrings.Souleater,
                    Resources.ParsedStrings.Hasso,
                    Resources.ParsedStrings.Defender,
                    Resources.ParsedStrings.Dia1,
                    Resources.ParsedStrings.Dia2,
                    Resources.ParsedStrings.Dia3,
                    Resources.ParsedStrings.Footwork,
                    Resources.ParsedStrings.Impetus,
                    Resources.ParsedStrings.StalwartTonic,
                    Resources.ParsedStrings.StalwartGambir
                };
            }
        }

        /// <summary>
        /// Get the list of spells/ability buffs that affect attack.
        /// </summary>
        public static List<string> AttackDefBuffNames
        {
            get
            {
                return new List<string>()
                {
                    Resources.ParsedStrings.Minne1,
                    Resources.ParsedStrings.Minne2,
                    Resources.ParsedStrings.Minne3,
                    Resources.ParsedStrings.Minne4,
                    Resources.ParsedStrings.Minne5,
                    Resources.ParsedStrings.Defender,
                    Resources.ParsedStrings.Rampart,
                    Resources.ParsedStrings.Sentinel,
                    Resources.ParsedStrings.Protect1,
                    Resources.ParsedStrings.Protect2,
                    Resources.ParsedStrings.Protect3,
                    Resources.ParsedStrings.Protect4,
                    Resources.ParsedStrings.Protect5,
                };
            }
        }

        /// <summary>
        /// Get the list of spells/ability buffs that affect haste.
        /// </summary>
        public static List<string> HasteBuffNames
        {
            get
            {
                return new List<string>()
                {
                    Resources.ParsedStrings.Haste,
                    Resources.ParsedStrings.March1,
                    Resources.ParsedStrings.March2,
                    Resources.ParsedStrings.Hasso,
                    Resources.ParsedStrings.HasteSamba,
                    Resources.ParsedStrings.Refueling,
                    Resources.ParsedStrings.AnimatingWail
                };
            }
        }

        /// <summary>
        /// Get the list of spells/ability buffs that affect haste.
        /// </summary>
        public static List<string> CritBuffNames
        {
            get
            {
                return new List<string>()
                {
                    Resources.ParsedStrings.Restraint,
                    Resources.ParsedStrings.ThfRoll,
                    Resources.ParsedStrings.Impetus,
                    Resources.ParsedStrings.Innin,
                    Resources.ParsedStrings.Focus,
                    Resources.ParsedStrings.ChampionTonic,
                    Resources.ParsedStrings.ChampionGambir
                };
            }
        }

        /// <summary>
        /// Get the list of spells/ability buffs that affect haste.
        /// </summary>
        public static List<string> WeaponskillBuffNames
        {
            get
            {
                return new List<string>()
                {
                    Resources.ParsedStrings.Restraint,
                };
            }
        }

        /// <summary>
        /// Get the list of spells/ability buffs that are tracked, but don't
        /// affect accuracy/attack/haste.
        /// </summary>
        public static List<string> OtherBuffNames
        {
            get
            {
                return new List<string>()
                {
                    Resources.ParsedStrings.SamRoll,
                    Resources.ParsedStrings.WarRoll,
                    Resources.ParsedStrings.ThfRoll,
                    Resources.ParsedStrings.BlmRoll,
                };
            }
        }

        /// <summary>
        /// Get the list of spells/ability buffs that are tracked, but don't
        /// affect accuracy/attack/haste.
        /// </summary>
        public static List<string> OtherDefBuffNames
        {
            get
            {
                return new List<string>()
                {
                    Resources.ParsedStrings.Reprisal,
                    Resources.ParsedStrings.Palisade,
                };
            }
        }

        /// <summary>
        /// Get a list of all tracked spells/abilities.
        /// </summary>
        public static List<string> TrackedOffenseBuffNames
        {
            get
            {
                return AccuracyBuffNames.Concat(
                       AttackBuffNames.Concat(
                       HasteBuffNames.Concat(
                       CritBuffNames.Concat(
                       WeaponskillBuffNames.Concat(
                       OtherBuffNames)))))
                    .ToList<string>();
            }
        }

        /// <summary>
        /// Get a list of all tracked spells/abilities.
        /// </summary>
        public static List<string> TrackedDefenseBuffNames
        {
            get
            {
                return AccuracyDefBuffNames.Concat(
                       AttackDefBuffNames.Concat(
                       OtherDefBuffNames))
                    .ToList<string>();
            }
        }
        #endregion

        #region Get Time-based info collected
        /// <summary>
        /// Get a list of PlayerTimeIntervalSets for a given list of players based
        /// on info in the provided database.
        /// </summary>
        /// <param name="dataSet">The database to source from.</param>
        /// <param name="playerList">The list of players to get data for.</param>
        /// <returns></returns>
        static public List<PlayerTimeIntervalSets> GetTimeIntervals(KPDatabaseDataSet dataSet, List<string> playerList)
        {
            if (dataSet == null)
                throw new ArgumentNullException("dataSet");

            if (playerList == null)
                throw new ArgumentNullException("playerList");

            List<PlayerTimeIntervalSets> playerIntervals = new List<PlayerTimeIntervalSets>();

            // If no players provided, just return an empty list.
            if (playerList.Count == 0)
                return playerIntervals;

            // Add a set for each player in the list.
            foreach (var player in playerList)
            {
                playerIntervals.Add(new PlayerTimeIntervalSets(player));
            }

            // General use regexes.
            string anySongRegex = Resources.ParsedStrings.AnySong;
            string anyRollRegex = Resources.ParsedStrings.PhantomRoll;

            CompileFixedLengthBuffs(Resources.ParsedStrings.Protect1, TimeSpan.FromMinutes(30),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Protect2, TimeSpan.FromMinutes(30),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Protect3, TimeSpan.FromMinutes(30),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Protect4, TimeSpan.FromMinutes(30),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Protect5, TimeSpan.FromMinutes(30),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Focus, TimeSpan.FromMinutes(2), 
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Dodge, TimeSpan.FromMinutes(2),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Aggressor, TimeSpan.FromMinutes(3),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Sharpshot, TimeSpan.FromMinutes(1),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Souleater, TimeSpan.FromMinutes(1),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.DiabolicEye, TimeSpan.FromMinutes(3),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Berserk, TimeSpan.FromMinutes(3),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Defender, TimeSpan.FromMinutes(3),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Warcry, TimeSpan.FromSeconds(30),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Rampart, TimeSpan.FromSeconds(30),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Sentinel, TimeSpan.FromSeconds(30),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Reprisal, TimeSpan.FromMinutes(1),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Palisade, TimeSpan.FromMinutes(1),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.LastResort, TimeSpan.FromSeconds(30),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Haste, TimeSpan.FromMinutes(3),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Footwork, TimeSpan.FromMinutes(5),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Refueling, TimeSpan.FromMinutes(5),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.AnimatingWail, TimeSpan.FromMinutes(5),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Restraint, TimeSpan.FromMinutes(5),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.Impetus, TimeSpan.FromMinutes(3),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.StalwartTonic, TimeSpan.FromMinutes(5),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.StalwartGambir, TimeSpan.FromMinutes(5),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.AsceticTonic, TimeSpan.FromMinutes(5),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.AsceticGambir, TimeSpan.FromMinutes(5),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.ChampionTonic, TimeSpan.FromSeconds(60),
                playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs(Resources.ParsedStrings.ChampionGambir, TimeSpan.FromSeconds(60),
                playerList, playerIntervals, dataSet);


            CompileStanceBuffs(Resources.ParsedStrings.Hasso, Resources.ParsedStrings.Seigan,
                TimeSpan.FromMinutes(5), playerList, playerIntervals, dataSet);
            CompileStanceBuffs(Resources.ParsedStrings.Yonin, Resources.ParsedStrings.Innin,
                TimeSpan.FromMinutes(5), playerList, playerIntervals, dataSet);
            CompileStanceBuffs(Resources.ParsedStrings.Innin, Resources.ParsedStrings.Yonin,
                TimeSpan.FromMinutes(5), playerList, playerIntervals, dataSet);
            CompileStanceBuffs(Resources.ParsedStrings.LightArts, Resources.ParsedStrings.DarkArts,
                TimeSpan.FromHours(2), playerList, playerIntervals, dataSet);
            CompileStanceBuffs(Resources.ParsedStrings.DarkArts, Resources.ParsedStrings.LightArts,
                TimeSpan.FromHours(2), playerList, playerIntervals, dataSet);


            CompileSongBuffs(Resources.ParsedStrings.Minuet1, anySongRegex, TimeSpan.FromSeconds(144),
                playerList, playerIntervals, dataSet);
            CompileSongBuffs(Resources.ParsedStrings.Minuet2, anySongRegex, TimeSpan.FromSeconds(144),
                playerList, playerIntervals, dataSet);
            CompileSongBuffs(Resources.ParsedStrings.Minuet3, anySongRegex, TimeSpan.FromSeconds(144),
                playerList, playerIntervals, dataSet);
            CompileSongBuffs(Resources.ParsedStrings.Minuet4, anySongRegex, TimeSpan.FromSeconds(144),
                playerList, playerIntervals, dataSet);
            CompileSongBuffs(Resources.ParsedStrings.Madrigal1, anySongRegex, TimeSpan.FromSeconds(144),
                playerList, playerIntervals, dataSet);
            CompileSongBuffs(Resources.ParsedStrings.Madrigal2, anySongRegex, TimeSpan.FromSeconds(144),
                playerList, playerIntervals, dataSet);
            CompileSongBuffs(Resources.ParsedStrings.Prelude1, anySongRegex, TimeSpan.FromSeconds(144),
                playerList, playerIntervals, dataSet);
            CompileSongBuffs(Resources.ParsedStrings.Prelude2, anySongRegex, TimeSpan.FromSeconds(144),
                playerList, playerIntervals, dataSet);
            CompileSongBuffs(Resources.ParsedStrings.March1, anySongRegex, TimeSpan.FromSeconds(144),
                playerList, playerIntervals, dataSet);
            CompileSongBuffs(Resources.ParsedStrings.March2, anySongRegex, TimeSpan.FromSeconds(144),
                playerList, playerIntervals, dataSet);
            CompileSongBuffs(Resources.ParsedStrings.Mambo1, anySongRegex, TimeSpan.FromSeconds(144),
                playerList, playerIntervals, dataSet);
            CompileSongBuffs(Resources.ParsedStrings.Mambo2, anySongRegex, TimeSpan.FromSeconds(144),
                playerList, playerIntervals, dataSet);
            CompileSongBuffs(Resources.ParsedStrings.Minne1, anySongRegex, TimeSpan.FromSeconds(144),
                playerList, playerIntervals, dataSet);
            CompileSongBuffs(Resources.ParsedStrings.Minne2, anySongRegex, TimeSpan.FromSeconds(144),
                playerList, playerIntervals, dataSet);
            CompileSongBuffs(Resources.ParsedStrings.Minne3, anySongRegex, TimeSpan.FromSeconds(144),
                playerList, playerIntervals, dataSet);
            CompileSongBuffs(Resources.ParsedStrings.Minne4, anySongRegex, TimeSpan.FromSeconds(144),
                playerList, playerIntervals, dataSet);
            CompileSongBuffs(Resources.ParsedStrings.Minne5, anySongRegex, TimeSpan.FromSeconds(144),
                playerList, playerIntervals, dataSet);


            CompileRollBuffs(Resources.ParsedStrings.RngRoll, anyRollRegex, TimeSpan.FromMinutes(5),
                playerList, playerIntervals, dataSet);
            CompileRollBuffs(Resources.ParsedStrings.DrkRoll, anyRollRegex, TimeSpan.FromMinutes(5),
                playerList, playerIntervals, dataSet);
            CompileRollBuffs(Resources.ParsedStrings.RngRoll, anyRollRegex, TimeSpan.FromMinutes(5),
                playerList, playerIntervals, dataSet);
            CompileRollBuffs(Resources.ParsedStrings.SamRoll, anyRollRegex, TimeSpan.FromMinutes(5),
                playerList, playerIntervals, dataSet);
            CompileRollBuffs(Resources.ParsedStrings.WarRoll, anyRollRegex, TimeSpan.FromMinutes(5),
                playerList, playerIntervals, dataSet);
            CompileRollBuffs(Resources.ParsedStrings.ThfRoll, anyRollRegex, TimeSpan.FromMinutes(5),
                playerList, playerIntervals, dataSet);
            CompileRollBuffs(Resources.ParsedStrings.NinRoll, anyRollRegex, TimeSpan.FromMinutes(5),
                playerList, playerIntervals, dataSet);


            // Debuffs only need to be calculated once for all players
            //CompileDebuffs("Gravity", TimeSpan.FromMinutes(1), playerList, playerIntervals, dataSet);

            CompileSambaBuffs(Resources.ParsedStrings.HasteSamba, Resources.ParsedStrings.AnySamba,
                TimeSpan.FromMinutes(2), playerList, playerIntervals, dataSet);

            CompileDebuffsWithOR(Resources.ParsedStrings.Dia1, Resources.ParsedStrings.Bio1,
                TimeSpan.FromMinutes(1), playerList, playerIntervals, dataSet);
            CompileDebuffsWithOR(Resources.ParsedStrings.Dia2, Resources.ParsedStrings.Bio2,
                TimeSpan.FromMinutes(2), playerList, playerIntervals, dataSet);
            CompileDebuffsWithOR(Resources.ParsedStrings.Dia3, Resources.ParsedStrings.Bio3,
                TimeSpan.FromMinutes(1), playerList, playerIntervals, dataSet);


            return playerIntervals;
        }

        /// <summary>
        /// Get time intervals for buffs that have a fixed duration.
        /// </summary>
        /// <param name="buffName">The name of the buff.</param>
        /// <param name="duration">The duration the buff lasts.</param>
        /// <param name="player">The PlayerTimeIntervalSets to add the intervals to.</param>
        /// <param name="dataSet">The database to pull the info from.</param>
        static private void CompileFixedLengthBuffs(string buffName, TimeSpan duration,
            List<string> playerList, List<PlayerTimeIntervalSets> playerIntervals,
            KPDatabaseDataSet dataSet)
        {
            if (string.IsNullOrEmpty(buffName))
                throw new ArgumentNullException("buffName");

            if (duration < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("duration");

            if (playerList == null)
                throw new ArgumentNullException("playerList");

            if (playerIntervals == null)
                throw new ArgumentNullException("playerIntervals");

            if (dataSet == null)
                throw new ArgumentNullException("dataSet");

            var action = dataSet.Actions.FirstOrDefault(a => a.ActionName == buffName);

            if (action != null)
            {
                var actions = action.GetInteractionsRows();

                var buffsByTarget = from i in actions
                                    where i.IsActorIDNull() == false &&
                                          i.Preparing == false
                                    let targetName = (i.IsTargetIDNull() == true) ?
                                        i.CombatantsRowByActorCombatantRelation.CombatantName :
                                        i.CombatantsRowByTargetCombatantRelation.CombatantName
                                    where playerList.Contains(targetName)
                                    group i by targetName;

                if (buffsByTarget != null)
                {
                    foreach (var targetBuffs in buffsByTarget)
                    {
                        // Should be faster than a full count, as it should succeed if anything exists.
                        if (targetBuffs.Any(a => a.InteractionID > 0))
                        {
                            var playerIntervalSet = playerIntervals.First(s => s.PlayerName == targetBuffs.Key);
                            TimeIntervalSet intervalSet = new TimeIntervalSet(buffName);

                            foreach (var buff in targetBuffs)
                            {
                                intervalSet.Add(new TimeInterval(buff.Timestamp, duration));
                            }

                            playerIntervalSet.AddIntervalSet(intervalSet);
                        }
                    }
                }

                return;
            }

            // If no JA actions match the buff, try for item usage

            string itemBuffName = string.Format("bottle of {0}", buffName.ToLower());
            var item = dataSet.Items.FirstOrDefault(a => a.ItemName == itemBuffName);

            if (item != null)
            {
                var itemActions = item.GetInteractionsRows();

                if ((itemActions != null) && (itemActions.Any()))
                {

                    var buffsByTarget = from i in itemActions
                                        where i.IsActorIDNull() == false &&
                                              i.Preparing == false
                                        let targetName = (i.IsTargetIDNull() == true) ?
                                            i.CombatantsRowByActorCombatantRelation.CombatantName :
                                            i.CombatantsRowByTargetCombatantRelation.CombatantName
                                        where playerList.Contains(targetName)
                                        group i by targetName;

                    if (buffsByTarget != null)
                    {
                        foreach (var targetBuffs in buffsByTarget)
                        {
                            // Should be faster than a full count, as it should succeed if anything exists.
                            if (targetBuffs.Any(a => a.InteractionID > 0))
                            {
                                var playerIntervalSet = playerIntervals.First(s => s.PlayerName == targetBuffs.Key);
                                TimeIntervalSet intervalSet = new TimeIntervalSet(buffName);

                                foreach (var buff in targetBuffs)
                                {
                                    intervalSet.Add(new TimeInterval(buff.Timestamp, duration));
                                }

                                playerIntervalSet.AddIntervalSet(intervalSet);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Compile durations for buffs that have a fixed duration, but may be overwritten
        /// by an alternate 'stance'.
        /// </summary>
        /// <param name="stanceBuffName">The stance we're checking for.</param>
        /// <param name="oppositeBuffName">The stance that may overwrite it.</param>
        /// <param name="duration">The max duration of the main stance.</param>
        /// <param name="playerIntervals">Player interval set to be added to.</param>
        /// <param name="dataSet">The database to pull info from.</param>
        static private void CompileStanceBuffs(string stanceBuffName, string oppositeBuffName, TimeSpan duration,
            List<string> playerList, List<PlayerTimeIntervalSets> playerIntervals,
            KPDatabaseDataSet dataSet)
        {
            if (string.IsNullOrEmpty(stanceBuffName))
                throw new ArgumentNullException("stanceBuffName");

            if (string.IsNullOrEmpty(oppositeBuffName))
                throw new ArgumentNullException("oppositeBuffName");

            if (duration < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("duration");

            if (playerList == null)
                throw new ArgumentNullException("playerList");

            if (playerIntervals == null)
                throw new ArgumentNullException("playerIntervals");

            if (dataSet == null)
                throw new ArgumentNullException("dataSet");

            var mainAction = dataSet.Actions.FirstOrDefault(a => a.ActionName == stanceBuffName);

            if (mainAction == null)
                return;

            var offAction = dataSet.Actions.FirstOrDefault(a => a.ActionName == oppositeBuffName);

            IOrderedEnumerable<KPDatabaseDataSet.InteractionsRow> actions;

            if (offAction == null)
            {
                actions = mainAction.GetInteractionsRows()
                    .OrderBy(a => a.InteractionID);
            }
            else
            {
                actions = mainAction.GetInteractionsRows()
                    .Concat(offAction.GetInteractionsRows())
                    .OrderBy(a => a.InteractionID);
            }


            var buffsByTarget = from i in actions
                                where i.IsActorIDNull() == false &&
                                      i.Preparing == false
                                let targetName = (i.IsTargetIDNull() == true) ?
                                    i.CombatantsRowByActorCombatantRelation.CombatantName :
                                    i.CombatantsRowByTargetCombatantRelation.CombatantName
                                where playerList.Contains(targetName)
                                group i by targetName;


            if (buffsByTarget != null)
            {
                foreach (var targetBuffs in buffsByTarget)
                {
                    // Should be faster than a full count, as it should succeed if anything exists.
                    if (targetBuffs.Any(a => a.InteractionID > 0))
                    {
                        var playerIntervalSet = playerIntervals.First(s => s.PlayerName == targetBuffs.Key);
                        TimeIntervalSet intervalSet = new TimeIntervalSet(stanceBuffName);

                        int last = targetBuffs.Count() - 1;

                        var usingBuffs = targetBuffs.Skip(0);

                        while (usingBuffs.Any())
                        {
                            var buff = usingBuffs.First();

                            if (buff.ActionsRow.ActionName == stanceBuffName)
                            {
                                var nextBuff = usingBuffs.Skip(1).FirstOrDefault();

                                if (nextBuff != null)
                                {
                                    if (nextBuff.Timestamp < (buff.Timestamp.Add(duration)))
                                    {
                                        intervalSet.Add(new TimeInterval(buff.Timestamp, nextBuff.Timestamp));
                                    }
                                    else
                                    {
                                        intervalSet.Add(new TimeInterval(buff.Timestamp, duration));
                                    }
                                }
                            }

                            usingBuffs = usingBuffs.Skip(1);
                        }

                        playerIntervalSet.AddIntervalSet(intervalSet);
                    }
                }
            }
        }

        /// <summary>
        /// Compile a list of durations for song buffs.  Songs stack up to 2 deep
        /// per bard, but otherwise overwrite previous song uses.  A third song by
        /// the same bard ends the duration of each checked song.
        /// </summary>
        /// <param name="songBuffName">The song being checked for.</param>
        /// <param name="anySongRegex">A regex to match any song.</param>
        /// <param name="duration">The default duration of the song.</param>
        /// <param name="playerIntervals">Player interval set to be added to.</param>
        /// <param name="dataSet">The database to pull info from.</param>
        static private void CompileSongBuffs(string songBuffName, string anySongRegex, TimeSpan duration,
            List<string> playerList, List<PlayerTimeIntervalSets> playerIntervals,
            KPDatabaseDataSet dataSet)
        {
            if (string.IsNullOrEmpty(songBuffName))
                throw new ArgumentNullException("songBuffName");

            if (string.IsNullOrEmpty(anySongRegex))
                throw new ArgumentNullException("anySongRegex");

            if (duration < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("duration");

            if (playerIntervals == null)
                throw new ArgumentNullException("playerIntervals");

            if (dataSet == null)
                throw new ArgumentNullException("dataSet");

            var song = dataSet.Actions.FirstOrDefault(a => a.ActionName == songBuffName);

            if (song == null)
                return;


            var allSongActions = from a in dataSet.Actions
                                 where Regex.Match(a.ActionName, anySongRegex).Success
                                 select a.GetInteractionsRows();

            List<KPDatabaseDataSet.InteractionsRow> iSongList = new List<KPDatabaseDataSet.InteractionsRow>();

            foreach (var actionSet in allSongActions)
            {
                iSongList.AddRange(actionSet);
            }

            var songsByTarget = from i in iSongList
                                where i.IsActorIDNull() == false &&
                                      i.IsTargetIDNull() == false &&
                                      i.Preparing == false
                                let targetName = i.CombatantsRowByTargetCombatantRelation.CombatantName
                                where playerList.Contains(targetName)
                                orderby i.Timestamp
                                group i by targetName;


            if (songsByTarget != null)
            {
                foreach (var target in songsByTarget)
                {
                    if (target.Any())
                    {
                        var playerIntervalSet = playerIntervals.First(s => s.PlayerName == target.Key);

                        TimeIntervalSet intervalSet = new TimeIntervalSet(songBuffName);

                        foreach (var targettedSong in target.Where(s => s.ActionsRow.ActionName == songBuffName))
                        {
                            DateTime endTime = targettedSong.Timestamp + duration;

                            var checkSongs = target.Where(s => (s.ActorID == targettedSong.ActorID) &&
                                (s.Timestamp > targettedSong.Timestamp) && (s.Timestamp < endTime))
                                .OrderBy(s => s.Timestamp);

                            if (checkSongs.Count() > 1)
                            {
                                endTime = checkSongs.Skip(1).First().Timestamp;
                            }

                            intervalSet.Add(new TimeInterval(targettedSong.Timestamp, endTime));
                        }

                        playerIntervalSet.AddIntervalSet(intervalSet);
                    }
                }
            }
        }

        /// <summary>
        /// Compile a list of durations for roll buffs.  Rolls stack up to 2 deep
        /// per corsair, but otherwise overwrite previous roll uses.  A third roll by
        /// the same corsair ends the duration of each checked song.
        /// Special consideration: Double-ups are marked as additional usage of that
        /// roll, and need to be excluded when determining end times.  Double-up
        /// can be used for 45 seconds after the initial roll.
        /// </summary>
        /// <param name="rollBuffName">The roll being checked for.</param>
        /// <param name="anyRollRegex">A regex to match any roll.</param>
        /// <param name="duration">The default duration of the roll.</param>
        /// <param name="playerIntervals">Player interval set to be added to.</param>
        /// <param name="dataSet">The database to pull info from.</param>
        static private void CompileRollBuffs(string rollBuffName, string anyRollRegex, TimeSpan duration,
            List<string> playerList, List<PlayerTimeIntervalSets> playerIntervals,
            KPDatabaseDataSet dataSet)
        {
            if (string.IsNullOrEmpty(rollBuffName))
                throw new ArgumentNullException("rollBuffName");

            if (string.IsNullOrEmpty(anyRollRegex))
                throw new ArgumentNullException("anyRollRegex");

            if (duration < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("duration");

            if (playerIntervals == null)
                throw new ArgumentNullException("playerIntervals");

            if (dataSet == null)
                throw new ArgumentNullException("dataSet");

            var roll = dataSet.Actions.FirstOrDefault(a => a.ActionName == rollBuffName);

            if (roll == null)
                return;

            var allRollActions = from a in dataSet.Actions
                                 where Regex.Match(a.ActionName, anyRollRegex).Success
                                 select a.GetInteractionsRows();

            List<KPDatabaseDataSet.InteractionsRow> iRollList = new List<KPDatabaseDataSet.InteractionsRow>();

            foreach (var actionSet in allRollActions)
            {
                iRollList.AddRange(actionSet);
            }

            var rollsByTarget = from i in iRollList
                                where i.IsActorIDNull() == false &&
                                      i.IsTargetIDNull() == false &&
                                      i.Preparing == false
                                let targetName = i.CombatantsRowByTargetCombatantRelation.CombatantName
                                where playerList.Contains(targetName)
                                orderby i.Timestamp
                                group i by targetName;


            if (rollsByTarget != null)
            {
                foreach (var target in rollsByTarget)
                {
                    if (target.Any())
                    {

                        var groupOrderedTargetRolls = target.
                            GroupAdjacentByTimeLimit<KPDatabaseDataSet.InteractionsRow, DateTime>(
                            i => i.Timestamp, TimeSpan.FromSeconds(45), TimeSpan.FromSeconds(45));

                        var playerIntervalSet = playerIntervals.First(s => s.PlayerName == target.Key);
                        TimeIntervalSet intervalSet = new TimeIntervalSet(rollBuffName);


                        foreach (var targettedRoll in groupOrderedTargetRolls
                            .Where(s => s.First().ActionsRow.ActionName == rollBuffName))
                        {
                            DateTime endTime = targettedRoll.First().Timestamp + duration;

                            var checkRolls = groupOrderedTargetRolls.Where(s =>
                                (s.First().ActorID == targettedRoll.First().ActorID) &&
                                (s.Key > targettedRoll.Last().Timestamp) &&
                                (s.Key < endTime));

                            if (checkRolls.Count() > 1)
                            {
                                endTime = checkRolls.Skip(1).First().Key;
                            }

                            intervalSet.Add(new TimeInterval(targettedRoll.Key, endTime));
                        }

                        playerIntervalSet.AddIntervalSet(intervalSet);
                    }
                }
            }
        }

        static private void CompileDebuffs(string debuffName, TimeSpan duration,
           List<string> playerList, List<PlayerTimeIntervalSets> playerIntervals,
           KPDatabaseDataSet dataSet)
        {
        }

        /// <summary>
        /// Compile a list of durations for debuffs on mobs.  These debuffs
        /// may be overridden by higher tier versions or complementary debuffs.
        /// EG: Dia/Dia II/Dia III/Bio/Bio II/Bio III.
        /// Since debuffs apply to all players acting on a mob, these only need to be
        /// calculated once for all selected players.
        /// </summary>
        /// <param name="debuffName">The name of the debuff to check for (exact).</param>
        /// <param name="overrideDebuffName">A regex specifying what debuffs can override the debuff.</param>
        /// <param name="duration">The base duration of the debuff.</param>
        /// <param name="playerList">The list of players to add these time intervals to.</param>
        /// <param name="playerIntervals">The interval sets to add the information to.</param>
        /// <param name="dataSet">The database to pull info from.</param>
        static private void CompileDebuffsWithOR(string debuffName, string overrideDebuffName, TimeSpan duration,
           List<string> playerList, List<PlayerTimeIntervalSets> playerIntervals, KPDatabaseDataSet dataSet)
        {
            if (string.IsNullOrEmpty(debuffName))
                throw new ArgumentNullException("debuffName");

            if (string.IsNullOrEmpty(overrideDebuffName))
                throw new ArgumentNullException("overrideDebuffName");

            if (duration < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("duration");

            if (playerIntervals == null)
                throw new ArgumentNullException("playerIntervals");

            if (dataSet == null)
                throw new ArgumentNullException("dataSet");

            var action = dataSet.Actions.FirstOrDefault(a => a.ActionName == debuffName);

            if (action == null)
                return;


            var allDebuffActions = from a in dataSet.Actions
                                   where a.ActionName == debuffName ||
                                         Regex.Match(a.ActionName, overrideDebuffName).Success
                                   select a.GetInteractionsRows();

            List<KPDatabaseDataSet.InteractionsRow> iDebuffList = new List<KPDatabaseDataSet.InteractionsRow>();

            foreach (var actionSet in allDebuffActions)
            {
                iDebuffList.AddRange(actionSet);
            }

            var debuffsByBattle = from d in iDebuffList
                                  where d.IsBattleIDNull() == false
                                  group d by d.BattleID into db
                                  select db;

            TimeIntervalSet intervalSet = new TimeIntervalSet(debuffName);
            DateTime prevDebuffUseTime;

            if ((debuffsByBattle != null) && (debuffsByBattle.Any()))
            {
                foreach (var battleSet in debuffsByBattle)
                {
                    prevDebuffUseTime = DateTime.MinValue;

                    foreach (var debuff in battleSet.Where(d => d.ActionsRow.ActionName == debuffName))
                    {
                        if (prevDebuffUseTime == DateTime.MinValue)
                            prevDebuffUseTime = debuff.Timestamp;

                        DateTime endTime = debuff.Timestamp + duration;

                        var endBattle = debuff.BattlesRow.EndTime;

                        if (endBattle < endTime)
                        {
                            if (endBattle < debuff.Timestamp)
                                endTime = debuff.Timestamp;
                            else
                                endTime = endBattle;
                        }

                        var overrideDebuff = battleSet.FirstOrDefault(d => d.Timestamp >= debuff.Timestamp &&
                            Regex.Match(d.ActionsRow.ActionName, overrideDebuffName).Success);

                        if (overrideDebuff != null)
                        {
                            if (overrideDebuff.Timestamp < endTime)
                                endTime = overrideDebuff.Timestamp;
                        }


                        intervalSet.Add(new TimeInterval(debuff.Timestamp, endTime));

                    }
                }
            }

            if (intervalSet.TimeIntervals.Count > 0)
            {
                foreach (var player in playerList)
                {
                    var playerIntervalSet = playerIntervals.First(s => s.PlayerName == player);

                    playerIntervalSet.AddIntervalSet(intervalSet);
                }
            }
        }

        static private void CompileSambaBuffs(string buffName, string overrideBuffs, TimeSpan duration,
           List<string> playerList, List<PlayerTimeIntervalSets> playerIntervals,
           KPDatabaseDataSet dataSet)
        {
            var allBuffActions = from a in dataSet.Actions
                                 where Regex.Match(a.ActionName, overrideBuffs).Success
                                 select a.GetInteractionsRows();

            List<KPDatabaseDataSet.InteractionsRow> iBuffList = new List<KPDatabaseDataSet.InteractionsRow>();

            foreach (var actionSet in allBuffActions)
            {
                iBuffList.AddRange(actionSet);
            }

            if (iBuffList.Count == 0)
                return;

            var sambaBuffList = iBuffList.OrderBy(a => a.Timestamp).ToList<KPDatabaseDataSet.InteractionsRow>();

            TimeIntervalSet intervalSet = new TimeIntervalSet(buffName);

            var sambaBuffs = from i in sambaBuffList
                             where i.ActionsRow.ActionName == buffName
                             select i;

            foreach (var samba in sambaBuffs)
            {
                DateTime endTime = samba.Timestamp + duration;

                var limitSamba = sambaBuffList.Find(b => b.Timestamp > samba.Timestamp && b.Timestamp < endTime);

                if (limitSamba != null)
                    endTime = limitSamba.Timestamp;

                intervalSet.Add(new TimeInterval(samba.Timestamp, endTime));
            }

            if (intervalSet.TimeIntervals.Count > 0)
            {
                foreach (var player in playerList)
                {
                    var playerIntervalSet = playerIntervals.First(s => s.PlayerName == player);

                    playerIntervalSet.AddIntervalSet(intervalSet);
                }
            }
        }

        #endregion
    }
}
