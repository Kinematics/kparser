﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WaywardGamers.KParser;
using WaywardGamers.KParser.Database;

namespace WaywardGamers.KParser.Plugin
{
    public class DefensePlugin : BasePluginControl
    {
        private class SpellNames
        {
            internal static readonly string UtsuIchi = "Utsusemi: Ichi";
            internal static readonly string UtsuNi = "Utsusemi: Ni";
        }


        #region Member Variables
        List<MainAccumulator> dataAccum = new List<MainAccumulator>();

        int totalDamage;
        List<string> playerList = new List<string>();
        Dictionary<string, int> playerDamage = new Dictionary<string, int>();

        string incAttacksHeader = "Player           Melee   Range   Abil/Ws   Spells   Unknown   Total   Attack# %   Avoided   Avoid %\n";

        //--
        
        string summaryHeader = "Player             Total Dmg   Damage %   Melee Dmg   Range Dmg   Abil. Dmg  WSkill Dmg   Spell Dmg  Other Dmg\n";
        string meleeHeader   = "Player             Melee Dmg   Melee %   Hit/Miss   M.Low/Hi    M.Avg  #Crit  C.Low/Hi   C.Avg     Crit%\n";
        string rangeHeader   = "Player             Range Dmg   Range %   Hit/Miss   R.Low/Hi    R.Avg  #Crit  C.Low/Hi   C.Avg     Crit%\n";
        string spellHeader   = "Player                  Spell Dmg   Spell %  #Spells  #Fail  S.Low/Hi     S.Avg  #MBurst  MB.Low/Hi   MB.Avg\n";
        string abilHeader    = "Player                  Abil. Dmg    Abil. %  Hit/Miss    A.Acc %    A.Low/Hi    A.Avg\n";
        string wskillHeader  = "Player                  WSkill Dmg   WSkill %  Hit/Miss   WS.Acc %   WS.Low/Hi   WS.Avg\n";
        string skillchainHeader = "Skillchain          SC Dmg  # SC  SC.Low/Hi  SC.Avg\n";
        string otherMHeader  = "Player             M.AE Dmg  # M.AE  M.AE Avg   R.AE Dmg  # R.AE  R.AE Avg   Spk.Dmg  # Spike  Spk.Avg\n";
        string otherPHeader  = "Player             CA.Dmg  CA.Hit/Miss  CA.Low/Hi  CA.Avg   Ret.Dmg  Ret.Hit/Miss  Ret.Low/Hi  Ret.Avg\n";

        string passiveDefHeader = "Player             Evasion  Evasion %   Parry  Parry %   Counter  Counter %   Intimidate  Intimidate %\n";
        string activeDefHeader  = "Player             Shadow  Shadow %   Anticipate  Anticipate %   Retaliations  Retaliation %\n";
        string utsuCastingheader = "Player             :Ichi Cast  :Ichi Finished    :Ni Cast  :Ni Finished    Casting Time (est.)\n";
        string shadowUseHeader   = "Player             Shadows Used    Shadows Cast  Shadows Cast(Nin)   Efficiency  Efficiency (Nin)\n";


        bool flagNoUpdate;
        bool groupMobs = true;
        bool exclude0XPMobs = false;
        ToolStripComboBox categoryCombo = new ToolStripComboBox();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();
        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();
        #endregion

        #region Constructor
        public DefensePlugin()
        {
            ToolStripLabel catLabel = new ToolStripLabel();
            catLabel.Text = "Category:";
            toolStrip.Items.Add(catLabel);

            categoryCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryCombo.Items.Add("All");
            categoryCombo.Items.Add("Damage Taken");
            categoryCombo.Items.Add("Defenses");
            categoryCombo.Items.Add("Utsusemi");
            categoryCombo.SelectedIndex = 0;
            categoryCombo.SelectedIndexChanged += new EventHandler(this.categoryCombo_SelectedIndexChanged);
            toolStrip.Items.Add(categoryCombo);


            ToolStripLabel mobsLabel = new ToolStripLabel();
            mobsLabel.Text = "Mobs:";
            toolStrip.Items.Add(mobsLabel);

            mobsCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            mobsCombo.AutoSize = false;
            mobsCombo.Width = 175;
            mobsCombo.Items.Add("All");
            mobsCombo.MaxDropDownItems = 10;
            mobsCombo.SelectedIndex = 0;
            mobsCombo.SelectedIndexChanged += new EventHandler(this.mobsCombo_SelectedIndexChanged);
            toolStrip.Items.Add(mobsCombo);


            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
            optionsMenu.Text = "Options";

            ToolStripMenuItem groupMobsOption = new ToolStripMenuItem();
            groupMobsOption.Text = "Group Mobs";
            groupMobsOption.CheckOnClick = true;
            groupMobsOption.Checked = true;
            groupMobsOption.Click += new EventHandler(groupMobs_Click);
            optionsMenu.DropDownItems.Add(groupMobsOption);

            ToolStripMenuItem exclude0XPOption = new ToolStripMenuItem();
            exclude0XPOption.Text = "Exclude 0 XP Mobs";
            exclude0XPOption.CheckOnClick = true;
            exclude0XPOption.Checked = false;
            exclude0XPOption.Click += new EventHandler(exclude0XPMobs_Click);
            optionsMenu.DropDownItems.Add(exclude0XPOption);

            toolStrip.Items.Add(optionsMenu);
        }
        #endregion

        #region IPlugin Overrides
        public override string TabName
        {
            get { return "Defense"; }
        }

        public override void Reset()
        {
            ResetTextBox();
            ResetAccumulation(false);
        }

        public override void NotifyOfUpdate()
        {
            ResetTextBox();

            UpdateMobList();

            flagNoUpdate = true;
            mobsCombo.CBSelectIndex(0);
            flagNoUpdate = false;

            ResetAccumulation(true);

            HandleDataset(null);
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if (e.DatasetChanges.Battles.Count > 0)
            {
                string selectedItem = mobsCombo.CBSelectedItem();
                UpdateMobList(true);

                flagNoUpdate = true;
                mobsCombo.CBSelectItem(selectedItem);
            }

            if (e.DatasetChanges.Interactions.Count != 0)
            {
                UpdateAccumulation(e.DatasetChanges);
                HandleDataset(null);
            }
        }
        #endregion

        #region Private Update functions
        private void UpdateMobList()
        {
            UpdateMobList(false);
            mobsCombo.CBSelectIndex(0);
        }

        private void UpdateMobList(bool overrideGrouping)
        {
            mobsCombo.CBReset();
            mobsCombo.CBAddStrings(GetMobListing(groupMobs, exclude0XPMobs));
        }

        private void ResetAccumulation(bool withUpdate)
        {
            dataAccum.Clear();

            if (withUpdate == true)
                UpdateAccumulation(null);
        }

        private void UpdateAccumulation(KPDatabaseDataSet datasetChanges)
        {
            if (datasetChanges == null)
            {
                using (AccessToTheDatabase db = new AccessToTheDatabase())
                {
                    UpdateAccumulationA(db.Database);
                }
            }
            else
            {
                UpdateAccumulationA(datasetChanges);
            }
        }

        private void UpdateAccumulationA(KPDatabaseDataSet dataSet)
        {
            MobFilter mobFilter = mobsCombo.CBGetMobFilter();

            #region LINQ queries

            IEnumerable<DefenseGroup2> defenseSet = null;

            defenseSet = from c in dataSet.Combatants
                         where ((c.CombatantType == (byte)EntityType.Player) ||
                                (c.CombatantType == (byte)EntityType.Pet) ||
                                (c.CombatantType == (byte)EntityType.Fellow))
                         orderby c.CombatantType, c.CombatantName
                         select new DefenseGroup2
                         {
                             Name = c.CombatantName,
                             ComType = (EntityType)c.CombatantType,
                             Melee = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                     where ((ActionType)n.ActionType == ActionType.Melee &&
                                            ((HarmType)n.HarmType == HarmType.Damage ||
                                             (HarmType)n.HarmType == HarmType.Drain)) &&
                                            mobFilter.CheckFilterMobActor(n)
                                     select n,
                             Range = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                     where ((ActionType)n.ActionType == ActionType.Ranged &&
                                            ((HarmType)n.HarmType == HarmType.Damage ||
                                             (HarmType)n.HarmType == HarmType.Drain)) &&
                                            mobFilter.CheckFilterMobActor(n)
                                     select n,
                             Spell = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                     where ((ActionType)n.ActionType == ActionType.Spell &&
                                            ((HarmType)n.HarmType == HarmType.Damage ||
                                             (HarmType)n.HarmType == HarmType.Drain) &&
                                             n.Preparing == false) &&
                                            mobFilter.CheckFilterMobActor(n)
                                     select n,
                             Ability = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                       where ((ActionType)n.ActionType == ActionType.Ability &&
                                            ((HarmType)n.HarmType == HarmType.Damage ||
                                             (HarmType)n.HarmType == HarmType.Drain ||
                                             (HarmType)n.HarmType == HarmType.Unknown) &&
                                             n.Preparing == false) &&
                                              mobFilter.CheckFilterMobActor(n)
                                       select n,
                             WSkill = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                      where ((ActionType)n.ActionType == ActionType.Weaponskill &&
                                            ((HarmType)n.HarmType == HarmType.Damage ||
                                             (HarmType)n.HarmType == HarmType.Drain) &&
                                             n.Preparing == false) &&
                                             mobFilter.CheckFilterMobActor(n)
                                      select n,
                             SC = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                  where ((ActionType)n.ActionType == ActionType.Skillchain &&
                                            ((HarmType)n.HarmType == HarmType.Damage ||
                                             (HarmType)n.HarmType == HarmType.Drain)) &&
                                         mobFilter.CheckFilterMobActor(n)
                                  select n,
                             Counter = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                       where (ActionType)n.ActionType == ActionType.Counterattack &&
                                              mobFilter.CheckFilterMobActor(n)
                                       select n,
                             Countered = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where (ActionType)n.ActionType == ActionType.Counterattack &&
                                                mobFilter.CheckFilterMobTarget(n)
                                         select n,
                             Retaliate = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where (ActionType)n.ActionType == ActionType.Retaliation &&
                                                mobFilter.CheckFilterMobActor(n)
                                         select n,
                             Spikes = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                      where (ActionType)n.ActionType == ActionType.Spikes &&
                                             mobFilter.CheckFilterMobActor(n)
                                      select n,
                             Unknown = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                       where ((HarmType)n.HarmType == HarmType.Damage ||
                                              (HarmType)n.HarmType == HarmType.Drain) &&
                                              (ActionType)n.ActionType == ActionType.Unknown &&
                                              mobFilter.CheckFilterMobActor(n) == true
                                       select n
                         };


            var utsuSet = from c in dataSet.Combatants
                          where ((c.CombatantType == (byte)EntityType.Player) ||
                                (c.CombatantType == (byte)EntityType.Pet) ||
                                (c.CombatantType == (byte)EntityType.Fellow))
                          orderby c.CombatantType, c.CombatantName
                          select new
                          {
                              Name = c.CombatantName,
                              ComType = (EntityType)c.CombatantType,
                              UtsuIchiCast = from s in c.GetInteractionsRowsByActorCombatantRelation()
                                             where ((ActionType)s.ActionType == ActionType.Spell &&
                                                    s.Preparing == true &&
                                                    s.IsActionIDNull() == false &&
                                                    s.ActionsRow.ActionName == SpellNames.UtsuIchi &&
                                                    mobFilter.CheckFilterMobBattle(s) == true)
                                             select s,
                              UtsuIchiFinish = from s in c.GetInteractionsRowsByActorCombatantRelation()
                                             where ((ActionType)s.ActionType == ActionType.Spell &&
                                                    s.Preparing == false &&
                                                    s.IsActionIDNull() == false &&
                                                    s.ActionsRow.ActionName == SpellNames.UtsuIchi &&
                                                    mobFilter.CheckFilterMobBattle(s) == true)
                                             select s,
                              UtsuNiCast = from s in c.GetInteractionsRowsByActorCombatantRelation()
                                             where ((ActionType)s.ActionType == ActionType.Spell &&
                                                    s.Preparing == true &&
                                                    s.IsActionIDNull() == false &&
                                                    s.ActionsRow.ActionName == SpellNames.UtsuNi &&
                                                    mobFilter.CheckFilterMobBattle(s) == true)
                                             select s,
                              UtsuNiFinish = from s in c.GetInteractionsRowsByActorCombatantRelation()
                                             where ((ActionType)s.ActionType == ActionType.Spell &&
                                                    s.Preparing == false &&
                                                    s.IsActionIDNull() == false &&
                                                    s.ActionsRow.ActionName == SpellNames.UtsuNi &&
                                                    mobFilter.CheckFilterMobBattle(s) == true)
                                             select s,

                          };
           #endregion

            if ((defenseSet == null) || (defenseSet.Count() == 0))
                return;

            int min, max;

            // Each player is the target of misc damage; total the amounts
            foreach (var player in defenseSet)
            {
                MainAccumulator playerAccum = dataAccum.FirstOrDefault(p => p.Name == player.Name);
                if (playerAccum == null)
                {
                    playerAccum = new MainAccumulator { Name = player.Name, CType = player.ComType };
                    dataAccum.Add(playerAccum);
                }

                #region Melee
                if (player.Melee.Count() > 0)
                {
                    playerAccum.TDmg += player.MeleeDmg;
                    playerAccum.TMDmg += player.MeleeDmg;

                    var succHits = player.Melee.Where(h => (DefenseType)h.DefenseType == DefenseType.None);
                    var critHits = succHits.Where(h => (DamageModifier)h.DamageModifier == DamageModifier.Critical);
                    var nonCritHits = succHits.Where(h => (DamageModifier)h.DamageModifier == DamageModifier.None);

                    if ((playerAccum.MHits == 0) && (nonCritHits.Count() > 0))
                    {
                        playerAccum.MHi = nonCritHits.First().Amount;
                        playerAccum.MLow = playerAccum.MHi;
                    }

                    if ((playerAccum.MCritHits == 0) && (critHits.Count() > 0))
                    {
                        playerAccum.MCritHi = critHits.First().Amount;
                        playerAccum.MCritLow = playerAccum.MCritHi;
                    }

                    if (nonCritHits.Count() > 0)
                    {
                        min = nonCritHits.Min(h => h.Amount);
                        max = nonCritHits.Max(h => h.Amount);

                        if (min < playerAccum.MLow)
                            playerAccum.MLow = min;
                        if (max > playerAccum.MHi)
                            playerAccum.MHi = max;
                    }

                    if (critHits.Count() > 0)
                    {
                        min = critHits.Min(h => h.Amount);
                        max = critHits.Max(h => h.Amount);

                        if (min < playerAccum.MCritLow)
                            playerAccum.MCritLow = min;
                        if (max > playerAccum.MCritHi)
                            playerAccum.MCritHi = max;
                    }

                    playerAccum.MHits += succHits.Count();
                    playerAccum.MMiss += player.Melee.Count(b => (DefenseType)b.DefenseType != DefenseType.None);

                    playerAccum.MCritHits += critHits.Count();
                    playerAccum.MCritDmg += critHits.Sum(h => h.Amount);

                    // Unsuccessful:
                    playerAccum.DefEvasion += player.Melee.Where(h => (DefenseType)h.DefenseType == DefenseType.Evasion).Count();
                    playerAccum.DefShadow += player.Melee.Where(h => (DefenseType)h.DefenseType == DefenseType.Shadow).Count();
                    playerAccum.DefParry += player.Melee.Where(h => (DefenseType)h.DefenseType == DefenseType.Parry).Count();
                    playerAccum.DefIntimidate += player.Melee.Where(h => (DefenseType)h.DefenseType == DefenseType.Intimidate).Count();
                    //playerAccum.DefCounter += player.Melee.Where(h => (DefenseType)h.DefenseType == DefenseType.Counter).Count();
                    playerAccum.DefCounter += player.Countered.Count();

                    playerAccum.UtsuUsed += player.Melee.Where(h => (DefenseType)h.DefenseType == DefenseType.Shadow).Sum(u => u.ShadowsUsed);
                }
                #endregion

                #region Range
                if (player.Range.Count() > 0)
                {
                    playerAccum.TDmg += player.RangeDmg;
                    playerAccum.TRDmg += player.RangeDmg;

                    var succHits = player.Range.Where(h => (DefenseType)h.DefenseType == DefenseType.None);
                    var critHits = succHits.Where(h => (DamageModifier)h.DamageModifier == DamageModifier.Critical);
                    var nonCritHits = succHits.Where(h => (DamageModifier)h.DamageModifier == DamageModifier.None);

                    if ((playerAccum.RHits == 0) && (nonCritHits.Count() > 0))
                    {
                        playerAccum.RHi = nonCritHits.First().Amount;
                        playerAccum.RLow = playerAccum.RHi;
                    }

                    if ((playerAccum.RCritHits == 0) && (critHits.Count() > 0))
                    {
                        playerAccum.RCritHi = critHits.First().Amount;
                        playerAccum.RCritLow = playerAccum.RCritHi;
                    }

                    if (nonCritHits.Count() > 0)
                    {
                        min = nonCritHits.Min(h => h.Amount);
                        max = nonCritHits.Max(h => h.Amount);

                        if (min < playerAccum.RLow)
                            playerAccum.RLow = min;
                        if (max > playerAccum.RHi)
                            playerAccum.RHi = max;
                    }

                    if (critHits.Count() > 0)
                    {
                        min = critHits.Min(h => h.Amount);
                        max = critHits.Max(h => h.Amount);

                        if (min < playerAccum.RCritLow)
                            playerAccum.RCritLow = min;
                        if (max > playerAccum.RCritHi)
                            playerAccum.RCritHi = max;
                    }

                    playerAccum.RHits += succHits.Count();
                    playerAccum.RMiss += player.Range.Count(b => (DefenseType)b.DefenseType != DefenseType.None);

                    playerAccum.RCritHits += critHits.Count();
                    playerAccum.RCritDmg += critHits.Sum(h => h.Amount);

                    // Unsuccessful:
                    playerAccum.DefEvasion += player.Range.Where(h => (DefenseType)h.DefenseType == DefenseType.Evasion).Count();
                }
                #endregion

                #region Ability
                if (player.Ability.Count() > 0)
                {
                    playerAccum.TDmg += player.AbilityDmg;
                    playerAccum.TADmg += player.AbilityDmg;

                    var abils = player.Ability.Where(a => a.IsActionIDNull() == false)
                        .GroupBy(a => a.ActionsRow.ActionName);

                    foreach (var abil in abils)
                    {
                        string abilName = abil.Key;

                        AbilAccum abilAcc = playerAccum.Abilities.FirstOrDefault(
                            a => a.AName == abilName);

                        if (abilAcc == null)
                        {
                            abilAcc = new AbilAccum { AName = abilName };
                            playerAccum.Abilities.Add(abilAcc);
                        }

                        var succAbil = abil.Where(a => (DefenseType)a.DefenseType == DefenseType.None);
                        var missAbil = abil.Where(a => (DefenseType)a.DefenseType != DefenseType.None);

                        if ((abilAcc.AHit == 0) && (succAbil.Count() > 0))
                        {
                            abilAcc.AHi = succAbil.First().Amount;
                            abilAcc.ALow = abilAcc.AHi;
                        }

                        if (succAbil.Count() > 0)
                        {
                            min = succAbil.Min(a => a.Amount);
                            max = succAbil.Max(a => a.Amount);

                            if (min < abilAcc.ALow)
                                abilAcc.ALow = min;
                            if (max > abilAcc.AHi)
                                abilAcc.AHi = max;
                        }

                        abilAcc.AHit += succAbil.Count();
                        abilAcc.AMiss += missAbil.Count();
                        abilAcc.ADmg += succAbil.Sum(a => a.Amount);

                        playerAccum.DefEvasion += abil.Where(h => (DefenseType)h.DefenseType == DefenseType.Evasion).Count();
                    }
                }
                #endregion

                #region Weaponskills
                if (player.WSkill.Count() > 0)
                {
                    playerAccum.TDmg += player.WSkillDmg;
                    playerAccum.TWDmg += player.WSkillDmg;

                    var wskills = player.WSkill.GroupBy(a => a.ActionsRow.ActionName);

                    foreach (var wskill in wskills)
                    {
                        string wskillName = wskill.Key;

                        WSAccum wskillAcc = playerAccum.Weaponskills.FirstOrDefault(
                            a => a.WName == wskillName);

                        if (wskillAcc == null)
                        {
                            wskillAcc = new WSAccum { WName = wskillName };
                            playerAccum.Weaponskills.Add(wskillAcc);
                        }

                        var succWS = wskill.Where(a => (DefenseType)a.DefenseType == DefenseType.None);
                        var missWS = wskill.Where(a => (DefenseType)a.DefenseType != DefenseType.None);

                        if ((wskillAcc.WHit == 0) && (succWS.Count() > 0))
                        {
                            wskillAcc.WHi = succWS.First().Amount;
                            wskillAcc.WLow = wskillAcc.WHi;
                        }

                        if (succWS.Count() > 0)
                        {
                            min = succWS.Min(a => a.Amount);
                            max = succWS.Max(a => a.Amount);

                            if (min < wskillAcc.WLow)
                                wskillAcc.WLow = min;
                            if (max > wskillAcc.WHi)
                                wskillAcc.WHi = max;
                        }

                        wskillAcc.WHit += succWS.Count();
                        wskillAcc.WMiss += missWS.Count();
                        wskillAcc.WDmg += succWS.Sum(a => a.Amount);

                        playerAccum.DefEvasion += wskill.Where(h => (DefenseType)h.DefenseType == DefenseType.Evasion).Count();
                    }
                }
                #endregion

                #region Spells
                if (player.Spell.Count() > 0)
                {
                    playerAccum.TDmg += player.SpellDmg;
                    playerAccum.TSDmg += player.SpellDmg;

                    var spells = player.Spell.GroupBy(a => a.ActionsRow.ActionName);

                    foreach (var spell in spells)
                    {
                        string spellName = spell.Key;

                        SpellAccum spellAcc = playerAccum.Spells.FirstOrDefault(
                            a => a.SName == spellName);

                        if (spellAcc == null)
                        {
                            spellAcc = new SpellAccum { SName = spellName };
                            playerAccum.Spells.Add(spellAcc);
                        }

                        var succSpell = spell.Where(a => (DefenseType)a.DefenseType == DefenseType.None);
                        var failSpell = spell.Where(a => (DefenseType)a.DefenseType == DefenseType.Resist);
                        var nonMBSpell = succSpell.Where(a => (DamageModifier)a.DamageModifier == DamageModifier.None);
                        var mbSpell = succSpell.Where(a => (DamageModifier)a.DamageModifier == DamageModifier.MagicBurst);

                        if ((spellAcc.SNum == 0) && (nonMBSpell.Count() > 0))
                        {
                            spellAcc.SHi = nonMBSpell.First().Amount;
                            spellAcc.SLow = spellAcc.SHi;
                        }

                        if ((spellAcc.SNumMB == 0) && (mbSpell.Count() > 0))
                        {
                            spellAcc.SMBHi = mbSpell.First().Amount;
                            spellAcc.SMBLow = spellAcc.SMBHi;
                        }

                        if (nonMBSpell.Count() > 0)
                        {
                            min = nonMBSpell.Min(a => a.Amount);
                            max = nonMBSpell.Max(a => a.Amount);

                            if (min < spellAcc.SLow)
                                spellAcc.SLow = min;
                            if (max > spellAcc.SHi)
                                spellAcc.SHi = max;
                        }

                        if (mbSpell.Count() > 0)
                        {
                            min = mbSpell.Min(a => a.Amount);
                            max = mbSpell.Max(a => a.Amount);

                            if (min < spellAcc.SMBLow)
                                spellAcc.SMBLow = min;
                            if (max > spellAcc.SMBHi)
                                spellAcc.SMBHi = max;
                        }

                        spellAcc.SNum += succSpell.Count();
                        spellAcc.SFail += failSpell.Count();
                        spellAcc.SNumMB += mbSpell.Count();
                        spellAcc.SDmg += succSpell.Sum(a => a.Amount);
                        spellAcc.SMBDmg += mbSpell.Sum(a => a.Amount);

                        playerAccum.DefIntimidate += spell.Where(h => (DefenseType)h.DefenseType == DefenseType.Intimidate).Count();
                    }
                }
                #endregion

                #region Other Magic
                if (player.MeleeEffect.Count() > 0)
                {
                    int dmg = player.MeleeEffect.Sum(a => a.SecondAmount);

                    playerAccum.MAENum += player.MeleeEffect.Count();
                    playerAccum.MAEDmg += dmg;
                    playerAccum.TODmg += dmg;
                    playerAccum.TDmg += dmg;
                }

                if (player.RangeEffect.Count() > 0)
                {
                    int dmg = player.RangeEffect.Sum(a => a.SecondAmount);

                    playerAccum.RAENum += player.RangeEffect.Count();
                    playerAccum.RAEDmg += dmg;
                    playerAccum.TODmg += dmg;
                    playerAccum.TDmg += dmg;
                }

                if (player.Spikes.Count() > 0)
                {
                    int dmg = player.Spikes.Sum(a => a.Amount);

                    playerAccum.SpkNum += player.Spikes.Count();
                    playerAccum.SpkDmg += dmg;
                    playerAccum.TODmg += dmg;
                    playerAccum.TDmg += dmg;
                }
                #endregion

                #region Other Physical
                if (player.Counter.Count() > 0)
                {
                    var succHits = player.Counter.Where(h => (DefenseType)h.DefenseType == DefenseType.None);

                    if ((playerAccum.CAHits == 0) && (succHits.Count() > 0))
                    {
                        playerAccum.CAHi = succHits.First().Amount;
                        playerAccum.CALow = playerAccum.CAHi;
                    }

                    if (succHits.Count() > 0)
                    {
                        min = succHits.Min(h => h.Amount);
                        max = succHits.Max(h => h.Amount);

                        if (min < playerAccum.CALow)
                            playerAccum.CALow = min;
                        if (max > playerAccum.CAHi)
                            playerAccum.CAHi = max;
                    }

                    playerAccum.CAHits += succHits.Count();
                    playerAccum.CAMiss += player.Counter.Count(b => (DefenseType)b.DefenseType != DefenseType.None);

                    int dmg = succHits.Sum(c => c.Amount);
                    playerAccum.CADmg += dmg;
                    playerAccum.TDmg += dmg;
                    playerAccum.TODmg += dmg;
                }

                if (player.Retaliate.Count() > 0)
                {
                    var succHits = player.Retaliate.Where(h => (DefenseType)h.DefenseType == DefenseType.None);

                    if ((playerAccum.RTHits == 0) && (succHits.Count() > 0))
                    {
                        playerAccum.RTHi = succHits.First().Amount;
                        playerAccum.RTLow = playerAccum.RTHi;
                    }

                    if (succHits.Count() > 0)
                    {
                        min = succHits.Min(h => h.Amount);
                        max = succHits.Max(h => h.Amount);

                        if (min < playerAccum.RTLow)
                            playerAccum.RTLow = min;
                        if (max > playerAccum.RTHi)
                            playerAccum.RTHi = max;
                    }

                    playerAccum.RTHits += succHits.Count();
                    playerAccum.RTMiss += player.Retaliate.Count(b => (DefenseType)b.DefenseType != DefenseType.None);

                    playerAccum.DefRetaliate = playerAccum.RTHits + playerAccum.RTMiss;

                    int dmg = succHits.Sum(c => c.Amount);
                    playerAccum.RTDmg += dmg;
                    playerAccum.TDmg += dmg;
                    playerAccum.TODmg += dmg;
                }
                #endregion

                #region Misc Defenses
                playerAccum.DefParry += player.Unknown.Where(h => (DefenseType)h.DefenseType == DefenseType.Parry).Count();
                playerAccum.DefAnticipate += player.Unknown.Where(h => (DefenseType)h.DefenseType == DefenseType.Anticipate).Count();
                playerAccum.DefShadow += player.Unknown.Where(h => (DefenseType)h.DefenseType == DefenseType.Shadow).Count();
                playerAccum.DefIntimidate += player.Unknown.Where(h => (DefenseType)h.DefenseType == DefenseType.Intimidate).Count();

                playerAccum.UtsuUsed += player.Unknown.Where(h => (DefenseType)h.DefenseType == DefenseType.Shadow).Sum(u => u.ShadowsUsed);
                #endregion

                #region Utsusemi counting
                var playerUtsu = utsuSet.SingleOrDefault(p => p.Name == player.Name && p.ComType == player.ComType);
                if (playerUtsu != null)
                {
                    playerAccum.UtsuICast = playerUtsu.UtsuIchiCast.Count();
                    playerAccum.UtsuIFin = playerUtsu.UtsuIchiFinish.Count();
                    playerAccum.UtsuNCast = playerUtsu.UtsuNiCast.Count();
                    playerAccum.UtsuNFin = playerUtsu.UtsuNiFinish.Count();
                }
                #endregion
            }
        }
        #endregion

        #region Processing Sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            ResetTextBox();
            string actionSourceFilter = categoryCombo.CBSelectedItem();

            List<StringMods> strModList = new List<StringMods>();
            StringBuilder sb = new StringBuilder();

            switch (actionSourceFilter)
            {
                case "All":
                    ProcessDamageTaken(ref sb, ref strModList);
                    ProcessDefenses(ref sb, ref strModList);
                    ProcessUtsusemi(ref sb, ref strModList);
                    break;
                case "Damage Taken":
                    ProcessDamageTaken(ref sb, ref strModList);
                    break;
                case "Defenses":
                    ProcessDefenses(ref sb, ref strModList);
                    break;
                case "Utsusemi":
                    ProcessUtsusemi(ref sb, ref strModList);
                    break;
            }

            PushStrings(sb, strModList);
        }

        #region Damage Taken
        private void ProcessDamageTaken(ref StringBuilder sb, ref List<StringMods> strModList)
        {
            string tmpText = "Damage Taken\n\n";
            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = tmpText.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(tmpText);

            ProcessDTAttackSummary(ref sb, ref strModList);
            ProcessDTMeleeAttacks(ref sb, ref strModList);
            ProcessDTRangedAttacks(ref sb, ref strModList);
            ProcessDTOtherAttacks(ref sb, ref strModList);
            ProcessDTWeaponskillAttacks(ref sb, ref strModList);
            ProcessDTAbilityAttacks(ref sb, ref strModList);
            ProcessDTSpellsAttacks(ref sb, ref strModList);
            ProcessDTSkillchains(ref sb, ref strModList);
        }

        private void ProcessDTAttackSummary(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            totalDamage = dataAccum.Sum(p => p.TDmg);

            if (totalDamage > 0)
            {
                string tmpText = "Damage Taken Summary\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.Append(tmpText);

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = summaryHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(summaryHeader);


                foreach (var player in dataAccum.OrderBy(p => p.CType).ThenBy(p => p.Name))
                {
                    if (player.TDmg > 0)
                    {
                        sb.AppendFormat("{0,-18}{1,10}{2,11:p2}{3,12}{4,12}{5,12}{6,12}{7,12}{8,11}\n",
                        player.Name,
                        player.TDmg,
                        (double)player.TDmg / totalDamage,
                        player.TMDmg,
                        player.TRDmg,
                        player.TADmg,
                        player.TWDmg,
                        player.TSDmg,
                        player.TODmg);
                    }
                }

                string strTotal =
                    string.Format("{0,-18}{1,10}{2,11:p2}{3,12}{4,12}{5,12}{6,12}{7,12}{8,11}\n",
                        "Total",
                        dataAccum.Sum(p => p.TDmg),
                        1,
                        dataAccum.Sum(p => p.TMDmg),
                        dataAccum.Sum(p => p.TRDmg),
                        dataAccum.Sum(p => p.TADmg),
                        dataAccum.Sum(p => p.TWDmg),
                        dataAccum.Sum(p => p.TSDmg),
                        dataAccum.Sum(p => p.TODmg));

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = strTotal.Length,
                    Bold = true,
                    Color = Color.Black
                });
                sb.Append(strTotal.ToString());

            }

            sb.Append("\n\n");
        }

        private void ProcessDTMeleeAttacks(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (dataAccum.Any(p => p.MHits > 0) ||
                dataAccum.Any(p => p.MMiss > 0))
            {
                string tmpText = "Melee Damage Taken\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.Append(tmpText);

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = meleeHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(meleeHeader);


                foreach (var player in dataAccum.OrderBy(p => p.CType).ThenBy(p => p.Name))
                {
                    if ((player.MHits + player.MMiss) > 0)
                    {
                        sb.AppendFormat("{0,-18}{1,10}{2,10:p2}{3,11}{5,11}{6,9:f2}{7,7}{8,10}{9,8:f2}{10,10:p2}\n",
                          player.Name,
                          player.TMDmg,
                          (player.TDmg > 0) ? (double)player.TMDmg / player.TDmg : 0,
                          string.Format("{0}/{1}", player.MHits, player.MMiss),
                          (double)player.MHits / (player.MHits + player.MMiss),
                          string.Format("{0}/{1}", player.MLow, player.MHi),
                          (player.MHits > player.MCritHits) ? (double)(player.TMDmg - player.MCritDmg) / (player.MHits - player.MCritHits) : 0,
                          player.MCritHits,
                          string.Format("{0}/{1}", player.MCritLow, player.MCritHi),
                          (player.MCritHits > 0) ? (double)player.MCritDmg / player.MCritHits : 0,
                          (player.MHits > 0) ? (double)player.MCritHits / player.MHits : 0);

                    }
                }

                sb.Append("\n\n");
            }
        }

        private void ProcessDTRangedAttacks(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (dataAccum.Any(p => p.RHits > 0) ||
                dataAccum.Any(p => p.RMiss > 0))
            {
                string tmpText = "Range Damage Taken\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.Append(tmpText);

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = meleeHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(rangeHeader);


                foreach (var player in dataAccum.OrderBy(p => p.CType).ThenBy(p => p.Name))
                {
                    if ((player.RHits + player.RMiss) > 0)
                    {
                        sb.AppendFormat("{0,-18}{1,10}{2,10:p2}{3,11}{5,11}{6,9:f2}{7,7}{8,10}{9,8:f2}{10,10:p2}\n",
                          player.Name,
                          player.TRDmg,
                          (player.TDmg > 0) ? (double)player.TRDmg / player.TDmg : 0,
                          string.Format("{0}/{1}", player.RHits, player.RMiss),
                          (double)player.RHits / (player.RHits + player.RMiss),
                          string.Format("{0}/{1}", player.RLow, player.RHi),
                          (player.RHits > player.RCritHits) ? (double)(player.TRDmg - player.RCritDmg) / (player.RHits - player.RCritHits) : 0,
                          player.RCritHits,
                          string.Format("{0}/{1}", player.RCritLow, player.RCritHi),
                          (player.RCritHits > 0) ? (double)player.RCritDmg / player.RCritHits : 0,
                          (player.RHits > 0) ? (double)player.RCritHits / player.RHits : 0);

                    }
                }

                sb.Append("\n\n");
            }
        }

        private void ProcessDTWeaponskillAttacks(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (dataAccum.Any(p => p.Weaponskills.Count > 0))
            {
                string tmpText = "Weaponskill Damage Taken\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.Append(tmpText);

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = wskillHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(wskillHeader);


                foreach (var player in dataAccum.OrderBy(p => p.CType).ThenBy(p => p.Name))
                {
                    if (player.Weaponskills.Count > 0)
                    {
                        sb.AppendFormat("{0,-23}{1,10}{2,11:p2}{3,10}{4,11:p2}{5,12}{6,9:f2}\n",
                             player.Name,
                             player.TWDmg,
                             (player.TDmg > 0) ? (double)player.TWDmg / player.TDmg : 0,
                             string.Format("{0}/{1}", player.Weaponskills.Sum(w => w.WHit), player.Weaponskills.Sum(w => w.WMiss)),
                             (double)player.Weaponskills.Sum(w => w.WHit) / player.Weaponskills.Sum(w => w.WHit + w.WMiss),
                             string.Format("{0}/{1}", player.Weaponskills.Min(w => w.WLow), player.Weaponskills.Max(w => w.WHi)),
                             player.Weaponskills.Any(w => w.WHit > 0) ? (double)player.TWDmg / player.Weaponskills.Sum(w => w.WHit) : 0);


                        foreach (var wskill in player.Weaponskills.OrderBy(w => w.WName))
                        {
                            sb.AppendFormat("{0,-23}{1,10}{2,11:p2}{3,10}{4,11:p2}{5,12}{6,9:f2}\n",
                                 string.Concat(" - ", wskill.WName),
                                 wskill.WDmg,
                                 (player.TWDmg > 0) ? (double)wskill.WDmg / player.TWDmg : 0,
                                 string.Format("{0}/{1}", wskill.WHit, wskill.WMiss),
                                 (wskill.WHit + wskill.WMiss) > 0 ? (double)wskill.WHit / (wskill.WHit + wskill.WMiss) : 0,
                                 string.Format("{0}/{1}", wskill.WLow, wskill.WHi),
                                 wskill.WHit > 0 ? (double)wskill.WDmg / wskill.WHit : 0);
                        }
                    }

                }

                sb.Append("\n\n");
            }
        }

        private void ProcessDTAbilityAttacks(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (dataAccum.Any(p => p.Abilities.Count > 0))
            {
                string tmpText = "Ability Damage Taken\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.Append(tmpText);

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = abilHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(abilHeader);


                foreach (var player in dataAccum.OrderBy(p => p.CType).ThenBy(p => p.Name))
                {
                    if (player.Abilities.Count > 0)
                    {
                        sb.AppendFormat("{0,-23}{1,10}{2,11:p2}{3,10}{4,11:p2}{5,12}{6,9:f2}\n",
                             player.Name,
                             player.TADmg,
                             (player.TDmg > 0) ? (double)player.TADmg / player.TDmg : 0,
                             string.Format("{0}/{1}", player.Abilities.Sum(w => w.AHit), player.Abilities.Sum(w => w.AMiss)),
                             (double)player.Abilities.Sum(w => w.AHit) / player.Abilities.Sum(w => w.AHit + w.AMiss),
                             player.Abilities.Sum(a => a.AHit) > 0 ?
                                string.Format("{0}/{1}", player.Abilities.Where(a => a.AHit > 0).Min(w => w.ALow), player.Abilities.Max(w => w.AHi)) :
                                string.Format("{0}/{1}", 0, 0),
                             player.Abilities.Any(w => w.AHit > 0) ? (double)player.TADmg / player.Abilities.Sum(w => w.AHit) : 0);


                        foreach (var abil in player.Abilities.OrderBy(w => w.AName))
                        {
                            sb.AppendFormat("{0,-23}{1,10}{2,11:p2}{3,10}{4,11:p2}{5,12}{6,9:f2}\n",
                                 string.Concat(" - ", abil.AName),
                                 abil.ADmg,
                                 (player.TADmg > 0) ? (double)abil.ADmg / player.TADmg : 0,
                                 string.Format("{0}/{1}", abil.AHit, abil.AMiss),
                                 (abil.AHit + abil.AMiss) > 0 ? (double)abil.AHit / (abil.AHit + abil.AMiss) : 0,
                                 string.Format("{0}/{1}", abil.ALow, abil.AHi),
                                 abil.AHit > 0 ? (double)abil.ADmg / abil.AHit : 0);
                        }
                    }

                }

                sb.Append("\n\n");
            }
        }

        private void ProcessDTSpellsAttacks(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {

            if (dataAccum.Any(p => p.Spells.Count > 0))
            {
                string tmpText = "Spell Damage Taken\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.Append(tmpText);

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = spellHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(spellHeader);


                foreach (var player in dataAccum.OrderBy(p => p.CType).ThenBy(p => p.Name))
                {
                    if (player.Spells.Count > 0)
                    {
                        sb.AppendFormat("{0,-23}{1,10}{2,10:p2}{3,9}{4,7}{5,10}{6,10:f2}{7,9}{8,11}{9,9:f2}\n",
                             player.Name,
                             player.TSDmg,
                             (player.TDmg > 0) ? (double)player.TSDmg / player.TDmg : 0,
                             player.Spells.Sum(s => s.SNum),
                             player.Spells.Sum(s => s.SFail),
                             string.Format("{0}/{1}", player.Spells.Min(w => w.SLow), player.Spells.Max(w => w.SHi)),
                             (player.Spells.Sum(s => s.SNum) > player.Spells.Sum(s => s.SNumMB)) ?
                                (double)(player.Spells.Sum(s => s.SDmg) - player.Spells.Sum(s => s.SMBDmg)) /
                                (player.Spells.Sum(s => s.SNum) - player.Spells.Sum(s => s.SNumMB)) : 0,
                             player.Spells.Sum(s => s.SNumMB),
                             player.Spells.Sum(s => s.SNumMB) > 0 ?
                                string.Format("{0}/{1}", player.Spells.Where(s => s.SNumMB > 0).Min(w => w.SMBLow), player.Spells.Max(w => w.SMBHi)) :
                                string.Format("{0}/{1}", 0, 0),
                             player.Spells.Any(w => w.SNumMB > 0) ? (double)player.Spells.Sum(s => s.SMBDmg) / player.Spells.Sum(w => w.SNumMB) : 0);


                        foreach (var spell in player.Spells.OrderBy(w => w.SName))
                        {
                            sb.AppendFormat("{0,-23}{1,10}{2,10:p2}{3,9}{4,7}{5,10}{6,10:f2}{7,9}{8,11}{9,9:f2}\n",
                                 string.Concat(" - ", spell.SName),
                                 spell.SDmg,
                                 (player.TSDmg > 0) ? (double)spell.SDmg / player.TSDmg : 0,
                                 spell.SNum,
                                 spell.SFail,
                                 string.Format("{0}/{1}", spell.SLow, spell.SHi),
                                 (spell.SNum > spell.SNumMB) ?
                                    (double)(spell.SDmg - spell.SMBDmg) / (spell.SNum - spell.SNumMB) : 0,
                                 spell.SNumMB,
                                 string.Format("{0}/{1}", spell.SMBLow, spell.SMBHi),
                                 spell.SNumMB > 0 ? (double)spell.SMBDmg / spell.SNumMB : 0);
                        }
                    }
                }

                sb.Append("\n\n");
            }
        }

        private void ProcessDTSkillchains(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (dataAccum.Any(p => (p.CType == EntityType.Skillchain) && (p.SCNum > 0)))
            {
                string tmpText = "Skillchain Damage Taken\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.Append(tmpText);

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = skillchainHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(skillchainHeader);


                foreach (var player in dataAccum.Where(p => p.CType == EntityType.Skillchain).OrderBy(p => p.Name))
                {
                    if (player.SCNum > 0)
                    {
                        sb.AppendFormat("{0,-20}{1,6}{2,6}{3,11}{4,8:f2}\n",
                             player.Name,
                             player.TSCDmg,
                             player.SCNum,
                             string.Format("{0}/{1}", player.SCLow, player.SCHi),
                             (player.SCNum > 0) ? (double)player.TSCDmg / player.SCNum : 0);

                    }
                }

                sb.Append("\n\n");
            }
        }

        private void ProcessDTOtherAttacks(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (dataAccum.Any(p => p.MAENum > 0 || p.RAENum > 0 || p.SpkNum > 0))
            {
                string tmpText = "Other Magical Damage Taken  (Additional Effects and Spikes)\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.Append(tmpText);

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = otherMHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(otherMHeader);


                foreach (var player in dataAccum.OrderBy(p => p.CType).ThenBy(p => p.Name))
                {
                    if ((player.MAENum + player.RAENum + player.SpkNum) > 0)
                    {
                        sb.AppendFormat("{0,-18}{1,9}{2,8}{3,10:f2}{4,11}{5,8}{6,10:f2}{7,10}{8,9}{9,9:f2}\n",
                            player.Name,
                            player.MAEDmg,
                            player.MAENum,
                            player.MAENum > 0 ? (double)player.MAEDmg / player.MAENum : 0,
                            player.RAEDmg,
                            player.RAENum,
                            player.RAENum > 0 ? (double)player.RAEDmg / player.RAENum : 0,
                            player.SpkDmg,
                            player.SpkNum,
                            player.SpkNum > 0 ? (double)player.SpkDmg / player.SpkNum : 0);

                    }
                }

                sb.Append("\n\n");
            }


            if (dataAccum.Any(p => (p.CAHits + p.CAMiss + p.RTHits + p.RTMiss) > 0))
            {
                string tmpText = "Other Physical Damage Taken  (Counterattacks and Retaliations)\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.Append(tmpText);

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = otherPHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(otherPHeader);


                foreach (var player in dataAccum.OrderBy(p => p.CType).ThenBy(p => p.Name))
                {
                    if ((player.CAHits + player.CAMiss + player.RTHits + player.RTMiss) > 0)
                    {
                        sb.AppendFormat("{0,-18}{1,7}{2,13}{3,11}{4,8:f2}{5,10}{6,14}{7,12}{8,9:f2}\n",
                            player.Name,
                            player.CADmg,
                            string.Concat(player.CAHits, "/", player.CAMiss),
                            string.Concat(player.CALow, "/", player.CAHi),
                            player.CAHits > 0 ? (double)player.CADmg / player.CAHits : 0,
                            player.RTDmg,
                            string.Concat(player.RTHits, "/", player.RTMiss),
                            string.Concat(player.RTLow, "/", player.RTHi),
                            player.RTHits > 0 ? (double)player.RTDmg / player.RTHits : 0);

                    }
                }

                sb.Append("\n\n");
            }
        }
        #endregion

        #region Defenses
        private void ProcessDefenses(ref StringBuilder sb, ref List<StringMods> strModList)
        {
            string tmpText = "Defenses\n\n";
            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = tmpText.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(tmpText);

            ProcessPassiveDefenses(ref sb, ref strModList);
            ProcessActiveDefenses(ref sb, ref strModList);
        }

        private void ProcessPassiveDefenses(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (dataAccum.Any(p => p.DefEvasion > 0 || p.DefParry > 0 || p.DefIntimidate > 0 ||
                (p.CAHits + p.CAMiss) > 0))
            {
                string tmpText = "Passive Defenses\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.Append(tmpText);

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = passiveDefHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(passiveDefHeader);

                int evaPool = 0;
                int parrPool = 0;
                int countPool = 0;
                int intimPool = 0;

                foreach (var player in dataAccum
                    .Where(p => p.DefEvasion > 0 || p.DefParry > 0 || p.DefCounter > 0 || p.DefIntimidate > 0)
                    .OrderBy(p => p.CType).ThenBy(p => p.Name))
                {
                    evaPool = player.MHits + player.RHits + player.DefCounter +
                        player.Abilities.Sum(a => a.AHit) + player.DefAnticipate +
                        player.DefShadow + player.DefParry + player.DefEvasion;

                    parrPool = player.MHits + player.DefCounter +
                        player.Abilities.Sum(a => a.AHit) + player.DefAnticipate +
                        player.DefShadow + player.DefParry;

                    countPool = player.MHits + player.DefCounter + player.DefAnticipate;

                    intimPool = (evaPool - player.RHits) + player.Spells.Sum(s => s.SNum);

                    sb.AppendFormat("{0,-18}{1,8}{2,11:p2}{3,8}{4,9:p2}{5,10}{6,11:p2}{7,13}{8,14:p2}\n",
                         player.Name,
                         player.DefEvasion,
                         evaPool > 0 ? (double)player.DefEvasion / evaPool : 0,
                         player.DefParry,
                         parrPool > 0 ? (double)player.DefParry / parrPool : 0,
                         player.DefCounter,
                         countPool > 0 ? (double)player.DefCounter / countPool : 0,
                         player.DefIntimidate,
                         intimPool > 0 ? (double)player.DefIntimidate / intimPool : 0);

                }

                sb.Append("\n\n");
            }
        }

        private void ProcessActiveDefenses(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (dataAccum.Any(p => p.DefShadow > 0 || p.DefAnticipate > 0 ||
                (p.RTHits + p.RTMiss) > 0))
            {
                string tmpText = "Active Defenses\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.Append(tmpText);

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = activeDefHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(activeDefHeader);

                int shadowPool = 0;
                int anticPool = 0;
                int retalPool = 0;

                foreach (var player in dataAccum
                    .Where(p => p.DefShadow > 0 || p.DefAnticipate > 0 || p.DefRetaliate > 0)
                    .OrderBy(p => p.CType).ThenBy(p => p.Name))
                {
                    shadowPool = player.MHits + player.RHits + player.DefCounter +
                        player.Spells.Sum(s => s.SNum) + player.Abilities.Sum(a => a.AHit) +
                        player.DefAnticipate + player.DefShadow;

                    anticPool = player.MHits + player.RHits + player.DefCounter +
                        player.Abilities.Sum(a => a.AHit) + player.DefAnticipate;

                    retalPool = player.MHits + player.DefCounter;

                    sb.AppendFormat("{0,-18}{1,7}{2,10:p2}{3,13}{4,14:p2}{5,15}{6,15:p2}\n",
                         player.Name,
                         player.DefShadow,
                         shadowPool > 0 ? (double)player.DefShadow / shadowPool : 0,
                         player.DefAnticipate,
                         anticPool > 0 ? (double)player.DefAnticipate / anticPool : 0,
                         player.DefRetaliate,
                         retalPool > 0 ? (double)player.DefRetaliate / retalPool : 0);

                }

                sb.Append("\n\n");
            }
        }

        #endregion

        #region Utsusemi
        private void ProcessUtsusemi(ref StringBuilder sb, ref List<StringMods> strModList)
        {
            string tmpText = "Utsusemi\n\n";
            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = tmpText.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(tmpText);

            if (dataAccum.Any(p => p.UtsuICast > 0 || p.UtsuNCast > 0 || p.UtsuUsed > 0))
            {
                tmpText = "Utsusemi Casting\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.Append(tmpText);

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = utsuCastingheader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(utsuCastingheader);

                TimeSpan totalCastTime = TimeSpan.MinValue;
                string totalCastTimeStr;

                foreach (var player in dataAccum
                    .Where(p => p.UtsuICast > 0 || p.UtsuNCast > 0)
                    .OrderBy(p => p.Name))
                {
                    totalCastTime = TimeSpan.FromSeconds(player.UtsuICast * 4.5 + player.UtsuNCast * 2);

                    if (totalCastTime.Hours > 0)
                        totalCastTimeStr = string.Format("{0}:{1,2:d2}:{2,2:d2}",
                            totalCastTime.Hours, totalCastTime.Minutes, totalCastTime.Seconds);
                    else
                        totalCastTimeStr = string.Format("{0}:{1,2:d2}",
                            totalCastTime.Minutes, totalCastTime.Seconds);


                    sb.AppendFormat("{0,-18}{1,11}{2,16}{3,12}{4,14}{5,22:f0}\n",
                         player.Name,
                         player.UtsuICast,
                         player.UtsuIFin,
                         player.UtsuNCast,
                         player.UtsuNFin,
                         totalCastTimeStr);
                }

                sb.Append("\n\n");


                tmpText = "Shadow Use\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.Append(tmpText);

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = shadowUseHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(shadowUseHeader);

                int utsuCast = 0;
                int utsuCastNin = 0;

                foreach (var player in dataAccum
                    .Where(p => p.UtsuUsed > 0)
                    .OrderBy(p => p.Name))
                {
                    utsuCast = player.UtsuIFin * 3 + player.UtsuNFin * 3;
                    utsuCastNin = player.UtsuIFin * 3 + player.UtsuNFin * 4;

                    sb.AppendFormat("{0,-18}{1,13}{2,16}{3,18}{4,14:p2}{5,17:p2}\n",
                         player.Name,
                         player.UtsuUsed,
                         utsuCast,
                         utsuCastNin,
                         utsuCast > 0 ? (double)player.UtsuUsed / utsuCast : 0,
                         utsuCastNin > 0 ? (double)player.UtsuUsed / utsuCastNin : 0);
                }

            }
        }
        #endregion

        #endregion

        #region Old processing sections
        private void ProcessDefenseAttacks(IEnumerable<DefenseGroup> incAttacks)
        {
            if (incAttacks.Count() == 0)
                return;

            AppendText("Attacks Against:\n", Color.Blue, true, false);
            AppendText(incAttacksHeader, Color.Black, true, true);

            StringBuilder sb = new StringBuilder();

            //"Player           Melee   Range   Abil/Ws   Spells   Unknown   Total   Attack# %   Avoided   Avoid %"

            int totalAttacks = incAttacks.Sum(b =>
                b.Melee.Count() + b.Range.Count() + b.Abil.Count() + b.Spell.Count() + b.Unknown.Count());

            foreach (var player in incAttacks)
            {
                int mHits = 0;
                int rHits = 0;
                int sHits = 0;
                int aHits = 0;
                int uHits = 0;
                int incHits = 0;
                int avoidHits = 0;

                double avoidPerc = 0;
                double attackPerc = 0;

                if (player.Melee != null)
                    mHits = player.Melee.Count();
                if (player.Range != null)
                    rHits = player.Range.Count();
                if (player.Abil != null)
                    aHits = player.Abil.Count();
                if (player.Spell != null)
                    sHits = player.Spell.Count();
                if (player.Unknown != null)
                    uHits = player.Unknown.Count();

                incHits = mHits + rHits + aHits + sHits + uHits;

                avoidHits = player.AllAttacks.Count(h => h.DefenseType != (byte)DefenseType.None);

                if (incHits > 0)
                {
                    if (incHits > 0)
                        avoidPerc = (double)avoidHits / incHits;

                    if (totalAttacks > 0)
                        attackPerc = (double)incHits / totalAttacks;


                    sb.Append(player.Name.PadRight(17));

                    sb.AppendFormat("{0,5}{1,8}{2,10}{3,9}{4,10}{5,8}{6,12:p2}{7,10}{8,10:p2}\n",
                        mHits, rHits, aHits, sHits, uHits, incHits, attackPerc, avoidHits, avoidPerc);
                }
            }

            sb.Append("\n\n");
            AppendText(sb.ToString());
        }
        #endregion

        #region Event Handlers
        protected void categoryCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(null);

            flagNoUpdate = false;
        }

        protected void mobsCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
            {
                ResetAccumulation(true);
                HandleDataset(null);
            }

            flagNoUpdate = false;
        }

        protected void groupMobs_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            groupMobs = sentBy.Checked;

            if (flagNoUpdate == false)
            {
                UpdateMobList();
                flagNoUpdate = true;
                mobsCombo.CBSelectIndex(0);

                ResetAccumulation(true);
                HandleDataset(null);
            }

            flagNoUpdate = false;
        }

        protected void exclude0XPMobs_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            exclude0XPMobs = sentBy.Checked;

            if (flagNoUpdate == false)
            {
                UpdateMobList();
                flagNoUpdate = true;
                mobsCombo.CBSelectIndex(0);

                ResetAccumulation(true);
                HandleDataset(null);
            }

            flagNoUpdate = false;
        }

        #endregion

    }
}
