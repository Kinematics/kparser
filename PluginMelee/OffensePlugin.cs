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
using WaywardGamers.KParser.Interface;

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

        bool flagNoUpdate = false;
        bool groupMobs = true;
        bool exclude0XPMobs = false;
        bool customMobSelection = false;

        ToolStripLabel catLabel = new ToolStripLabel();
        ToolStripComboBox categoryCombo = new ToolStripComboBox();
        ToolStripLabel mobsLabel = new ToolStripLabel();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();

        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();
        ToolStripMenuItem groupMobsOption = new ToolStripMenuItem();
        ToolStripMenuItem exclude0XPOption = new ToolStripMenuItem();
        ToolStripMenuItem customMobSelectionOption = new ToolStripMenuItem();

        ToolStripButton editCustomMobFilter = new ToolStripButton();

        // Localized strings

        // Titles

        string lsSummaryTitle;
        string lsMeleeTitle;
        string lsMeleeCritTitle;
        string lsRangeTitle;
        string lsSpellTitle;
        string lsAbilityTitle;
        string lsWeaponskillTitle;
        string lsSkillchainTitle;
        string lsOtherPhysicalTitle;
        string lsOtherMagicalTitle;
        string lsCopySummaryTitle;

        // Headers

        string lsSummaryHeader;
        string lsMeleeHeader;
        string lsMeleeCritHeader;
        string lsRangeHeader;
        string lsSpellHeader;
        string lsAbilityHeader;
        string lsWeaponskillHeader;
        string lsSkillchainHeader;
        string lsOtherPhysicalHeader;
        string lsOtherMagicalHeader;

        // Formatters

        string lsSummaryFormat;
        string lsMeleeFormat;
        string lsMeleeCritFormat;
        string lsRangeFormat;
        string lsSpellFormat;
        string lsAbilityFormat;
        string lsWeaponskillFormat;
        string lsSkillchainFormat;
        string lsOtherPhysicalFormat;
        string lsOtherMagicalFormat;

        // Misc

        string lsTotal;
        #endregion

        #region Constructor
        public OffensePlugin()
        {
            LoadLocalizedUI();

            categoryCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryCombo.MaxDropDownItems = 10;
            categoryCombo.SelectedIndexChanged += new EventHandler(this.categoryCombo_SelectedIndexChanged);

            mobsCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            mobsCombo.AutoSize = false;
            mobsCombo.Width = 175;
            mobsCombo.MaxDropDownItems = 10;
            mobsCombo.SelectedIndexChanged += new EventHandler(this.mobsCombo_SelectedIndexChanged);


            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;

            groupMobsOption.CheckOnClick = true;
            groupMobsOption.Checked = true;
            groupMobsOption.Click += new EventHandler(groupMobs_Click);

            exclude0XPOption.CheckOnClick = true;
            exclude0XPOption.Checked = false;
            exclude0XPOption.Click += new EventHandler(exclude0XPMobs_Click);

            customMobSelectionOption.CheckOnClick = true;
            customMobSelectionOption.Checked = false;
            customMobSelectionOption.Click += new EventHandler(customMobSelection_Click);

            optionsMenu.DropDownItems.Add(groupMobsOption);
            optionsMenu.DropDownItems.Add(exclude0XPOption);
            optionsMenu.DropDownItems.Add(customMobSelectionOption);


            editCustomMobFilter.Enabled = false;
            editCustomMobFilter.Click += new EventHandler(editCustomMobFilter_Click);


            ToolStripSeparator aSeparator = new ToolStripSeparator();

            toolStrip.Items.Add(catLabel);
            toolStrip.Items.Add(categoryCombo);
            toolStrip.Items.Add(mobsLabel);
            toolStrip.Items.Add(mobsCombo);
            toolStrip.Items.Add(optionsMenu);
            toolStrip.Items.Add(aSeparator);
            toolStrip.Items.Add(editCustomMobFilter);
        }
        #endregion

        #region IPlugin Overrides
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
            if (datasetChanges == null)
            {
                using (AccessToTheDatabase db = new AccessToTheDatabase())
                {
                    if (db.HasAccess)
                    {
                        //UpdateAccumulationA(db.Database);
                        UpdateAccumulationB(db.Database, false);
                    }
                }
            }
            else
            {
                //UpdateAccumulationA(datasetChanges);
                UpdateAccumulationB(datasetChanges, true);
            }
        }

        private void UpdateAccumulationA(KPDatabaseDataSet dataSet)
        {
            MobFilter mobFilter = mobsCombo.CBGetMobFilter(exclude0XPMobs);

            #region LINQ query

            attackSet = from c in dataSet.Combatants
                        where (((EntityType)c.CombatantType == EntityType.Player) ||
                               ((EntityType)c.CombatantType == EntityType.Pet) ||
                               ((EntityType)c.CombatantType == EntityType.CharmedMob) ||
                               ((EntityType)c.CombatantType == EntityType.Fellow) ||
                               ((EntityType)c.CombatantType == EntityType.Skillchain))
                        orderby c.CombatantType, c.CombatantName
                        let actorActions = c.GetInteractionsRowsByActorCombatantRelation()
                        select new AttackGroup
                        {
                            Name = c.CombatantName,
                            ComType = (EntityType)c.CombatantType,
                            Melee = from n in actorActions
                                    where ((ActionType)n.ActionType == ActionType.Melee &&
                                           ((HarmType)n.HarmType == HarmType.Damage ||
                                            (HarmType)n.HarmType == HarmType.Drain)) &&
                                           mobFilter.CheckFilterMobTarget(n)
                                    select n,
                            Range = from n in actorActions
                                    where ((ActionType)n.ActionType == ActionType.Ranged &&
                                           ((HarmType)n.HarmType == HarmType.Damage ||
                                            (HarmType)n.HarmType == HarmType.Drain)) &&
                                           mobFilter.CheckFilterMobTarget(n)
                                    select n,
                            Spell = from n in actorActions
                                    where ((ActionType)n.ActionType == ActionType.Spell &&
                                           ((HarmType)n.HarmType == HarmType.Damage ||
                                            (HarmType)n.HarmType == HarmType.Drain) &&
                                            n.Preparing == false) &&
                                           mobFilter.CheckFilterMobTarget(n)
                                    select n,
                            Ability = from n in actorActions
                                      where ((ActionType)n.ActionType == ActionType.Ability &&
                                           ((HarmType)n.HarmType == HarmType.Damage ||
                                            (HarmType)n.HarmType == HarmType.Drain ||
                                            (HarmType)n.HarmType == HarmType.Unknown) &&
                                            n.Preparing == false) &&
                                             mobFilter.CheckFilterMobTarget(n)
                                      select n,
                            WSkill = from n in actorActions
                                     where ((ActionType)n.ActionType == ActionType.Weaponskill &&
                                           ((HarmType)n.HarmType == HarmType.Damage ||
                                            (HarmType)n.HarmType == HarmType.Drain) &&
                                            n.Preparing == false) &&
                                            mobFilter.CheckFilterMobTarget(n)
                                     select n,
                            SC = from n in actorActions
                                 where ((ActionType)n.ActionType == ActionType.Skillchain &&
                                           ((HarmType)n.HarmType == HarmType.Damage ||
                                            (HarmType)n.HarmType == HarmType.Drain)) &&
                                        mobFilter.CheckFilterMobTarget(n)
                                 select n,
                            Counter = from n in actorActions
                                      where (ActionType)n.ActionType == ActionType.Counterattack &&
                                             mobFilter.CheckFilterMobTarget(n)
                                      select n,
                            Retaliate = from n in actorActions
                                        where (ActionType)n.ActionType == ActionType.Retaliation &&
                                               mobFilter.CheckFilterMobTarget(n)
                                        select n,
                            Spikes = from n in actorActions
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
                    mainAcc = new MainAccumulator { Name = player.Name, DisplayName = player.DisplayName, CType = player.ComType };
                    dataAccum.Add(mainAcc);
                }

                if (player.ComType == EntityType.Skillchain)
                {
                    #region SC
                    if (player.SC.Any())
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
                    if (player.Melee.Any())
                    {
                        mainAcc.TDmg += player.MeleeDmg - player.AbsorbedMeleeDmg;
                        mainAcc.TMDmg += player.MeleeDmg - player.AbsorbedMeleeDmg;

                        var succHits = player.Melee.Where(h => (DefenseType)h.DefenseType == DefenseType.None);
                        var critHits = succHits.Where(h => (DamageModifier)h.DamageModifier == DamageModifier.Critical);
                        var nonCritHits = succHits.Where(h => (DamageModifier)h.DamageModifier == DamageModifier.None);

                        if ((mainAcc.MHits == 0) && (nonCritHits.Any()))
                        {
                            mainAcc.MHi = nonCritHits.First().Amount;
                            mainAcc.MLow = mainAcc.MHi;
                        }

                        if ((mainAcc.MCritHits == 0) && (critHits.Any()))
                        {
                            mainAcc.MCritHi = critHits.First().Amount;
                            mainAcc.MCritLow = mainAcc.MCritHi;
                        }

                        if (nonCritHits.Any())
                        {
                            min = nonCritHits.Min(h => h.Amount);
                            max = nonCritHits.Max(h => h.Amount);

                            if (min < mainAcc.MLow)
                                mainAcc.MLow = min;
                            if (max > mainAcc.MHi)
                                mainAcc.MHi = max;
                        }

                        if (critHits.Any())
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
                    if (player.Range.Any())
                    {
                        mainAcc.TDmg += player.RangeDmg;
                        mainAcc.TRDmg += player.RangeDmg;

                        var succHits = player.Range.Where(h => (DefenseType)h.DefenseType == DefenseType.None);
                        var critHits = succHits.Where(h => (DamageModifier)h.DamageModifier == DamageModifier.Critical);
                        var nonCritHits = succHits.Where(h => (DamageModifier)h.DamageModifier == DamageModifier.None);

                        if ((mainAcc.RHits == 0) && (nonCritHits.Any()))
                        {
                            mainAcc.RHi = nonCritHits.First().Amount;
                            mainAcc.RLow = mainAcc.RHi;
                        }

                        if ((mainAcc.RCritHits == 0) && (critHits.Any()))
                        {
                            mainAcc.RCritHi = critHits.First().Amount;
                            mainAcc.RCritLow = mainAcc.RCritHi;
                        }

                        if (nonCritHits.Any())
                        {
                            min = nonCritHits.Min(h => h.Amount);
                            max = nonCritHits.Max(h => h.Amount);

                            if (min < mainAcc.RLow)
                                mainAcc.RLow = min;
                            if (max > mainAcc.RHi)
                                mainAcc.RHi = max;
                        }

                        if (critHits.Any())
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
                    if (player.Ability.Any())
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

                            if ((abilAcc.AHit == 0) && (succAbil.Any()))
                            {
                                abilAcc.AHi = succAbil.First().Amount;
                                abilAcc.ALow = abilAcc.AHi;
                            }

                            if (succAbil.Any())
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
                    if (player.WSkill.Any())
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

                            if ((wskillAcc.WHit == 0) && (succWS.Any()))
                            {
                                wskillAcc.WHi = succWS.First().Amount;
                                wskillAcc.WLow = wskillAcc.WHi;
                            }

                            if (succWS.Any())
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
                    if (player.Spell.Any())
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

                            if ((spellAcc.SNum == 0) && (nonMBSpell.Any()))
                            {
                                spellAcc.SHi = nonMBSpell.First().Amount;
                                spellAcc.SLow = spellAcc.SHi;
                            }

                            if ((spellAcc.SNumMB == 0) && (mbSpell.Any()))
                            {
                                spellAcc.SMBHi = mbSpell.First().Amount;
                                spellAcc.SMBLow = spellAcc.SMBHi;
                            }

                            if (nonMBSpell.Any())
                            {
                                min = nonMBSpell.Min(a => a.Amount);
                                max = nonMBSpell.Max(a => a.Amount);

                                if (min < spellAcc.SLow)
                                    spellAcc.SLow = min;
                                if (max > spellAcc.SHi)
                                    spellAcc.SHi = max;
                            }

                            if (mbSpell.Any())
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
                    if (player.MeleeEffect.Any())
                    {
                        int dmg = player.MeleeEffect.Sum(a => a.SecondAmount);

                        mainAcc.MAENum += player.MeleeEffect.Count();
                        mainAcc.MAEDmg += dmg;
                        mainAcc.TODmg += dmg;
                        mainAcc.TDmg += dmg;
                    }

                    if (player.RangeEffect.Any())
                    {
                        int dmg = player.RangeEffect.Sum(a => a.SecondAmount);

                        mainAcc.RAENum += player.RangeEffect.Count();
                        mainAcc.RAEDmg += dmg;
                        mainAcc.TODmg += dmg;
                        mainAcc.TDmg += dmg;
                    }

                    if (player.Spikes.Any())
                    {
                        int dmg = player.Spikes.Sum(a => a.Amount);

                        mainAcc.SpkNum += player.Spikes.Count();
                        mainAcc.SpkDmg += dmg;
                        mainAcc.TODmg += dmg;
                        mainAcc.TDmg += dmg;
                    }
                    #endregion

                    #region Other Physical
                    if (player.Counter.Any())
                    {
                        var succHits = player.Counter.Where(h => (DefenseType)h.DefenseType == DefenseType.None
                            && h.Amount > 0);

                        if ((mainAcc.CAHits == 0) && (succHits.Any()))
                        {
                            mainAcc.CAHi = succHits.First().Amount;
                            mainAcc.CALow = mainAcc.CAHi;
                        }

                        if (succHits.Any())
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

                    if (player.Retaliate.Any())
                    {
                        var succHits = player.Retaliate.Where(h => (DefenseType)h.DefenseType == DefenseType.None
                            && h.Amount > 0);

                        if ((mainAcc.RTHits == 0) && (succHits.Any()))
                        {
                            mainAcc.RTHi = succHits.First().Amount;
                            mainAcc.RTLow = mainAcc.RTHi;
                        }

                        if (succHits.Any())
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
            if (dataSet == null)
                return;

            MobFilter mobFilter;
            if (customMobSelection)
                mobFilter = MobXPHandler.Instance.CustomMobFilter;
            else
                mobFilter = mobsCombo.CBGetMobFilter(exclude0XPMobs);

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
                                DisplayName = ca.Key.CombatantNameOrJobName,
                                ComType = (EntityType)ca.Key.CombatantType,
                                Melee = from q in ca
                                        where ((ActionType)q.ActionType == ActionType.Melee &&
                                               ((HarmType)q.HarmType == HarmType.Damage ||
                                                (HarmType)q.HarmType == HarmType.Drain ||
                                                (HarmType)q.HarmType == HarmType.Heal))
                                        select q,
                                Range = from q in ca
                                        where ((ActionType)q.ActionType == ActionType.Ranged &&
                                               ((HarmType)q.HarmType == HarmType.Damage ||
                                                (HarmType)q.HarmType == HarmType.Drain ||
                                                (HarmType)q.HarmType == HarmType.Heal))
                                        select q,
                                Spell = from q in ca
                                        where ((ActionType)q.ActionType == ActionType.Spell &&
                                               ((HarmType)q.HarmType == HarmType.Damage ||
                                                (HarmType)q.HarmType == HarmType.Drain ||
                                                (HarmType)q.HarmType == HarmType.Heal) &&
                                                q.Preparing == false)
                                        select q,
                                Ability = from q in ca
                                          where ((ActionType)q.ActionType == ActionType.Ability &&
                                               ((HarmType)q.HarmType == HarmType.Damage ||
                                                (HarmType)q.HarmType == HarmType.Drain ||
                                                (HarmType)q.HarmType == HarmType.Heal ||
                                                (HarmType)q.HarmType == HarmType.Unknown) &&
                                                q.Preparing == false)
                                          select q,
                                WSkill = from q in ca
                                         where ((ActionType)q.ActionType == ActionType.Weaponskill &&
                                               ((HarmType)q.HarmType == HarmType.Damage ||
                                                (HarmType)q.HarmType == HarmType.Drain ||
                                                (HarmType)q.HarmType == HarmType.Heal) &&
                                                q.Preparing == false)
                                         select q,
                                SC = from q in ca
                                     where ((ActionType)q.ActionType == ActionType.Skillchain &&
                                               ((HarmType)q.HarmType == HarmType.Damage ||
                                                (HarmType)q.HarmType == HarmType.Drain ||
                                                (HarmType)q.HarmType == HarmType.Heal))
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
                            let actorActions = c.GetInteractionsRowsByActorCombatantRelation()
                                    .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                            select new AttackGroup
                            {
                                Name = c.CombatantName,
                                DisplayName = c.CombatantNameOrJobName,
                                ComType = (EntityType)c.CombatantType,
                                Melee = from n in actorActions
                                        where ((ActionType)n.ActionType == ActionType.Melee &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain ||
                                                (HarmType)n.HarmType == HarmType.Heal))
                                        select n,
                                Range = from n in actorActions
                                        where ((ActionType)n.ActionType == ActionType.Ranged &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain ||
                                                (HarmType)n.HarmType == HarmType.Heal))
                                        select n,
                                Spell = from n in actorActions
                                        where ((ActionType)n.ActionType == ActionType.Spell &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain ||
                                                (HarmType)n.HarmType == HarmType.Heal) &&
                                                n.Preparing == false)
                                        select n,
                                Ability = from n in actorActions
                                          where ((ActionType)n.ActionType == ActionType.Ability &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain ||
                                                (HarmType)n.HarmType == HarmType.Heal ||
                                                (HarmType)n.HarmType == HarmType.Unknown) &&
                                                n.Preparing == false)
                                          select n,
                                WSkill = from n in actorActions
                                         where ((ActionType)n.ActionType == ActionType.Weaponskill &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain ||
                                                (HarmType)n.HarmType == HarmType.Heal) &&
                                                n.Preparing == false)
                                         select n,
                                SC = from n in actorActions
                                     where ((ActionType)n.ActionType == ActionType.Skillchain &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain ||
                                                (HarmType)n.HarmType == HarmType.Heal))
                                     select n,
                                Counter = from n in actorActions
                                          where (ActionType)n.ActionType == ActionType.Counterattack
                                          select n,
                                Retaliate = from n in actorActions
                                            where (ActionType)n.ActionType == ActionType.Retaliation
                                            select n,
                                Spikes = from n in actorActions
                                         where (ActionType)n.ActionType == ActionType.Spikes
                                         select n
                            };
            }
            #endregion

            if ((attackSet == null) || (!attackSet.Any()))
                return;

            int min, max;

            foreach (var player in attackSet)
            {
                MainAccumulator mainAcc = dataAccum.FirstOrDefault(p => p.Name == player.Name);
                if (mainAcc == null)
                {
                    mainAcc = new MainAccumulator { Name = player.Name, DisplayName = player.DisplayName, CType = player.ComType };
                    dataAccum.Add(mainAcc);
                }

                if (player.ComType == EntityType.Skillchain)
                {
                    #region SC
                    if (player.SC.Any())
                    {
                        if (mainAcc.SCNum == 0)
                        {
                            mainAcc.SCHi = player.SC.First().Amount;
                            mainAcc.SCLow = player.SC.First().Amount;
                        }

                        mainAcc.TDmg += player.SCDmg + player.AbsorbedSCDmg;
                        mainAcc.SCDmg += player.SCDmg;
                        mainAcc.TSCDmg += player.SCDmg + player.AbsorbedSCDmg;
                        mainAcc.TAbsSCDmg += player.AbsorbedSCDmg;
                        mainAcc.TAbsDmg += mainAcc.TAbsSCDmg;
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
                    if (player.Melee.Any())
                    {
                        var groupHits = from m in player.Melee
                                     group m by (DefenseType)m.DefenseType;

                        foreach (var hitType in groupHits)
                        {
                            switch (hitType.Key)
                            {
                                case DefenseType.None:
                                    var groupCrits = from m in hitType
                                                     group m by (DamageModifier)m.DamageModifier;

                                    int count;
                                    int meleeDmg;
                                    int zeroCount;

                                    foreach (var critType in groupCrits)
                                    {
                                        switch (critType.Key)
                                        {
                                            case DamageModifier.Critical:
                                                count = critType.Count();
                                                mainAcc.MHits += count;
                                                mainAcc.MCritHits += count;
                                                mainAcc.MNonEvaded += count;

                                                int critDmg = critType.Sum(m => m.Amount);

                                                mainAcc.MDmg += critDmg;
                                                mainAcc.TDmg += critDmg;
                                                mainAcc.TMDmg += critDmg;
                                                mainAcc.MCritDmg += critDmg;

                                                min = critType.First().Amount;
                                                max = min;
                                                zeroCount = 0;

                                                foreach (var crit in critType)
                                                {
                                                    if (crit.Amount < min)
                                                        min = crit.Amount;
                                                    if (crit.Amount > max)
                                                        max = crit.Amount;
                                                    if (crit.Amount == 0)
                                                        zeroCount++;
                                                }

                                                mainAcc.MCritLow = min;
                                                mainAcc.MCritHi = max;
                                                mainAcc.MZeroDmgCritHits = zeroCount;

                                                break;
                                            case DamageModifier.None:
                                                count = critType.Count();
                                                mainAcc.MHits += count;
                                                mainAcc.MNonEvaded += count;

                                                meleeDmg = critType.Sum(m => m.Amount);
                                                mainAcc.MDmg += meleeDmg;
                                                mainAcc.TDmg += meleeDmg;
                                                mainAcc.TMDmg += meleeDmg;

                                                min = critType.First().Amount;
                                                max = min;
                                                zeroCount = 0;

                                                foreach (var nonCrit in critType)
                                                {
                                                    if (nonCrit.Amount < min)
                                                        min = nonCrit.Amount;
                                                    if (nonCrit.Amount > max)
                                                        max = nonCrit.Amount;
                                                    if (nonCrit.Amount == 0)
                                                        zeroCount++;
                                                }

                                                mainAcc.MLow = min;
                                                mainAcc.MHi = max;
                                                mainAcc.MZeroDmgHits = zeroCount;

                                                break;
                                        }
                                    }

                                    break;
                                case DefenseType.Absorb:
                                    mainAcc.MAbsHits += hitType.Count();

                                    int absMDamage = hitType.Sum(m => m.Amount);

                                    mainAcc.TAbsMDmg += absMDamage;
                                    mainAcc.TAbsDmg += absMDamage;
                                    mainAcc.TDmg += absMDamage;
                                    mainAcc.TMDmg += absMDamage;

                                    break;
                                case DefenseType.Evasion:
                                    mainAcc.MMiss += hitType.Count();
                                    mainAcc.MEvaded += hitType.Count();
                                    break;
                                default:
                                    mainAcc.MMiss += hitType.Count();
                                    mainAcc.MNonEvaded += hitType.Count();
                                    break;
                            }
                        }
                    }
                    #endregion

                    #region Range
                    if (player.Range.Any())
                    {
                        var groupHits = from r in player.Range
                                        group r by (DefenseType)r.DefenseType;

                        foreach (var hitType in groupHits)
                        {
                            switch (hitType.Key)
                            {
                                case DefenseType.None:
                                    var groupCrits = from r in hitType
                                                     group r by (DamageModifier)r.DamageModifier;

                                    int count;
                                    int rangeDmg;
                                    int zeroCount;

                                    foreach (var critType in groupCrits)
                                    {
                                        switch (critType.Key)
                                        {
                                            case DamageModifier.Critical:
                                                count = critType.Count();
                                                mainAcc.RHits += count;
                                                mainAcc.RCritHits += count;
                                                mainAcc.RNonEvaded += count;

                                                int critDmg = critType.Sum(r => r.Amount);

                                                mainAcc.RDmg += critDmg;
                                                mainAcc.TDmg += critDmg;
                                                mainAcc.TRDmg += critDmg;
                                                mainAcc.RCritDmg += critDmg;

                                                min = critType.First().Amount;
                                                max = min;
                                                zeroCount = 0;

                                                foreach (var crit in critType)
                                                {
                                                    if (crit.Amount < min)
                                                        min = crit.Amount;
                                                    if (crit.Amount > max)
                                                        max = crit.Amount;
                                                    if (crit.Amount == 0)
                                                        zeroCount++;
                                                }

                                                mainAcc.RCritLow = min;
                                                mainAcc.RCritHi = max;
                                                mainAcc.RZeroDmgCritHits = zeroCount;

                                                break;
                                            case DamageModifier.None:
                                                count = critType.Count();
                                                mainAcc.RHits += count;
                                                mainAcc.RNonEvaded += count;

                                                rangeDmg = critType.Sum(r => r.Amount);
                                                mainAcc.RDmg += rangeDmg;
                                                mainAcc.TDmg += rangeDmg;
                                                mainAcc.TRDmg += rangeDmg;

                                                min = critType.First().Amount;
                                                max = min;
                                                zeroCount = 0;

                                                foreach (var nonCrit in critType)
                                                {
                                                    if (nonCrit.Amount < min)
                                                        min = nonCrit.Amount;
                                                    if (nonCrit.Amount > max)
                                                        max = nonCrit.Amount;
                                                    if (nonCrit.Amount == 0)
                                                        zeroCount++;
                                                }

                                                mainAcc.RLow = min;
                                                mainAcc.RHi = max;
                                                mainAcc.RZeroDmgHits = zeroCount;

                                                break;
                                        }
                                    }

                                    break;
                                case DefenseType.Absorb:
                                    mainAcc.RAbsHits += hitType.Count();

                                    int absRDamage = hitType.Sum(r => r.Amount);

                                    mainAcc.TAbsRDmg += absRDamage;
                                    mainAcc.TAbsDmg += absRDamage;
                                    mainAcc.TDmg += absRDamage;
                                    mainAcc.TRDmg += absRDamage;

                                    break;
                                case DefenseType.Evasion:
                                    mainAcc.RMiss += hitType.Count();
                                    mainAcc.REvaded += hitType.Count();
                                    break;
                                default:
                                    mainAcc.RMiss += hitType.Count();
                                    mainAcc.RNonEvaded += hitType.Count();
                                    break;
                            }
                        }
                    }
                    #endregion

                    #region Ability

                    if (player.Ability.Any())
                    {
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


                            var groupHits = from a in abil
                                            group a by (DefenseType)a.DefenseType;

                            foreach (var hitType in groupHits)
                            {
                                switch (hitType.Key)
                                {
                                    case DefenseType.None:
                                        abilAcc.AHit += hitType.Count();

                                        int abilDamage = hitType.Sum(a => a.Amount);

                                        abilAcc.ADmg += abilDamage;
                                        mainAcc.TDmg += abilDamage;
                                        mainAcc.TADmg += abilDamage;
                                        mainAcc.ADmg += abilDamage;

                                        min = hitType.First().Amount;
                                        max = min;

                                        foreach (var dmg in hitType)
                                        {
                                            if (dmg.Amount < min)
                                                min = dmg.Amount;
                                            if (dmg.Amount > max)
                                                max = dmg.Amount;
                                        }

                                        abilAcc.ALow = min;
                                        abilAcc.AHi = max; 
                                        
                                        break;
                                    case DefenseType.Absorb:
                                        abilAcc.AAbsHit += hitType.Count();

                                        int absADamage = hitType.Sum(a => a.Amount);

                                        abilAcc.AAbsDmg += absADamage;
                                        mainAcc.TAbsADmg += absADamage;
                                        mainAcc.TAbsDmg += absADamage;
                                        mainAcc.TDmg += absADamage;
                                        mainAcc.TADmg += absADamage;

                                        break;
                                    default:
                                        abilAcc.AMiss += hitType.Count();
                                        break;
                                }
                            }
                        }
                    }
                    #endregion

                    #region Weaponskills

                    if (player.WSkill.Any())
                    {
                        var wskills = player.WSkill.Where(w => w.IsActionIDNull() == false)
                            .GroupBy(a => a.ActionsRow.ActionName);

                        foreach (var wskill in wskills)
                        {
                            string wskillName = wskill.Key;

                            WSAccum wsAccum = mainAcc.Weaponskills.FirstOrDefault(
                                w => w.WName == wskillName);

                            if (wsAccum == null)
                            {
                                wsAccum = new WSAccum { WName = wskillName };
                                mainAcc.Weaponskills.Add(wsAccum);
                            }

                            var groupHits = from w in wskill
                                            group w by (DefenseType)w.DefenseType;

                            foreach (var hitType in groupHits)
                            {
                                switch (hitType.Key)
                                {
                                    case DefenseType.None:
                                        wsAccum.WHit += hitType.Count();

                                        int wsDamage = hitType.Sum(a => a.Amount);

                                        wsAccum.WDmg += wsDamage;
                                        mainAcc.TDmg += wsDamage;
                                        mainAcc.TWDmg += wsDamage;
                                        mainAcc.WDmg += wsDamage;

                                        min = hitType.First().Amount;
                                        max = min;

                                        foreach (var dmg in hitType)
                                        {
                                            if (dmg.Amount < min)
                                                min = dmg.Amount;
                                            if (dmg.Amount > max)
                                                max = dmg.Amount;
                                        }

                                        wsAccum.WLow = min;
                                        wsAccum.WHi = max; 
                                        
                                        break;
                                    case DefenseType.Absorb:
                                        wsAccum.WAbsHit += hitType.Count();

                                        int absWDamage = hitType.Sum(a => a.Amount);

                                        wsAccum.WAbsDmg += absWDamage;
                                        mainAcc.TAbsWDmg += absWDamage;
                                        mainAcc.TAbsDmg += absWDamage;
                                        mainAcc.TDmg += absWDamage;
                                        mainAcc.TWDmg += absWDamage;

                                        break;
                                    default:
                                        wsAccum.WMiss += hitType.Count();
                                        break;
                                }
                            }
                        }
                    }

                    #endregion

                    #region Spells


                    if (player.Spell.Any())
                    {
                        var spells = player.Spell.Where(a => a.IsActionIDNull() == false)
                            .GroupBy(a => a.ActionsRow.ActionName);

                        foreach (var spell in spells)
                        {
                            string spellName = spell.Key;

                            SpellAccum spellAcc = mainAcc.Spells.FirstOrDefault(
                                s => s.SName == spellName);

                            if (spellAcc == null)
                            {
                                spellAcc = new SpellAccum { SName = spellName };
                                mainAcc.Spells.Add(spellAcc);
                            }


                            var groupSpells = from a in spell
                                            group a by (DefenseType)a.DefenseType;

                            foreach (var resType in groupSpells)
                            {
                                switch (resType.Key)
                                {
                                    case DefenseType.None:
                                        var groupMBs = from m in resType
                                                         group m by (DamageModifier)m.DamageModifier;

                                        int count;
                                        int spellDamage;

                                        foreach (var mbType in groupMBs)
                                        {
                                            switch (mbType.Key)
                                            {
                                                case DamageModifier.MagicBurst:
                                                    count = mbType.Count();
                                                    spellAcc.SNumMB += count;
                                                    spellAcc.SNum += count;

                                                    int mbDmg = mbType.Sum(m => m.Amount);

                                                    spellAcc.SDmg += mbDmg;
                                                    spellAcc.SMBDmg += mbDmg;
                                                    mainAcc.TDmg += mbDmg;
                                                    mainAcc.TSDmg += mbDmg;
                                                    mainAcc.SDmg += mbDmg;

                                                    min = mbType.First().Amount;
                                                    max = min;

                                                    foreach (var mb in mbType)
                                                    {
                                                        if (mb.Amount < min)
                                                            min = mb.Amount;
                                                        if (mb.Amount > max)
                                                            max = mb.Amount;
                                                    }

                                                    spellAcc.SMBLow = min;
                                                    spellAcc.SMBHi = max;

                                                    break;
                                                case DamageModifier.None:
                                                    spellAcc.SNum += resType.Count();

                                                    spellDamage = mbType.Sum(m => m.Amount);

                                                    spellAcc.SDmg += spellDamage;
                                                    mainAcc.TDmg += spellDamage;
                                                    mainAcc.TSDmg += spellDamage;
                                                    mainAcc.SDmg += spellDamage;

                                                    min = mbType.First().Amount;
                                                    max = min;

                                                    foreach (var nonMB in mbType)
                                                    {
                                                        if (nonMB.Amount < min)
                                                            min = nonMB.Amount;
                                                        if (nonMB.Amount > max)
                                                            max = nonMB.Amount;
                                                    }

                                                    spellAcc.SLow = min;
                                                    spellAcc.SHi = max;

                                                    break;
                                            }
                                        }

                                        break;
                                    case DefenseType.Absorb:
                                        spellAcc.SAbsNum += resType.Count();

                                        int absSDamage = resType.Sum(a => a.Amount);

                                        spellAcc.SAbsDmg += absSDamage;
                                        mainAcc.TAbsSDmg += absSDamage;
                                        mainAcc.TAbsDmg += absSDamage;
                                        mainAcc.TDmg += absSDamage;
                                        mainAcc.TSDmg += absSDamage;

                                        break;
                                    case DefenseType.Resist:
                                        spellAcc.SFail += resType.Count();
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    #endregion

                    #region Other Magic
                    if (player.MeleeEffect.Any())
                    {
                        int dmg = player.MeleeEffect.Sum(a => a.SecondAmount);

                        mainAcc.MAENum += player.MeleeEffect.Count();
                        mainAcc.MAEDmg += dmg;
                        mainAcc.TODmg += dmg;
                        mainAcc.TDmg += dmg;
                    }

                    if (player.RangeEffect.Any())
                    {
                        int dmg = player.RangeEffect.Sum(a => a.SecondAmount);

                        mainAcc.RAENum += player.RangeEffect.Count();
                        mainAcc.RAEDmg += dmg;
                        mainAcc.TODmg += dmg;
                        mainAcc.TDmg += dmg;
                    }

                    if (player.Spikes.Any())
                    {
                        int dmg = player.Spikes.Sum(a => a.Amount);

                        mainAcc.SpkNum += player.Spikes.Count();
                        mainAcc.SpkDmg += dmg;
                        mainAcc.TODmg += dmg;
                        mainAcc.TDmg += dmg;
                    }
                    #endregion

                    #region Other Physical
                    if (player.Counter.Any())
                    {
                        var succHits = player.Counter.Where(h => (DefenseType)h.DefenseType == DefenseType.None);

                        if ((mainAcc.CAHits == 0) && (succHits.Any()))
                        {
                            mainAcc.CAHi = succHits.First().Amount;
                            mainAcc.CALow = mainAcc.CAHi;
                        }

                        if (succHits.Any())
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

                    if (player.Retaliate.Any())
                    {
                        var succHits = player.Retaliate.Where(h => (DefenseType)h.DefenseType == DefenseType.None);

                        if ((mainAcc.RTHits == 0) && (succHits.Any()))
                        {
                            mainAcc.RTHi = succHits.First().Amount;
                            mainAcc.RTLow = mainAcc.RTHi;
                        }

                        if (succHits.Any())
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
            int actionSourceFilterIndex = categoryCombo.CBSelectedIndex();

            List<StringMods> strModList = new List<StringMods>();
            StringBuilder sb = new StringBuilder();

            switch (actionSourceFilterIndex)
            {
                case 0: // All
                    ProcessAttackSummary(ref sb, ref strModList);
                    ProcessMeleeAttacks(ref sb, ref strModList);
                    ProcessRangedAttacks(ref sb, ref strModList);
                    ProcessOtherAttacks(ref sb, ref strModList);
                    ProcessWeaponskillAttacks(ref sb, ref strModList);
                    ProcessAbilityAttacks(ref sb, ref strModList);
                    ProcessSpellsAttacks(ref sb, ref strModList);
                    ProcessSkillchains(ref sb, ref strModList);
                    break;
                case 1: // "Summary":
                    ProcessAttackSummary(ref sb, ref strModList);
                    break;
                case 2: // "Melee":
                    ProcessMeleeAttacks(ref sb, ref strModList);
                    break;
                case 3: // "Ranged":
                    ProcessRangedAttacks(ref sb, ref strModList);
                    break;
                case 4: // "Other":
                    ProcessOtherAttacks(ref sb, ref strModList);
                    break;
                case 5: // "Weaponskills":
                    ProcessWeaponskillAttacks(ref sb, ref strModList);
                    break;
                case 6: // "Abilities":
                    ProcessAbilityAttacks(ref sb, ref strModList);
                    break;
                case 7: // "Spells":
                    ProcessSpellsAttacks(ref sb, ref strModList);
                    break;
                case 8: // "Skillchains":
                    ProcessSkillchains(ref sb, ref strModList);
                    break;
                case 9: // "Copy Summary":
                    ProcessCopySummary(ref sb, ref strModList);
                    break;
            }

            PushStrings(sb, strModList);
        }

        private void ProcessAttackSummary(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            totalDamage = dataAccum.Sum(p => p.TDmg);

            if (totalDamage != 0)
            {
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsSummaryTitle.Length,
                    Bold = true,
                    Color = Color.Red
                });
                sb.Append(lsSummaryTitle + "\n");

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsSummaryHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(lsSummaryHeader + "\n");


                foreach (var player in dataAccum.OrderBy(p => p.CType).ThenBy(p => p.Name))
                {
                    if (player.TDmg != 0)
                    {
                        sb.AppendFormat(lsSummaryFormat,
                            player.DisplayName,
                            player.TDmg,
                            (double)player.TDmg / totalDamage,
                            player.TMDmg,
                            player.TRDmg,
                            player.TADmg,
                            player.TWDmg,
                            player.TSDmg,
                            player.TODmg,
                            player.TAbsDmg);
                        sb.Append("\n");
                    }
                }

                string strTotal =
                    string.Format(lsSummaryFormat,
                        lsTotal,
                        dataAccum.Sum(p => p.TDmg),
                        1,
                        dataAccum.Sum(p => p.TMDmg),
                        dataAccum.Sum(p => p.TRDmg),
                        dataAccum.Sum(p => p.TADmg),
                        dataAccum.Sum(p => p.TWDmg),
                        dataAccum.Sum(p => p.TSDmg),
                        dataAccum.Sum(p => p.TODmg),
                        dataAccum.Sum(p => p.TAbsDmg));

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = strTotal.Length,
                    Bold = true,
                    Color = Color.Black
                });
                sb.Append(strTotal.ToString() + "\n");

            }

            sb.Append("\n\n");
        }

        private void ProcessMeleeAttacks(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (dataAccum.Any(p => p.MHits > 0) ||
                dataAccum.Any(p => p.MMiss > 0))
            {
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsMeleeTitle.Length,
                    Bold = true,
                    Color = Color.Red
                });
                sb.Append(lsMeleeTitle + "\n");

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsMeleeHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(lsMeleeHeader + "\n");


                foreach (var player in dataAccum.OrderBy(p => p.CType).ThenBy(p => p.Name))
                {
                    if ((player.MHits + player.MMiss) > 0)
                    {
                        sb.AppendFormat(lsMeleeFormat,
                          player.DisplayName,
                          player.MDmg,
                          player.TAbsMDmg,
                          player.TMDmg,
                          (player.TDmg != 0) ? (double)player.TMDmg / player.TDmg : 0,
                          string.Format("{0}/{1}", player.MHits, player.MMiss),
                          (double)player.MHits / (player.MHits + player.MMiss),
                          (double)player.MNonEvaded / (player.MNonEvaded + player.MEvaded),
                          string.Format("{0}/{1}", player.MLow, player.MHi),
                          (player.MHits > player.MCritHits) ? (double)(player.MDmg - player.MCritDmg) / (player.MHits - player.MCritHits) : 0,
                          (player.MHits > player.MCritHits) ? (double)(player.MDmg - player.MCritDmg) / (player.MHits - player.MCritHits - player.MZeroDmgHits) : 0,
                          player.MCritHits,
                          string.Format("{0}/{1}", player.MCritLow, player.MCritHi),
                          ((player.MCritHits - player.MZeroDmgCritHits) > 0) ? (double)player.MCritDmg / (player.MCritHits - player.MZeroDmgCritHits) : 0,
                          (player.MHits > 0) ? (double)player.MCritHits / player.MHits : 0
                          );
                        sb.Append("\n");
                    }
                }

                sb.Append("\n\n");


                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsMeleeCritTitle.Length,
                    Bold = true,
                    Color = Color.Red
                });
                sb.Append(lsMeleeCritTitle + "\n");

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsMeleeCritHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(lsMeleeCritHeader + "\n");


                foreach (var player in dataAccum.OrderBy(p => p.CType).ThenBy(p => p.Name))
                {
                    if ((player.MHits + player.MMiss) > 0)
                    {
                        sb.AppendFormat(lsMeleeCritFormat,
                          player.DisplayName,
                          player.MCritHits,
                          string.Format("{0}/{1}", player.MCritLow, player.MCritHi),
                          ((player.MCritHits - player.MZeroDmgCritHits) > 0) ? (double)player.MCritDmg / (player.MCritHits - player.MZeroDmgCritHits) : 0,
                          (player.MHits > 0) ? (double)player.MCritHits / player.MHits : 0
                          );
                        sb.Append("\n");
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
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsRangeTitle.Length,
                    Bold = true,
                    Color = Color.Red
                });
                sb.Append(lsRangeTitle + "\n");

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsRangeHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(lsRangeHeader + "\n");


                foreach (var player in dataAccum.OrderBy(p => p.CType).ThenBy(p => p.Name))
                {
                    if ((player.RHits + player.RMiss) > 0)
                    {
                        sb.AppendFormat(lsRangeFormat,
                          player.DisplayName,
                          player.TRDmg,
                          (player.TDmg > 0) ? (double)player.TRDmg / player.TDmg : 0,
                          string.Format("{0}/{1}", player.RHits, player.RMiss),
                          (double)player.RHits / (player.RHits + player.RMiss),
                          string.Format("{0}/{1}", player.RLow, player.RHi),
                          (player.RHits > player.RCritHits) ? (double)(player.TRDmg - player.RCritDmg) / (player.RHits - player.RCritHits) : 0,
                          player.RCritHits,
                          string.Format("{0}/{1}", player.RCritLow, player.RCritHi),
                          ((player.RCritHits - player.RZeroDmgCritHits) > 0) ? (double)player.RCritDmg / (player.RCritHits - player.RZeroDmgCritHits) : 0,
                          (player.RHits > 0) ? (double)player.RCritHits / player.RHits : 0,
                          (double)player.RNonEvaded / (player.RNonEvaded + player.REvaded),
                          (player.RHits > player.RCritHits) ? (double)(player.TRDmg - player.RCritDmg) / (player.RHits - player.RCritHits - player.RZeroDmgHits) : 0
                          );
                        sb.Append("\n");
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
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsWeaponskillTitle.Length,
                    Bold = true,
                    Color = Color.Red
                });
                sb.Append(lsWeaponskillTitle + "\n");

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsWeaponskillHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(lsWeaponskillHeader + "\n");


                foreach (var player in dataAccum.OrderBy(p => p.CType).ThenBy(p => p.Name))
                {
                    if (player.Weaponskills.Count > 0)
                    {
                        sb.AppendFormat(lsWeaponskillFormat,
                             player.DisplayName,
                             player.WDmg,
                             player.TAbsWDmg,
                             player.TWDmg,
                             (player.TDmg > 0) ? (double)player.TWDmg / player.TDmg : 0,
                             string.Format("{0}/{1}", player.Weaponskills.Sum(w => w.WHit), player.Weaponskills.Sum(w => w.WMiss)),
                             (double)player.Weaponskills.Sum(w => w.WHit) / player.Weaponskills.Sum(w => w.WHit + w.WMiss),
                             string.Format("{0}/{1}", player.Weaponskills.Min(w => w.WLow), player.Weaponskills.Max(w => w.WHi)),
                             player.Weaponskills.Any(w => w.WHit > 0) ? (double)player.TWDmg / player.Weaponskills.Sum(w => w.WHit) : 0);
                        sb.Append("\n");

                        foreach (var wskill in player.Weaponskills.OrderBy(w => w.WName))
                        {
                            sb.AppendFormat(lsWeaponskillFormat,
                                 string.Concat(" - ", wskill.WName),
                                 wskill.WDmg,
                                 wskill.WAbsDmg,
                                 wskill.WDmg + wskill.WAbsDmg,
                                 (player.TWDmg > 0) ? (double)wskill.WDmg / player.WDmg : 0,
                                 string.Format("{0}/{1}", wskill.WHit, wskill.WMiss),
                                 (wskill.WHit + wskill.WMiss) > 0 ? (double)wskill.WHit / (wskill.WHit + wskill.WMiss) : 0,
                                 string.Format("{0}/{1}", wskill.WLow, wskill.WHi),
                                 wskill.WHit > 0 ? (double)wskill.WDmg / wskill.WHit : 0);
                            sb.Append("\n");
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
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsAbilityTitle.Length,
                    Bold = true,
                    Color = Color.Red
                });
                sb.Append(lsAbilityTitle + "\n");

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsAbilityHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(lsAbilityHeader + "\n");


                foreach (var player in dataAccum.OrderBy(p => p.CType).ThenBy(p => p.Name))
                {
                    if (player.Abilities.Count > 0)
                    {
                        sb.AppendFormat(lsAbilityFormat,
                             player.DisplayName,
                             player.ADmg,
                             player.TAbsADmg,
                             player.TADmg,
                             (player.TDmg != 0) ? (double)player.TADmg / player.TDmg : 0,
                             string.Format("{0}/{1}", player.Abilities.Sum(w => w.AHit), player.Abilities.Sum(w => w.AMiss)),
                             (double)player.Abilities.Sum(w => w.AHit) / player.Abilities.Sum(w => w.AHit + w.AMiss),
                             player.Abilities.Sum(a => a.AHit) > 0 ?
                                string.Format("{0}/{1}", player.Abilities.Where(a => a.AHit > 0).Min(w => w.ALow), player.Abilities.Max(w => w.AHi)) :
                                string.Format("{0}/{1}", 0, 0),
                             player.Abilities.Any(w => w.AHit > 0) ? (double)player.ADmg / player.Abilities.Sum(w => w.AHit) : 0);
                        sb.Append("\n");

                        foreach (var abil in player.Abilities.OrderBy(w => w.AName))
                        {
                            sb.AppendFormat(lsAbilityFormat,
                                 string.Concat(" - ", abil.AName),
                                 abil.ADmg,
                                 abil.AAbsDmg,
                                 abil.ADmg + abil.AAbsDmg,
                                 (player.TADmg > 0) ? (double)abil.ADmg / player.ADmg : 0,
                                 string.Format("{0}/{1}", abil.AHit, abil.AMiss),
                                 (abil.AHit + abil.AMiss) > 0 ? (double)abil.AHit / (abil.AHit + abil.AMiss) : 0,
                                 string.Format("{0}/{1}", abil.ALow, abil.AHi),
                                 abil.AHit > 0 ? (double)abil.ADmg / abil.AHit : 0);
                            sb.Append("\n");
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
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsSpellTitle.Length,
                    Bold = true,
                    Color = Color.Red
                });
                sb.Append(lsSpellTitle + "\n");

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsSpellHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(lsSpellHeader + "\n");


                foreach (var player in dataAccum.OrderBy(p => p.CType).ThenBy(p => p.Name))
                {
                    if (player.Spells.Count > 0)
                    {
                        sb.AppendFormat(lsSpellFormat,
                             player.DisplayName,
                             player.SDmg,
                             player.TAbsSDmg,
                             player.TSDmg,
                             (player.TDmg != 0) ? (double)player.TSDmg / player.TDmg : 0,
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
                        sb.Append("\n");

                        foreach (var spell in player.Spells.OrderBy(w => w.SName))
                        {
                            sb.AppendFormat(lsSpellFormat,
                                 string.Concat(" - ", spell.SName),
                                 spell.SDmg,
                                 spell.SAbsDmg,
                                 spell.SDmg + spell.SAbsDmg,
                                 (player.TSDmg != 0) ? (double)(spell.SDmg + spell.SAbsDmg) / player.TSDmg : 0,
                                 spell.SNum,
                                 spell.SFail,
                                 string.Format("{0}/{1}", spell.SLow, spell.SHi),
                                 (spell.SNum > spell.SNumMB) ?
                                    (double)(spell.SDmg - spell.SMBDmg) / (spell.SNum - spell.SNumMB) : 0,
                                 spell.SNumMB,
                                 string.Format("{0}/{1}", spell.SMBLow, spell.SMBHi),
                                 spell.SNumMB > 0 ? (double)spell.SMBDmg / spell.SNumMB : 0);
                            sb.Append("\n");
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
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsSkillchainTitle.Length,
                    Bold = true,
                    Color = Color.Red
                });
                sb.Append(lsSkillchainTitle + "\n");

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsSkillchainHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(lsSkillchainHeader + "\n");


                foreach (var player in dataAccum.Where(p => p.CType == EntityType.Skillchain).OrderBy(p => p.Name))
                {
                    if (player.SCNum > 0)
                    {
                        sb.AppendFormat(lsSkillchainFormat,
                             player.Name,
                             player.SCDmg,
                             player.TAbsSCDmg,
                             player.TSCDmg,
                             player.SCNum,
                             string.Format("{0}/{1}", player.SCLow, player.SCHi),
                             (player.SCNum > 0) ? (double)player.TSCDmg / player.SCNum : 0);
                        sb.Append("\n");
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
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsOtherMagicalTitle.Length,
                    Bold = true,
                    Color = Color.Red
                });
                sb.Append(lsOtherMagicalTitle + "\n");

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsOtherMagicalHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(lsOtherMagicalHeader + "\n");


                foreach (var player in dataAccum.OrderBy(p => p.CType).ThenBy(p => p.Name))
                {
                    if ((player.MAENum + player.RAENum + player.SpkNum) > 0)
                    {
                        sb.AppendFormat(lsOtherMagicalFormat,
                            player.DisplayName,
                            player.MAEDmg,
                            player.MAENum,
                            player.MAENum > 0 ? (double)player.MAEDmg / player.MAENum : 0,
                            player.RAEDmg,
                            player.RAENum,
                            player.RAENum > 0 ? (double)player.RAEDmg / player.RAENum : 0,
                            player.SpkDmg,
                            player.SpkNum,
                            player.SpkNum > 0 ? (double)player.SpkDmg / player.SpkNum : 0);
                        sb.Append("\n");
                    }
                }

                sb.Append("\n\n");
            }


            if (dataAccum.Any(p => (p.CAHits + p.CAMiss + p.RTHits + p.RTMiss) > 0))
            {
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsOtherPhysicalTitle.Length,
                    Bold = true,
                    Color = Color.Red
                });
                sb.Append(lsOtherPhysicalTitle + "\n");

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsOtherPhysicalHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(lsOtherPhysicalHeader + "\n");


                foreach (var player in dataAccum.OrderBy(p => p.CType).ThenBy(p => p.Name))
                {
                    if ((player.CAHits + player.CAMiss + player.RTHits + player.RTMiss) > 0)
                    {
                        sb.AppendFormat(lsOtherPhysicalFormat,
                            player.DisplayName,
                            player.CADmg,
                            player.CAHits,
                            string.Concat(player.CALow, "/", player.CAHi),
                            player.CAHits > 0 ? (double)player.CADmg / player.CAHits : 0,
                            player.RTDmg,
                            player.RTHits,
                            string.Concat(player.RTLow, "/", player.RTHi),
                            player.RTHits > 0 ? (double)player.RTDmg / player.RTHits : 0);
                        sb.Append("\n");
                    }
                }

                sb.Append("\n\n");
            }
        }

        private void ProcessCopySummary(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            totalDamage = dataAccum.Sum(p => p.TDmg);

            if (totalDamage > 0)
            {
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsCopySummaryTitle.Length,
                    Bold = true,
                    Color = Color.Red
                });
                sb.Append(lsCopySummaryTitle + "\n\n");



                // Only 1% and higher results
                int cutoff = totalDamage / 100;

                var playersPlusPets = dataAccum
                    .Where(d => d.CType == EntityType.Player || d.CType == EntityType.Pet)
                    .Where(d => d.TDmg > cutoff)
                    .OrderBy(d => d.TDmg).Reverse();

                var head = playersPlusPets.Take(5);
                var tail = playersPlusPets.Skip(5);

                // in a linear format for pasting into game
                while (head.Any())
                {
                    string prefix = "";

                    foreach (var entry in head)
                    {
                        sb.AppendFormat("{2}{0}: {1:p2}", entry.DisplayName, (double)entry.TDmg / totalDamage, prefix);

                        prefix = ", ";
                    }
                    sb.Append("\n");

                    head = tail.Take(5);
                    tail = tail.Skip(5);
                }

                sb.Append("\n\n");

                // and in a tabular format for posting.

                foreach (var entry in playersPlusPets)
                {
                    sb.AppendFormat("{0,-18} :  {1,8:p2}\n", entry.DisplayName, (double)entry.TDmg / totalDamage);
                }
            }

            sb.Append("\n\n");
        }

        #endregion

        #region Event Handlers
        protected void categoryCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
            {
                try
                {
                    HandleDataset(fakeDatabaseChanges);

                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex);
                }
            }

            flagNoUpdate = false;
        }

        protected void mobsCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
            {
                try
                {
                    ResetAndUpdateAccumulation();
                    HandleDataset(fakeDatabaseChanges);

                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex);
                }
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
                try
                {
                    UpdateMobList();
                    flagNoUpdate = true;
                    mobsCombo.CBSelectIndex(0);

                    ResetAndUpdateAccumulation();
                    HandleDataset(fakeDatabaseChanges);

                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex);
                }
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
                try
                {
                    UpdateMobList();
                    flagNoUpdate = true;
                    mobsCombo.CBSelectIndex(0);

                    ResetAndUpdateAccumulation();
                    HandleDataset(fakeDatabaseChanges);

                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex);
                }
            }

            flagNoUpdate = false;
        }

        protected void customMobSelection_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            customMobSelection = sentBy.Checked;

            mobsCombo.Enabled = !customMobSelection;
            groupMobsOption.Enabled = !customMobSelection;
            exclude0XPOption.Enabled = !customMobSelection;

            editCustomMobFilter.Enabled = customMobSelection;

            if (flagNoUpdate == false)
            {
                try
                {
                    ResetAndUpdateAccumulation();
                    HandleDataset(fakeDatabaseChanges);

                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex);
                }
            }

            flagNoUpdate = false;
        }

        protected void editCustomMobFilter_Click(object sender, EventArgs e)
        {
            MobXPHandler.Instance.ShowCustomMobFilter();
        }

        protected override void OnCustomMobFilterChanged()
        {
            try
            {
                ResetAndUpdateAccumulation();
                HandleDataset(fakeDatabaseChanges);

            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
        }
        #endregion

        #region Localization Overrides
        protected override void LoadLocalizedUI()
        {
            catLabel.Text = Resources.PublicResources.CategoryLabel;
            mobsLabel.Text = Resources.PublicResources.MobsLabel;

            categoryCombo.Items.Clear();
            categoryCombo.Items.Add(Resources.PublicResources.All);
            categoryCombo.Items.Add(Resources.Combat.OffensePluginCategorySummary);
            categoryCombo.Items.Add(Resources.Combat.OffensePluginCategoryMelee);
            categoryCombo.Items.Add(Resources.Combat.OffensePluginCategoryRanged);
            categoryCombo.Items.Add(Resources.Combat.OffensePluginCategoryOther);
            categoryCombo.Items.Add(Resources.Combat.OffensePluginCategoryWeaponskill);
            categoryCombo.Items.Add(Resources.Combat.OffensePluginCategoryAbility);
            categoryCombo.Items.Add(Resources.Combat.OffensePluginCategorySpell);
            categoryCombo.Items.Add(Resources.Combat.OffensePluginCategorySkillchain);
            categoryCombo.Items.Add(Resources.Combat.OffensePluginCopySummary);
            categoryCombo.SelectedIndex = 0;

            UpdateMobList();
            mobsCombo.SelectedIndex = 0;

            optionsMenu.Text = Resources.PublicResources.Options;
            groupMobsOption.Text = Resources.PublicResources.GroupMobs;
            exclude0XPOption.Text = Resources.PublicResources.Exclude0XPMobs;
            customMobSelectionOption.Text = Resources.PublicResources.CustomMobSelection;
            editCustomMobFilter.Text = Resources.PublicResources.EditMobFilter;

        }

        protected override void LoadResources()
        {
            this.tabName = Resources.Combat.OffensePluginTabName;

            // Titles

            lsSummaryTitle = Resources.Combat.OffensePluginTitleSummary;
            lsMeleeTitle = Resources.Combat.OffensePluginTitleMelee;
            lsMeleeCritTitle = Resources.Combat.OffensePluginTitleMeleeCrit;
            lsRangeTitle = Resources.Combat.OffensePluginTitleRanged;
            lsSpellTitle = Resources.Combat.OffensePluginTitleSpell;
            lsAbilityTitle = Resources.Combat.OffensePluginTitleAbility;
            lsWeaponskillTitle = Resources.Combat.OffensePluginTitleWeaponskill;
            lsSkillchainTitle = Resources.Combat.OffensePluginTitleSkillchain;
            lsOtherPhysicalTitle = Resources.Combat.OffensePluginTitleOtherPhysical;
            lsOtherMagicalTitle = Resources.Combat.OffensePluginTitleOtherMagical;
            lsCopySummaryTitle = Resources.Combat.OffensePluginCopySummary;

            // Headers

            lsSummaryHeader = Resources.Combat.OffensePluginHeaderSummary;
            lsMeleeHeader = Resources.Combat.OffensePluginHeaderMelee;
            lsMeleeCritHeader = Resources.Combat.OffensePluginHeaderMeleeCrit;
            lsRangeHeader = Resources.Combat.OffensePluginHeaderRanged;
            lsSpellHeader = Resources.Combat.OffensePluginHeaderSpell;
            lsAbilityHeader = Resources.Combat.OffensePluginHeaderAbility;
            lsWeaponskillHeader = Resources.Combat.OffensePluginHeaderWeaponskill;
            lsSkillchainHeader = Resources.Combat.OffensePluginHeaderSkillchain;
            lsOtherPhysicalHeader = Resources.Combat.OffensePluginHeaderOtherPhysical;
            lsOtherMagicalHeader = Resources.Combat.OffensePluginHeaderOtherMagical;

            // Formatters

            lsSummaryFormat = Resources.Combat.OffensePluginFormatSummary;
            lsMeleeFormat = Resources.Combat.OffensePluginFormatMelee;
            lsMeleeCritFormat = Resources.Combat.OffensePluginFormatMeleeCrit;
            lsRangeFormat = Resources.Combat.OffensePluginFormatRanged;
            lsSpellFormat = Resources.Combat.OffensePluginFormatSpell;
            lsAbilityFormat = Resources.Combat.OffensePluginFormatAbility;
            lsWeaponskillFormat = Resources.Combat.OffensePluginFormatWeaponskill;
            lsSkillchainFormat = Resources.Combat.OffensePluginFormatSkillchain;
            lsOtherPhysicalFormat = Resources.Combat.OffensePluginFormatOtherPhysical;
            lsOtherMagicalFormat = Resources.Combat.OffensePluginFormatOtherMagical;

            // Misc

            lsTotal = Resources.PublicResources.Total;
        }
        #endregion
    }
}
