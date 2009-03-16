using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using WaywardGamers.KParser;
using WaywardGamers.KParser.Database;

namespace WaywardGamers.KParser.Plugin
{
    public class OffensePlugin : BasePluginControl
    {
        #region Member Variables
        List<MainAccumulator> dataAccum = new List<MainAccumulator>();

        int totalDamage;
        IEnumerable<AttackGroup> attackSet = null;

        // Process calls always use the accumulator data.  To prevent the
        // BasePlugin from trying to get a lock on a new copy of the main
        // database each call, pass it a fake 'changed' database.
        KPDatabaseDataSet fakeDatabaseChanges = new KPDatabaseDataSet();

        string summaryHeader = "Player               Total Dmg   Damage %   Melee Dmg   Range Dmg   Abil. Dmg  WSkill Dmg   Spell Dmg  Other Dmg\n";
        string meleeHeader = "Player            Melee Dmg   Melee %   Hit/Miss   M.Acc %  M.Low/Hi    M.Avg  #Crit  C.Low/Hi   C.Avg     Crit%\n";
        string rangeHeader = "Player            Range Dmg   Range %   Hit/Miss   R.Acc %  R.Low/Hi    R.Avg  #Crit  C.Low/Hi   C.Avg     Crit%\n";
        string spellHeader = "Player                  Spell Dmg   Spell %  #Spells  #Fail  S.Low/Hi     S.Avg  #MBurst  MB.Low/Hi   MB.Avg\n";
        string abilHeader = "Player                  Abil. Dmg    Abil. %  Hit/Miss    A.Acc %    A.Low/Hi    A.Avg\n";
        string wskillHeader = "Player                 WSkill Dmg   WSkill %  Hit/Miss   WS.Acc %   WS.Low/Hi   WS.Avg\n";
        string skillchainHeader = "Skillchain          SC Dmg  # SC  SC.Low/Hi  SC.Avg\n";
        string otherMHeader = "Player            M.AE Dmg  # M.AE  M.AE Avg   R.AE Dmg  # R.AE  R.AE Avg   Spk.Dmg  # Spike  Spk.Avg\n";
        string otherPHeader = "Player            CA.Dmg  CA.Hit/Miss  CA.Low/Hi  CA.Avg   Ret.Dmg  Ret.Hit/Miss  Ret.Low/Hi  Ret.Avg\n";

        bool flagNoUpdate = false;
        bool groupMobs = true;
        bool exclude0XPMobs = false;

        ToolStripComboBox categoryCombo = new ToolStripComboBox();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();
        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();
        #endregion

        #region Constructor
        public OffensePlugin()
        {
            ToolStripLabel catLabel = new ToolStripLabel();
            catLabel.Text = "Category:";
            toolStrip.Items.Add(catLabel);

            categoryCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryCombo.Items.Add("All");
            categoryCombo.Items.Add("Summary");
            categoryCombo.Items.Add("Melee");
            categoryCombo.Items.Add("Ranged");
            categoryCombo.Items.Add("Other");
            categoryCombo.Items.Add("Weaponskill");
            categoryCombo.Items.Add("Ability");
            categoryCombo.Items.Add("Spell");
            categoryCombo.Items.Add("Skillchain");
            categoryCombo.MaxDropDownItems = 10;
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
            get { return "Offense"; }
        }

        public override void Reset()
        {
            ResetTextBox();
            ResetAccumulation();
        }

        public override void NotifyOfUpdate()
        {
            ResetTextBox();

            UpdateMobList();

            flagNoUpdate = true;
            mobsCombo.CBSelectIndex(0);
            flagNoUpdate = false;

            ResetAndUpdateAccumulation();

            HandleDataset(fakeDatabaseChanges);
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if (e.DatasetChanges.Battles != null)
            {
                if (e.DatasetChanges.Battles.Any(x => x.RowState == DataRowState.Added))
                {
                    string currentSelection = mobsCombo.CBSelectedItem();
                    if (currentSelection == string.Empty)
                        currentSelection = "All";

                    UpdateMobList();
                    flagNoUpdate = true;
                    mobsCombo.CBSelectItem(currentSelection);
                }
            }

            if (e.DatasetChanges.Interactions.Any(x => x.RowState == DataRowState.Added))
            {
                UpdateAccumulation(e.DatasetChanges);
                HandleDataset(fakeDatabaseChanges);
            }
        }
        #endregion

        #region Private functions
        private void UpdateMobList()
        {
            mobsCombo.UpdateWithMobList(groupMobs, exclude0XPMobs);
        }

        private void ResetAccumulation()
        {
            dataAccum.Clear();
        }

        private void ResetAndUpdateAccumulation()
        {
            ResetAccumulation();
            UpdateAccumulation(null);
        }

        private void UpdateAccumulation(KPDatabaseDataSet datasetChanges)
        {
            using (new RegionProfiler("offense: update accumulation"))
            {
                if (datasetChanges == null)
                {
                    using (AccessToTheDatabase db = new AccessToTheDatabase())
                    {
                        //UpdateAccumulationA(db.Database);
                        UpdateAccumulationB(db.Database, false);
                    }
                }
                else
                {
                    //UpdateAccumulationA(datasetChanges);
                    UpdateAccumulationB(datasetChanges, true);
                }
                
            }
        }

        private void UpdateAccumulationA(KPDatabaseDataSet dataSet)
        {
            MobFilter mobFilter = mobsCombo.CBGetMobFilter();

            #region LINQ query

            attackSet = from c in dataSet.Combatants
                        where (((EntityType)c.CombatantType == EntityType.Player) ||
                               ((EntityType)c.CombatantType == EntityType.Pet) ||
                               ((EntityType)c.CombatantType == EntityType.CharmedMob) ||
                               ((EntityType)c.CombatantType == EntityType.Fellow) ||
                               ((EntityType)c.CombatantType == EntityType.Skillchain))
                        orderby c.CombatantType, c.CombatantName
                        select new AttackGroup
                        {
                            Name = c.CombatantName,
                            ComType = (EntityType)c.CombatantType,
                            Melee = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                    where ((ActionType)n.ActionType == ActionType.Melee &&
                                           ((HarmType)n.HarmType == HarmType.Damage ||
                                            (HarmType)n.HarmType == HarmType.Drain)) &&
                                           mobFilter.CheckFilterMobTarget(n)
                                    select n,
                            Range = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                    where ((ActionType)n.ActionType == ActionType.Ranged &&
                                           ((HarmType)n.HarmType == HarmType.Damage ||
                                            (HarmType)n.HarmType == HarmType.Drain)) &&
                                           mobFilter.CheckFilterMobTarget(n)
                                    select n,
                            Spell = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                    where ((ActionType)n.ActionType == ActionType.Spell &&
                                           ((HarmType)n.HarmType == HarmType.Damage ||
                                            (HarmType)n.HarmType == HarmType.Drain) &&
                                            n.Preparing == false) &&
                                           mobFilter.CheckFilterMobTarget(n)
                                    select n,
                            Ability = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                      where ((ActionType)n.ActionType == ActionType.Ability &&
                                           ((HarmType)n.HarmType == HarmType.Damage ||
                                            (HarmType)n.HarmType == HarmType.Drain ||
                                            (HarmType)n.HarmType == HarmType.Unknown) &&
                                            n.Preparing == false) &&
                                             mobFilter.CheckFilterMobTarget(n)
                                      select n,
                            WSkill = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                     where ((ActionType)n.ActionType == ActionType.Weaponskill &&
                                           ((HarmType)n.HarmType == HarmType.Damage ||
                                            (HarmType)n.HarmType == HarmType.Drain) &&
                                            n.Preparing == false) &&
                                            mobFilter.CheckFilterMobTarget(n)
                                     select n,
                            SC = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                 where ((ActionType)n.ActionType == ActionType.Skillchain &&
                                           ((HarmType)n.HarmType == HarmType.Damage ||
                                            (HarmType)n.HarmType == HarmType.Drain)) &&
                                        mobFilter.CheckFilterMobTarget(n)
                                 select n,
                            Counter = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                      where (ActionType)n.ActionType == ActionType.Counterattack &&
                                             mobFilter.CheckFilterMobTarget(n)
                                      select n,
                            Retaliate = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where (ActionType)n.ActionType == ActionType.Retaliation &&
                                               mobFilter.CheckFilterMobTarget(n)
                                        select n,
                            Spikes = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                     where (ActionType)n.ActionType == ActionType.Spikes &&
                                            mobFilter.CheckFilterMobTarget(n)
                                     select n
                        };

            #endregion

            if ((attackSet == null) || (attackSet.Count() == 0))
                return;

            int min, max;

            foreach (var player in attackSet)
            {
                MainAccumulator mainAcc = dataAccum.FirstOrDefault(p => p.Name == player.Name);
                if (mainAcc == null)
                {
                    mainAcc = new MainAccumulator { Name = player.Name, CType = player.ComType };
                    dataAccum.Add(mainAcc);
                }

                if (player.ComType == EntityType.Skillchain)
                {
                    #region SC
                    if (player.SC.Count() > 0)
                    {
                        if (mainAcc.SCNum == 0)
                        {
                            mainAcc.SCHi = player.SC.First().Amount;
                            mainAcc.SCLow = player.SC.First().Amount;
                        }

                        mainAcc.TDmg += player.SCDmg;
                        mainAcc.TSCDmg += player.SCDmg;
                        mainAcc.SCNum += player.SC.Count();

                        min = player.SC.Min(sc => sc.Amount);
                        max = player.SC.Max(sc => sc.Amount);

                        if (min < mainAcc.SCLow)
                            mainAcc.SCLow = min;
                        if (max > mainAcc.SCHi)
                            mainAcc.SCHi = max;
                    }
                    #endregion
                }
                else
                {
                    #region Melee
                    if (player.Melee.Count() > 0)
                    {
                        mainAcc.TDmg += player.MeleeDmg;
                        mainAcc.TMDmg += player.MeleeDmg;

                        var succHits = player.Melee.Where(h => (DefenseType)h.DefenseType == DefenseType.None);
                        var critHits = succHits.Where(h => (DamageModifier)h.DamageModifier == DamageModifier.Critical);
                        var nonCritHits = succHits.Where(h => (DamageModifier)h.DamageModifier == DamageModifier.None);

                        if ((mainAcc.MHits == 0) && (nonCritHits.Count() > 0))
                        {
                            mainAcc.MHi = nonCritHits.First().Amount;
                            mainAcc.MLow = mainAcc.MHi;
                        }

                        if ((mainAcc.MCritHits == 0) && (critHits.Count() > 0))
                        {
                            mainAcc.MCritHi = critHits.First().Amount;
                            mainAcc.MCritLow = mainAcc.MCritHi;
                        }

                        if (nonCritHits.Count() > 0)
                        {
                            min = nonCritHits.Min(h => h.Amount);
                            max = nonCritHits.Max(h => h.Amount);

                            if (min < mainAcc.MLow)
                                mainAcc.MLow = min;
                            if (max > mainAcc.MHi)
                                mainAcc.MHi = max;
                        }

                        if (critHits.Count() > 0)
                        {
                            min = critHits.Min(h => h.Amount);
                            max = critHits.Max(h => h.Amount);

                            if (min < mainAcc.MCritLow)
                                mainAcc.MCritLow = min;
                            if (max > mainAcc.MCritHi)
                                mainAcc.MCritHi = max;
                        }

                        mainAcc.MHits += succHits.Count();
                        mainAcc.MMiss += player.Melee.Count(b => (DefenseType)b.DefenseType != DefenseType.None);

                        mainAcc.MCritHits += critHits.Count();
                        mainAcc.MCritDmg += critHits.Sum(h => h.Amount);
                    }
                    #endregion

                    #region Range
                    if (player.Range.Count() > 0)
                    {
                        mainAcc.TDmg += player.RangeDmg;
                        mainAcc.TRDmg += player.RangeDmg;

                        var succHits = player.Range.Where(h => (DefenseType)h.DefenseType == DefenseType.None);
                        var critHits = succHits.Where(h => (DamageModifier)h.DamageModifier == DamageModifier.Critical);
                        var nonCritHits = succHits.Where(h => (DamageModifier)h.DamageModifier == DamageModifier.None);

                        if ((mainAcc.RHits == 0) && (nonCritHits.Count() > 0))
                        {
                            mainAcc.RHi = nonCritHits.First().Amount;
                            mainAcc.RLow = mainAcc.RHi;
                        }

                        if ((mainAcc.RCritHits == 0) && (critHits.Count() > 0))
                        {
                            mainAcc.RCritHi = critHits.First().Amount;
                            mainAcc.RCritLow = mainAcc.RCritHi;
                        }

                        if (nonCritHits.Count() > 0)
                        {
                            min = nonCritHits.Min(h => h.Amount);
                            max = nonCritHits.Max(h => h.Amount);

                            if (min < mainAcc.RLow)
                                mainAcc.RLow = min;
                            if (max > mainAcc.RHi)
                                mainAcc.RHi = max;
                        }

                        if (critHits.Count() > 0)
                        {
                            min = critHits.Min(h => h.Amount);
                            max = critHits.Max(h => h.Amount);

                            if (min < mainAcc.RCritLow)
                                mainAcc.RCritLow = min;
                            if (max > mainAcc.RCritHi)
                                mainAcc.RCritHi = max;
                        }

                        mainAcc.RHits += succHits.Count();
                        mainAcc.RMiss += player.Range.Count(b => (DefenseType)b.DefenseType != DefenseType.None);

                        mainAcc.RCritHits += critHits.Count();
                        mainAcc.RCritDmg += critHits.Sum(h => h.Amount);
                    }
                    #endregion

                    #region Ability
                    if (player.Ability.Count() > 0)
                    {
                        mainAcc.TDmg += player.AbilityDmg;
                        mainAcc.TADmg += player.AbilityDmg;

                        var abils = player.Ability.Where(a => a.IsActionIDNull() == false)
                            .GroupBy(a => a.ActionsRow.ActionName);

                        foreach (var abil in abils)
                        {
                            string abilName = abil.Key;

                            AbilAccum abilAcc = mainAcc.Abilities.FirstOrDefault(
                                a => a.AName == abilName);

                            if (abilAcc == null)
                            {
                                abilAcc = new AbilAccum { AName = abilName };
                                mainAcc.Abilities.Add(abilAcc);
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
                        }
                    }
                    #endregion

                    #region Weaponskills
                    if (player.WSkill.Count() > 0)
                    {
                        mainAcc.TDmg += player.WSkillDmg;
                        mainAcc.TWDmg += player.WSkillDmg;

                        var wskills = player.WSkill.GroupBy(a => a.ActionsRow.ActionName);

                        foreach (var wskill in wskills)
                        {
                            string wskillName = wskill.Key;

                            WSAccum wskillAcc = mainAcc.Weaponskills.FirstOrDefault(
                                a => a.WName == wskillName);

                            if (wskillAcc == null)
                            {
                                wskillAcc = new WSAccum { WName = wskillName };
                                mainAcc.Weaponskills.Add(wskillAcc);
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
                        }
                    }
                    #endregion

                    #region Spells
                    if (player.Spell.Count() > 0)
                    {
                        mainAcc.TDmg += player.SpellDmg;
                        mainAcc.TSDmg += player.SpellDmg;

                        var spells = player.Spell.GroupBy(a => a.ActionsRow.ActionName);

                        foreach (var spell in spells)
                        {
                            string spellName = spell.Key;

                            SpellAccum spellAcc = mainAcc.Spells.FirstOrDefault(
                                a => a.SName == spellName);

                            if (spellAcc == null)
                            {
                                spellAcc = new SpellAccum { SName = spellName };
                                mainAcc.Spells.Add(spellAcc);
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
                        }
                    }
                    #endregion

                    #region Other Magic
                    if (player.MeleeEffect.Count() > 0)
                    {
                        int dmg = player.MeleeEffect.Sum(a => a.SecondAmount);

                        mainAcc.MAENum += player.MeleeEffect.Count();
                        mainAcc.MAEDmg += dmg;
                        mainAcc.TODmg += dmg;
                        mainAcc.TDmg += dmg;
                    }

                    if (player.RangeEffect.Count() > 0)
                    {
                        int dmg = player.RangeEffect.Sum(a => a.SecondAmount);

                        mainAcc.RAENum += player.RangeEffect.Count();
                        mainAcc.RAEDmg += dmg;
                        mainAcc.TODmg += dmg;
                        mainAcc.TDmg += dmg;
                    }

                    if (player.Spikes.Count() > 0)
                    {
                        int dmg = player.Spikes.Sum(a => a.Amount);

                        mainAcc.SpkNum += player.Spikes.Count();
                        mainAcc.SpkDmg += dmg;
                        mainAcc.TODmg += dmg;
                        mainAcc.TDmg += dmg;
                    }
                    #endregion

                    #region Other Physical
                    if (player.Counter.Count() > 0)
                    {
                        var succHits = player.Counter.Where(h => (DefenseType)h.DefenseType == DefenseType.None);

                        if ((mainAcc.CAHits == 0) && (succHits.Count() > 0))
                        {
                            mainAcc.CAHi = succHits.First().Amount;
                            mainAcc.CALow = mainAcc.CAHi;
                        }

                        if (succHits.Count() > 0)
                        {
                            min = succHits.Min(h => h.Amount);
                            max = succHits.Max(h => h.Amount);

                            if (min < mainAcc.CALow)
                                mainAcc.CALow = min;
                            if (max > mainAcc.CAHi)
                                mainAcc.CAHi = max;
                        }

                        mainAcc.CAHits += succHits.Count();
                        mainAcc.CAMiss += player.Counter.Count(b => (DefenseType)b.DefenseType != DefenseType.None);

                        int dmg = succHits.Sum(c => c.Amount);
                        mainAcc.CADmg += dmg;
                        mainAcc.TDmg += dmg;
                        mainAcc.TODmg += dmg;
                    }

                    if (player.Retaliate.Count() > 0)
                    {
                        var succHits = player.Retaliate.Where(h => (DefenseType)h.DefenseType == DefenseType.None);

                        if ((mainAcc.RTHits == 0) && (succHits.Count() > 0))
                        {
                            mainAcc.RTHi = succHits.First().Amount;
                            mainAcc.RTLow = mainAcc.RTHi;
                        }

                        if (succHits.Count() > 0)
                        {
                            min = succHits.Min(h => h.Amount);
                            max = succHits.Max(h => h.Amount);

                            if (min < mainAcc.RTLow)
                                mainAcc.RTLow = min;
                            if (max > mainAcc.RTHi)
                                mainAcc.RTHi = max;
                        }

                        mainAcc.RTHits += succHits.Count();
                        mainAcc.RTMiss += player.Retaliate.Count(b => (DefenseType)b.DefenseType != DefenseType.None);

                        int dmg = succHits.Sum(c => c.Amount);
                        mainAcc.RTDmg += dmg;
                        mainAcc.TDmg += dmg;
                        mainAcc.TODmg += dmg;
                    }
                    #endregion
                }
            }
        }

        private void UpdateAccumulationB(KPDatabaseDataSet dataSet, bool newRowsOnly)
        {
            MobFilter mobFilter = mobsCombo.CBGetMobFilter();

            #region LINQ query

            if (mobFilter.AllMobs == false)
            {
                // If we have any mob filter subset, get that data starting
                // with the battle table and working outwards.  Significantly
                // faster (eg: 5-25 ms instead of 400 ms on a 200 mob parse).

                var bSet = from b in dataSet.Battles
                           where (mobFilter.CheckFilterBattle(b) == true)
                           orderby b.BattleID
                           select b.GetInteractionsRows()
                                   .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added));

                if (bSet.Count() == 0)
                    return;

                IEnumerable<KPDatabaseDataSet.InteractionsRow> iRows = bSet.First();

                var bSetSkip = bSet.Skip(1);

                foreach (var b in bSetSkip)
                {
                    iRows = iRows.Concat(b);
                }


                attackSet = from c in iRows
                            where (c.IsActorIDNull() == false) &&
                                  ((EntityType)c.CombatantsRowByActorCombatantRelation.CombatantType == EntityType.Player ||
                                   (EntityType)c.CombatantsRowByActorCombatantRelation.CombatantType == EntityType.Pet ||
                                   (EntityType)c.CombatantsRowByActorCombatantRelation.CombatantType == EntityType.CharmedMob ||
                                   (EntityType)c.CombatantsRowByActorCombatantRelation.CombatantType == EntityType.Fellow ||
                                   (EntityType)c.CombatantsRowByActorCombatantRelation.CombatantType == EntityType.Skillchain)
                            group c by c.CombatantsRowByActorCombatantRelation into ca
                            orderby ca.Key.CombatantType, ca.Key.CombatantName
                            select new AttackGroup
                            {
                                Name = ca.Key.CombatantName,
                                ComType = (EntityType)ca.Key.CombatantType,
                                Melee = from q in ca
                                        where ((ActionType)q.ActionType == ActionType.Melee &&
                                               ((HarmType)q.HarmType == HarmType.Damage ||
                                                (HarmType)q.HarmType == HarmType.Drain))
                                        select q,
                                Range = from q in ca
                                        where ((ActionType)q.ActionType == ActionType.Ranged &&
                                               ((HarmType)q.HarmType == HarmType.Damage ||
                                                (HarmType)q.HarmType == HarmType.Drain))
                                        select q,
                                Spell = from q in ca
                                        where ((ActionType)q.ActionType == ActionType.Spell &&
                                               ((HarmType)q.HarmType == HarmType.Damage ||
                                                (HarmType)q.HarmType == HarmType.Drain) &&
                                                q.Preparing == false)
                                        select q,
                                Ability = from q in ca
                                          where ((ActionType)q.ActionType == ActionType.Ability &&
                                               ((HarmType)q.HarmType == HarmType.Damage ||
                                                (HarmType)q.HarmType == HarmType.Drain ||
                                                (HarmType)q.HarmType == HarmType.Unknown) &&
                                                q.Preparing == false)
                                          select q,
                                WSkill = from q in ca
                                         where ((ActionType)q.ActionType == ActionType.Weaponskill &&
                                               ((HarmType)q.HarmType == HarmType.Damage ||
                                                (HarmType)q.HarmType == HarmType.Drain) &&
                                                q.Preparing == false)
                                         select q,
                                SC = from q in ca
                                     where ((ActionType)q.ActionType == ActionType.Skillchain &&
                                               ((HarmType)q.HarmType == HarmType.Damage ||
                                                (HarmType)q.HarmType == HarmType.Drain))
                                     select q,
                                Counter = from q in ca
                                          where (ActionType)q.ActionType == ActionType.Counterattack
                                          select q,
                                Retaliate = from q in ca
                                            where (ActionType)q.ActionType == ActionType.Retaliation
                                            select q,
                                Spikes = from q in ca
                                         where (ActionType)q.ActionType == ActionType.Spikes
                                         select q
                            };
            }
            else
            {
                // Faster to process this from the combatant side if our mob filter is 'All'

                attackSet = from c in dataSet.Combatants
                            where (((EntityType)c.CombatantType == EntityType.Player) ||
                                   ((EntityType)c.CombatantType == EntityType.Pet) ||
                                   ((EntityType)c.CombatantType == EntityType.CharmedMob) ||
                                   ((EntityType)c.CombatantType == EntityType.Fellow) ||
                                   ((EntityType)c.CombatantType == EntityType.Skillchain))
                            orderby c.CombatantType, c.CombatantName
                            select new AttackGroup
                            {
                                Name = c.CombatantName,
                                ComType = (EntityType)c.CombatantType,
                                Melee = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                                   .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                        where ((ActionType)n.ActionType == ActionType.Melee &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain))
                                        select n,
                                Range = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                                   .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                        where ((ActionType)n.ActionType == ActionType.Ranged &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain))
                                        select n,
                                Spell = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                                   .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                        where ((ActionType)n.ActionType == ActionType.Spell &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.Preparing == false)
                                        select n,
                                Ability = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                                     .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                          where ((ActionType)n.ActionType == ActionType.Ability &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain ||
                                                (HarmType)n.HarmType == HarmType.Unknown) &&
                                                n.Preparing == false)
                                          select n,
                                WSkill = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                                    .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                         where ((ActionType)n.ActionType == ActionType.Weaponskill &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.Preparing == false)
                                         select n,
                                SC = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                                .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                     where ((ActionType)n.ActionType == ActionType.Skillchain &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain))
                                     select n,
                                Counter = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                                     .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                          where (ActionType)n.ActionType == ActionType.Counterattack
                                          select n,
                                Retaliate = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                                       .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                            where (ActionType)n.ActionType == ActionType.Retaliation
                                            select n,
                                Spikes = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                                    .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                         where (ActionType)n.ActionType == ActionType.Spikes
                                         select n
                            };
            }
            #endregion

            if ((attackSet == null) || (attackSet.Count() == 0))
                return;

            int min, max;

            foreach (var player in attackSet)
            {
                MainAccumulator mainAcc = dataAccum.FirstOrDefault(p => p.Name == player.Name);
                if (mainAcc == null)
                {
                    mainAcc = new MainAccumulator { Name = player.Name, CType = player.ComType };
                    dataAccum.Add(mainAcc);
                }

                if (player.ComType == EntityType.Skillchain)
                {
                    #region SC
                    if (player.SC.Count() > 0)
                    {
                        if (mainAcc.SCNum == 0)
                        {
                            mainAcc.SCHi = player.SC.First().Amount;
                            mainAcc.SCLow = player.SC.First().Amount;
                        }

                        mainAcc.TDmg += player.SCDmg;
                        mainAcc.TSCDmg += player.SCDmg;
                        mainAcc.SCNum += player.SC.Count();

                        min = player.SC.Min(sc => sc.Amount);
                        max = player.SC.Max(sc => sc.Amount);

                        if (min < mainAcc.SCLow)
                            mainAcc.SCLow = min;
                        if (max > mainAcc.SCHi)
                            mainAcc.SCHi = max;
                    }
                    #endregion
                }
                else
                {
                    #region Melee
                    if (player.Melee.Count() > 0)
                    {
                        mainAcc.TDmg += player.MeleeDmg;
                        mainAcc.TMDmg += player.MeleeDmg;

                        var succHits = player.Melee.Where(h => (DefenseType)h.DefenseType == DefenseType.None);
                        var critHits = succHits.Where(h => (DamageModifier)h.DamageModifier == DamageModifier.Critical);
                        var nonCritHits = succHits.Where(h => (DamageModifier)h.DamageModifier == DamageModifier.None);

                        if ((mainAcc.MHits == 0) && (nonCritHits.Count() > 0))
                        {
                            mainAcc.MHi = nonCritHits.First().Amount;
                            mainAcc.MLow = mainAcc.MHi;
                        }

                        if ((mainAcc.MCritHits == 0) && (critHits.Count() > 0))
                        {
                            mainAcc.MCritHi = critHits.First().Amount;
                            mainAcc.MCritLow = mainAcc.MCritHi;
                        }

                        if (nonCritHits.Count() > 0)
                        {
                            min = nonCritHits.Min(h => h.Amount);
                            max = nonCritHits.Max(h => h.Amount);

                            if (min < mainAcc.MLow)
                                mainAcc.MLow = min;
                            if (max > mainAcc.MHi)
                                mainAcc.MHi = max;
                        }

                        if (critHits.Count() > 0)
                        {
                            min = critHits.Min(h => h.Amount);
                            max = critHits.Max(h => h.Amount);

                            if (min < mainAcc.MCritLow)
                                mainAcc.MCritLow = min;
                            if (max > mainAcc.MCritHi)
                                mainAcc.MCritHi = max;
                        }

                        mainAcc.MHits += succHits.Count();
                        mainAcc.MMiss += player.Melee.Count(b => (DefenseType)b.DefenseType != DefenseType.None);

                        mainAcc.MCritHits += critHits.Count();
                        mainAcc.MCritDmg += critHits.Sum(h => h.Amount);
                    }
                    #endregion

                    #region Range
                    if (player.Range.Count() > 0)
                    {
                        mainAcc.TDmg += player.RangeDmg;
                        mainAcc.TRDmg += player.RangeDmg;

                        var succHits = player.Range.Where(h => (DefenseType)h.DefenseType == DefenseType.None);
                        var critHits = succHits.Where(h => (DamageModifier)h.DamageModifier == DamageModifier.Critical);
                        var nonCritHits = succHits.Where(h => (DamageModifier)h.DamageModifier == DamageModifier.None);

                        if ((mainAcc.RHits == 0) && (nonCritHits.Count() > 0))
                        {
                            mainAcc.RHi = nonCritHits.First().Amount;
                            mainAcc.RLow = mainAcc.RHi;
                        }

                        if ((mainAcc.RCritHits == 0) && (critHits.Count() > 0))
                        {
                            mainAcc.RCritHi = critHits.First().Amount;
                            mainAcc.RCritLow = mainAcc.RCritHi;
                        }

                        if (nonCritHits.Count() > 0)
                        {
                            min = nonCritHits.Min(h => h.Amount);
                            max = nonCritHits.Max(h => h.Amount);

                            if (min < mainAcc.RLow)
                                mainAcc.RLow = min;
                            if (max > mainAcc.RHi)
                                mainAcc.RHi = max;
                        }

                        if (critHits.Count() > 0)
                        {
                            min = critHits.Min(h => h.Amount);
                            max = critHits.Max(h => h.Amount);

                            if (min < mainAcc.RCritLow)
                                mainAcc.RCritLow = min;
                            if (max > mainAcc.RCritHi)
                                mainAcc.RCritHi = max;
                        }

                        mainAcc.RHits += succHits.Count();
                        mainAcc.RMiss += player.Range.Count(b => (DefenseType)b.DefenseType != DefenseType.None);

                        mainAcc.RCritHits += critHits.Count();
                        mainAcc.RCritDmg += critHits.Sum(h => h.Amount);
                    }
                    #endregion

                    #region Ability
                    if (player.Ability.Count() > 0)
                    {
                        mainAcc.TDmg += player.AbilityDmg;
                        mainAcc.TADmg += player.AbilityDmg;

                        var abils = player.Ability.Where(a => a.IsActionIDNull() == false)
                            .GroupBy(a => a.ActionsRow.ActionName);

                        foreach (var abil in abils)
                        {
                            string abilName = abil.Key;

                            AbilAccum abilAcc = mainAcc.Abilities.FirstOrDefault(
                                a => a.AName == abilName);

                            if (abilAcc == null)
                            {
                                abilAcc = new AbilAccum { AName = abilName };
                                mainAcc.Abilities.Add(abilAcc);
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
                        }
                    }
                    #endregion

                    #region Weaponskills
                    if (player.WSkill.Count() > 0)
                    {
                        mainAcc.TDmg += player.WSkillDmg;
                        mainAcc.TWDmg += player.WSkillDmg;

                        var wskills = player.WSkill.GroupBy(a => a.ActionsRow.ActionName);

                        foreach (var wskill in wskills)
                        {
                            string wskillName = wskill.Key;

                            WSAccum wskillAcc = mainAcc.Weaponskills.FirstOrDefault(
                                a => a.WName == wskillName);

                            if (wskillAcc == null)
                            {
                                wskillAcc = new WSAccum { WName = wskillName };
                                mainAcc.Weaponskills.Add(wskillAcc);
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
                        }
                    }
                    #endregion

                    #region Spells
                    if (player.Spell.Count() > 0)
                    {
                        mainAcc.TDmg += player.SpellDmg;
                        mainAcc.TSDmg += player.SpellDmg;

                        var spells = player.Spell.GroupBy(a => a.ActionsRow.ActionName);

                        foreach (var spell in spells)
                        {
                            string spellName = spell.Key;

                            SpellAccum spellAcc = mainAcc.Spells.FirstOrDefault(
                                a => a.SName == spellName);

                            if (spellAcc == null)
                            {
                                spellAcc = new SpellAccum { SName = spellName };
                                mainAcc.Spells.Add(spellAcc);
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
                        }
                    }
                    #endregion

                    #region Other Magic
                    if (player.MeleeEffect.Count() > 0)
                    {
                        int dmg = player.MeleeEffect.Sum(a => a.SecondAmount);

                        mainAcc.MAENum += player.MeleeEffect.Count();
                        mainAcc.MAEDmg += dmg;
                        mainAcc.TODmg += dmg;
                        mainAcc.TDmg += dmg;
                    }

                    if (player.RangeEffect.Count() > 0)
                    {
                        int dmg = player.RangeEffect.Sum(a => a.SecondAmount);

                        mainAcc.RAENum += player.RangeEffect.Count();
                        mainAcc.RAEDmg += dmg;
                        mainAcc.TODmg += dmg;
                        mainAcc.TDmg += dmg;
                    }

                    if (player.Spikes.Count() > 0)
                    {
                        int dmg = player.Spikes.Sum(a => a.Amount);

                        mainAcc.SpkNum += player.Spikes.Count();
                        mainAcc.SpkDmg += dmg;
                        mainAcc.TODmg += dmg;
                        mainAcc.TDmg += dmg;
                    }
                    #endregion

                    #region Other Physical
                    if (player.Counter.Count() > 0)
                    {
                        var succHits = player.Counter.Where(h => (DefenseType)h.DefenseType == DefenseType.None);

                        if ((mainAcc.CAHits == 0) && (succHits.Count() > 0))
                        {
                            mainAcc.CAHi = succHits.First().Amount;
                            mainAcc.CALow = mainAcc.CAHi;
                        }

                        if (succHits.Count() > 0)
                        {
                            min = succHits.Min(h => h.Amount);
                            max = succHits.Max(h => h.Amount);

                            if (min < mainAcc.CALow)
                                mainAcc.CALow = min;
                            if (max > mainAcc.CAHi)
                                mainAcc.CAHi = max;
                        }

                        mainAcc.CAHits += succHits.Count();
                        mainAcc.CAMiss += player.Counter.Count(b => (DefenseType)b.DefenseType != DefenseType.None);

                        int dmg = succHits.Sum(c => c.Amount);
                        mainAcc.CADmg += dmg;
                        mainAcc.TDmg += dmg;
                        mainAcc.TODmg += dmg;
                    }

                    if (player.Retaliate.Count() > 0)
                    {
                        var succHits = player.Retaliate.Where(h => (DefenseType)h.DefenseType == DefenseType.None);

                        if ((mainAcc.RTHits == 0) && (succHits.Count() > 0))
                        {
                            mainAcc.RTHi = succHits.First().Amount;
                            mainAcc.RTLow = mainAcc.RTHi;
                        }

                        if (succHits.Count() > 0)
                        {
                            min = succHits.Min(h => h.Amount);
                            max = succHits.Max(h => h.Amount);

                            if (min < mainAcc.RTLow)
                                mainAcc.RTLow = min;
                            if (max > mainAcc.RTHi)
                                mainAcc.RTHi = max;
                        }

                        mainAcc.RTHits += succHits.Count();
                        mainAcc.RTMiss += player.Retaliate.Count(b => (DefenseType)b.DefenseType != DefenseType.None);

                        int dmg = succHits.Sum(c => c.Amount);
                        mainAcc.RTDmg += dmg;
                        mainAcc.TDmg += dmg;
                        mainAcc.TODmg += dmg;
                    }
                    #endregion
                }
            }
        }

        #endregion

        #region Processing sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            // Database parameter is not used at all.  All processing is
            // based on the accumulator data.

            ResetTextBox();
            string actionSourceFilter = categoryCombo.CBSelectedItem();

            List<StringMods> strModList = new List<StringMods>();
            StringBuilder sb = new StringBuilder();

            switch (actionSourceFilter)
            {
                // Unknown == "All"
                case "All":
                    ProcessAttackSummary(ref sb, ref strModList);
                    ProcessMeleeAttacks(ref sb, ref strModList);
                    ProcessRangedAttacks(ref sb, ref strModList);
                    ProcessOtherAttacks(ref sb, ref strModList);
                    ProcessWeaponskillAttacks(ref sb, ref strModList);
                    ProcessAbilityAttacks(ref sb, ref strModList);
                    ProcessSpellsAttacks(ref sb, ref strModList);
                    ProcessSkillchains(ref sb, ref strModList);
                    break;
                case "Summary":
                    ProcessAttackSummary(ref sb, ref strModList);
                    break;
                case "Melee":
                    ProcessMeleeAttacks(ref sb, ref strModList);
                    break;
                case "Ranged":
                    ProcessRangedAttacks(ref sb, ref strModList);
                    break;
                case "Spell":
                    ProcessSpellsAttacks(ref sb, ref strModList);
                    break;
                case "Ability":
                    ProcessAbilityAttacks(ref sb, ref strModList);
                    break;
                case "Weaponskill":
                    ProcessWeaponskillAttacks(ref sb, ref strModList);
                    break;
                case "Skillchain":
                    ProcessSkillchains(ref sb, ref strModList);
                    break;
                case "Other":
                    ProcessOtherAttacks(ref sb, ref strModList);
                    break;
            }

            PushStrings(sb, strModList);
        }

        private void ProcessAttackSummary(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            totalDamage = dataAccum.Sum(p => p.TDmg);

            if (totalDamage > 0)
            {
                string tmpText = "Damage Summary\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Red
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
                        sb.AppendFormat("{0,-20}{1,10}{2,11:p2}{3,12}{4,12}{5,12}{6,12}{7,12}{8,11}\n",
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
                    string.Format("{0,-20}{1,10}{2,11:p2}{3,12}{4,12}{5,12}{6,12}{7,12}{8,11}\n",
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

        private void ProcessMeleeAttacks(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (dataAccum.Any(p => p.MHits > 0) ||
                dataAccum.Any(p => p.MMiss > 0))
            {
                string tmpText = "Melee Damage\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Red
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
                        sb.AppendFormat("{0,-17}{1,10}{2,10:p2}{3,11}{4,10:p2}{5,10}{6,9:f2}{7,7}{8,10}{9,8:f2}{10,10:p2}\n",
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

        private void ProcessRangedAttacks(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (dataAccum.Any(p => p.RHits > 0) ||
                dataAccum.Any(p => p.RMiss > 0))
            {
                string tmpText = "Range Damage\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Red
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
                        sb.AppendFormat("{0,-17}{1,10}{2,10:p2}{3,11}{4,10:p2}{5,10}{6,9:f2}{7,7}{8,10}{9,8:f2}{10,10:p2}\n",
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

        private void ProcessWeaponskillAttacks(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (dataAccum.Any(p => p.Weaponskills.Count > 0))
            {
                string tmpText = "Weaponskill Damage\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Red
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

        private void ProcessAbilityAttacks(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (dataAccum.Any(p => p.Abilities.Count > 0))
            {
                string tmpText = "Ability Damage\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Red
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

        private void ProcessSpellsAttacks(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {

            if (dataAccum.Any(p => p.Spells.Count > 0))
            {
                string tmpText = "Spell Damage\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Red
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

        private void ProcessSkillchains(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (dataAccum.Any(p => (p.CType == EntityType.Skillchain) && (p.SCNum > 0)))
            {
                string tmpText = "Skillchain Damage\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Red
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

        private void ProcessOtherAttacks(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (dataAccum.Any(p => p.MAENum > 0 || p.RAENum > 0 || p.SpkNum > 0))
            {
                string tmpText = "Other Magical Damage  (Additional Effects and Spikes)\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Red
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
                        sb.AppendFormat("{0,-17}{1,9}{2,8}{3,10:f2}{4,11}{5,8}{6,10:f2}{7,10}{8,9}{9,9:f2}\n",
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
                string tmpText = "Other Physical Damage  (Counterattacks and Retaliations)\n";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpText.Length,
                    Bold = true,
                    Color = Color.Red
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
                        sb.AppendFormat("{0,-17}{1,7}{2,13}{3,11}{4,8:f2}{5,10}{6,14}{7,12}{8,9:f2}\n",
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

        #region Event Handlers
        protected void categoryCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(fakeDatabaseChanges);

            flagNoUpdate = false;
        }

        protected void mobsCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
            {
                ResetAndUpdateAccumulation();
                HandleDataset(fakeDatabaseChanges);
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

                ResetAndUpdateAccumulation();
                HandleDataset(fakeDatabaseChanges);
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

                ResetAndUpdateAccumulation();
                HandleDataset(fakeDatabaseChanges);
            }

            flagNoUpdate = false;
        }
        #endregion
    }
}
