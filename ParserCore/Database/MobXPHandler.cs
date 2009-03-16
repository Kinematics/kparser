using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser.Database
{
    public class MobXPHandler
    {
        #region Data subclass
        public class MobXPValues
        {
            public int BattleID { get; set; }
            public string Name { get; set; }
            public int XP { get; set; }
            public int Chain { get; set; }
            public int BaseXP { get; set; }
        }
        #endregion

        #region Singleton Construction
        /// <summary>
        /// Make the class a singleton
        /// </summary>
        private static readonly MobXPHandler instance = new MobXPHandler();

        /// <summary>
        /// Gets the singleton instance of the NewMessageManager class.
        /// </summary>
        public static MobXPHandler Instance { get { return instance; } }
        
        /// <summary>
        /// Private constructor ensures singleton purity.
        /// </summary>
        private MobXPHandler()
		{
        }
        #endregion

        #region Member Variables
        Dictionary<int, MobXPValues> completeMobFightList = new Dictionary<int, MobXPValues>();
        Dictionary<int, MobXPValues> mobFightsThatEnded = new Dictionary<int, MobXPValues>();
        Dictionary<int, MobXPValues> mobFightsThatHaveNotEnded = new Dictionary<int, MobXPValues>();
        #endregion

        #region Public Methods
        public void Reset()
        {
            completeMobFightList.Clear();
            mobFightsThatEnded.Clear();
            mobFightsThatHaveNotEnded.Clear();
        }

        public void Update()
        {
            bool modified = false;
            int modifiedCount = 0;
            MobXPValues oneBattle = new MobXPValues();
            int difference;


            using (Database.AccessToTheDatabase dbAccess = new AccessToTheDatabase())
            {
                // If we don't have any data, or nothing has changed, short-circuit and leave

                if (dbAccess.Database.Battles.Count == 0)
                    return;

                var lastBattle = dbAccess.Database.Battles.Last();
                if (lastBattle.DefaultBattle == true)
                    return;

                if ((mobFightsThatEnded.Count > 0) && (mobFightsThatHaveNotEnded.Count == 0))
                {
                    MobXPValues lastDictionaryBattle = mobFightsThatEnded.Last().Value;

                    if (lastDictionaryBattle.BattleID == lastBattle.BattleID)
                        return;
                }

                // Ok, we have potential new data

                // First update any incomplete data

                if (mobFightsThatHaveNotEnded.Count > 0)
                {
                    List<int> clearedFights = new List<int>();

                    foreach (var incompleteFight in mobFightsThatHaveNotEnded)
                    {
                        var battle = dbAccess.Database.Battles.FindByBattleID(incompleteFight.Value.BattleID);

                        if (battle.IsOver == true)
                        {
                            modified = true;
                            modifiedCount++;
                            clearedFights.Add(incompleteFight.Key);

                            mobFightsThatEnded.Add(battle.BattleID, new MobXPValues()
                            {
                                BattleID = battle.BattleID,
                                Name = battle.CombatantsRowByEnemyCombatantRelation.CombatantName,
                                XP = battle.ExperiencePoints,
                                Chain = battle.ExperienceChain,
                                BaseXP = XPWithoutChain(battle.ExperiencePoints, battle.ExperienceChain)
                            });

                            oneBattle = mobFightsThatEnded[battle.BattleID];

                            if (completeMobFightList.ContainsKey(battle.BattleID))
                                completeMobFightList.Remove(battle.BattleID);

                            completeMobFightList.Add(battle.BattleID, mobFightsThatEnded[battle.BattleID]);
                        }
                    }

                    foreach (var clearedFight in clearedFights)
                        mobFightsThatHaveNotEnded.Remove(clearedFight);
                }

                // Next find any battles that we don't already have a record of

                var battles = from b in dbAccess.Database.Battles
                              where ((b.DefaultBattle == false) &&
                                     (b.IsEnemyIDNull() == false) &&
                                     ((EntityType)b.CombatantsRowByEnemyCombatantRelation.CombatantType == EntityType.Mob))
                              select b;

                foreach (var battle in battles)
                {
                    if (mobFightsThatEnded.ContainsKey(battle.BattleID) == false)
                    {
                        if (battle.IsOver == true)
                        {
                            modified = true;
                            modifiedCount++;

                            mobFightsThatEnded.Add(battle.BattleID, new MobXPValues()
                            {
                                BattleID = battle.BattleID,
                                Name = battle.CombatantsRowByEnemyCombatantRelation.CombatantName,
                                XP = battle.ExperiencePoints,
                                Chain = battle.ExperienceChain,
                                BaseXP = XPWithoutChain(battle.ExperiencePoints, battle.ExperienceChain)
                            });

                            oneBattle = mobFightsThatEnded[battle.BattleID];

                            completeMobFightList.Add(battle.BattleID, mobFightsThatEnded[battle.BattleID]);
                        }
                        else
                        {
                            mobFightsThatHaveNotEnded.Add(battle.BattleID, new MobXPValues()
                            {
                                BattleID = battle.BattleID,
                                Name = battle.CombatantsRowByEnemyCombatantRelation.CombatantName,
                                XP = 0,
                                Chain = 0,
                                BaseXP = 0
                            });

                            completeMobFightList.Add(battle.BattleID, mobFightsThatHaveNotEnded[battle.BattleID]);
                        }
                    }
                }


                // If only one new entry was added we only need to run the calculations
                // once.
                if ((modified == true) && (modifiedCount == 1))
                {
                    if (oneBattle.BaseXP != 0)
                    {
                        foreach (var fight in mobFightsThatEnded.Values)
                        {
                            if (fight.BattleID != oneBattle.BattleID)
                            {
                                difference = fight.BaseXP - oneBattle.BaseXP;

                                if (difference > 0)
                                {
                                    // get absolute value
                                    if (difference < 0)
                                        difference = difference * -1;

                                    if (difference <= 2)
                                    {
                                        if (fight.BaseXP > oneBattle.BaseXP)
                                            fight.BaseXP = oneBattle.BaseXP;
                                        else
                                            oneBattle.BaseXP = fight.BaseXP;
                                    }
                                }
                            }
                        }
                    }

                    return;
                }


                // If the main dictionary list has been modified, iterate over it
                // to recalculate min base xp values.

                if ((modified == true) && (mobFightsThatEnded.Count > 1))
                {
                    Dictionary<int, MobXPValues> remainingList = mobFightsThatEnded;

                    var keyList = mobFightsThatEnded.Keys;
                    var remainingKeys = keyList.Skip(0);

                    foreach (var mainFightKey in keyList)
                    {
                        var mainFight = mobFightsThatEnded[mainFightKey];
                        remainingKeys = remainingKeys.Skip(1);

                        foreach (var fightID in remainingKeys)
                        {
                            var checkFight = mobFightsThatEnded[fightID];

                            difference = mainFight.BaseXP - checkFight.BaseXP;

                            if (difference > 0)
                            {
                                // get absolute value
                                if (difference < 0)
                                    difference = difference * -1;

                                if (difference <= 2)
                                {
                                    if (mainFight.BaseXP > checkFight.BaseXP)
                                        mainFight.BaseXP = checkFight.BaseXP;
                                    else
                                        checkFight.BaseXP = mainFight.BaseXP;
                                }
                            }
                        }
                    }
                }
            }
        }

        public int GetBaseXP(int battleID)
        {
            return mobFightsThatEnded[battleID].BaseXP;
        }

        public Dictionary<int, MobXPValues>.ValueCollection CompleteMobList
        {
            get
            {
                return completeMobFightList.Values;
            }
        }
        #endregion

        #region Private helper functions
        private int XPWithoutChain(int experience, int chain)
        {
            if (experience == 0)
                return 0;

            if (chain == 0)
                return experience;

            double xpFactor;

            switch (chain)
            {
                case 1:
                    xpFactor = 1.20;
                    break;
                case 2:
                    xpFactor = 1.25;
                    break;
                case 3:
                    xpFactor = 1.30;
                    break;
                case 4:
                    xpFactor = 1.40;
                    break;
                case 5:
                default:
                    xpFactor = 1.50;
                    break;
            }

            double baseXP = Math.Ceiling((double)experience / xpFactor);

            return (int)baseXP;
        }

        #endregion
    }
}
