using System;
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
        #region Member Variables
        List<MainAccumulator> dataAccum = new List<MainAccumulator>();
        IEnumerable<DefenseGroup> defenseSet = null;
        IEnumerable<DefenseGroup> counterSet = null;
        IEnumerable<DefenseGroup> utsuSet = null;

        int totalDamage;
        List<string> playerList = new List<string>();
        Dictionary<string, int> playerDamage = new Dictionary<string, int>();

        bool flagNoUpdate;
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
        string lsRangeTitle;
        string lsSpellTitle;
        string lsAbilityTitle;
        string lsWeaponskillTitle;
        string lsSkillchainTitle;
        string lsOtherPhysicalTitle;
        string lsOtherMagicalTitle;

        string lsPassiveDefensesTitle;
        string lsActiveDefensesTitle;
        string lsUtsuCastingTitle;
        string lsShadowUseTitle;

        // Headers

        string lsSummaryHeader;
        string lsMeleeHeader;
        string lsRangeHeader;
        string lsSpellHeader;
        string lsAbilityHeader;
        string lsWeaponskillHeader;
        string lsSkillchainHeader;
        string lsOtherPhysicalHeader;
        string lsOtherMagicalHeader;

        string lsPassiveDefensesHeader;
        string lsActiveDefensesHeader;
        string lsUtsuCastingHeader;
        string lsShadowUseHeader;

        // Formatters

        string lsSummaryFormat;
        string lsMeleeFormat;
        string lsRangeFormat;
        string lsSpellFormat;
        string lsAbilityFormat;
        string lsWeaponskillFormat;
        string lsSkillchainFormat;
        string lsOtherPhysicalFormat;
        string lsOtherMagicalFormat;

        string lsPassiveDefensesFormat;
        string lsActiveDefensesFormat;
        string lsUtsuCastingFormat;
        string lsShadowUseFormat;

        // Misc

        string lsTotal;
        string lsDamageTaken;
        string lsDefenses;
        string lsUtsusemi;

        string lsUtsuIchi;
        string lsUtsuNi;

        #endregion

        #region Constructor
        public DefensePlugin()
        {
            LoadLocalizedUI();

            categoryCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryCombo.SelectedIndexChanged += new EventHandler(this.categoryCombo_SelectedIndexChanged);

            mobsCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            mobsCombo.AutoSize = false;
            mobsCombo.Width = 175;
            mobsCombo.MaxDropDownItems = 10;
            mobsCombo.SelectedIndexChanged += new EventHandler(this.mobsCombo_SelectedIndexChanged);


            groupMobsOption.CheckOnClick = true;
            groupMobsOption.Checked = true;
            groupMobsOption.Click += new EventHandler(groupMobs_Click);

            exclude0XPOption.CheckOnClick = true;
            exclude0XPOption.Checked = false;
            exclude0XPOption.Click += new EventHandler(exclude0XPMobs_Click);

            customMobSelectionOption.CheckOnClick = true;
            customMobSelectionOption.Checked = false;
            customMobSelectionOption.Click += new EventHandler(customMobSelection_Click);

            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;

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

            HandleDataset(null);
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if (e.DatasetChanges.Battles != null)
            {
                if (e.DatasetChanges.Battles.Any(x => x.RowState == DataRowState.Added))
                {
                    string selectedItem = mobsCombo.CBSelectedItem();
                    UpdateMobList(true);

                    flagNoUpdate = true;
                    mobsCombo.CBSelectItem(selectedItem);
                }
            }

            if (e.DatasetChanges.Interactions.Any(x => x.RowState == DataRowState.Added))
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
        }

        private void UpdateMobList(bool overrideGrouping)
        {
            if (overrideGrouping == true)
                mobsCombo.UpdateWithMobList(false, exclude0XPMobs);
            else
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

        private void UpdateAccumulationA(KPDatabaseDataSet dataSet)
        {
            MobFilter mobFilter = mobsCombo.CBGetMobFilter(exclude0XPMobs);

            #region LINQ queries

            IEnumerable<DefenseGroup> defenseSet = null;

            defenseSet = from c in dataSet.Combatants
                         where (((EntityType)c.CombatantType == EntityType.Player) ||
                                ((EntityType)c.CombatantType == EntityType.Pet) ||
                                ((EntityType)c.CombatantType == EntityType.CharmedMob) ||
                                ((EntityType)c.CombatantType == EntityType.Fellow))
                         orderby c.CombatantType, c.CombatantName
                         select new DefenseGroup
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
                          where (((EntityType)c.CombatantType == EntityType.Player) ||
                                ((EntityType)c.CombatantType == EntityType.Pet) ||
                                ((EntityType)c.CombatantType == EntityType.CharmedMob) ||
                                ((EntityType)c.CombatantType == EntityType.Fellow))
                          orderby c.CombatantType, c.CombatantName
                          select new
                          {
                              Name = c.CombatantName,
                              ComType = (EntityType)c.CombatantType,
                              UtsuIchiCast = from s in c.GetInteractionsRowsByActorCombatantRelation()
                                             where ((ActionType)s.ActionType == ActionType.Spell &&
                                                    s.Preparing == true &&
                                                    s.IsActionIDNull() == false &&
                                                    s.ActionsRow.ActionName == lsUtsuIchi &&
                                                    mobFilter.CheckFilterMobBattle(s) == true)
                                             select s,
                              UtsuIchiFinish = from s in c.GetInteractionsRowsByActorCombatantRelation()
                                             where ((ActionType)s.ActionType == ActionType.Spell &&
                                                    s.Preparing == false &&
                                                    s.IsActionIDNull() == false &&
                                                    s.ActionsRow.ActionName == lsUtsuIchi &&
                                                    mobFilter.CheckFilterMobBattle(s) == true)
                                             select s,
                              UtsuNiCast = from s in c.GetInteractionsRowsByActorCombatantRelation()
                                             where ((ActionType)s.ActionType == ActionType.Spell &&
                                                    s.Preparing == true &&
                                                    s.IsActionIDNull() == false &&
                                                    s.ActionsRow.ActionName == lsUtsuNi &&
                                                    mobFilter.CheckFilterMobBattle(s) == true)
                                             select s,
                              UtsuNiFinish = from s in c.GetInteractionsRowsByActorCombatantRelation()
                                             where ((ActionType)s.ActionType == ActionType.Spell &&
                                                    s.Preparing == false &&
                                                    s.IsActionIDNull() == false &&
                                                    s.ActionsRow.ActionName == lsUtsuNi &&
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

        private void UpdateAccumulationB(KPDatabaseDataSet dataSet, bool newRowsOnly)
        {
            if (dataSet == null)
                return;

            MobFilter mobFilter;
            if (customMobSelection)
                mobFilter = MobXPHandler.Instance.CustomMobFilter;
            else
                mobFilter = mobsCombo.CBGetMobFilter(exclude0XPMobs);

            #region LINQ queries

            if (mobFilter.AllMobs == false)
            {
                // If we have any mob filter subset, get that data starting
                // with the battle table and working outwards.

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

                defenseSet = from c in iRows
                             where (c.IsTargetIDNull() == false) &&
                                   ((EntityType)c.CombatantsRowByTargetCombatantRelation.CombatantType == EntityType.Player ||
                                    (EntityType)c.CombatantsRowByTargetCombatantRelation.CombatantType == EntityType.Pet ||
                                    (EntityType)c.CombatantsRowByTargetCombatantRelation.CombatantType == EntityType.CharmedMob ||
                                    (EntityType)c.CombatantsRowByTargetCombatantRelation.CombatantType == EntityType.Fellow)
                             group c by c.CombatantsRowByTargetCombatantRelation into ca
                             orderby ca.Key.CombatantType, ca.Key.CombatantName
                             select new DefenseGroup
                            {
                                Name = ca.Key.CombatantName,
                                DisplayName = ca.Key.CombatantNameOrJobName,
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
                                Counter = from q in ca
                                          where (ActionType)q.ActionType == ActionType.Counterattack
                                          select q,
                                Retaliate = from q in ca
                                            where (ActionType)q.ActionType == ActionType.Retaliation
                                            select q,
                                SC = from q in ca
                                     where ((ActionType)q.ActionType == ActionType.Skillchain &&
                                               ((HarmType)q.HarmType == HarmType.Damage ||
                                                (HarmType)q.HarmType == HarmType.Drain))
                                     select q,
                                Spikes = from q in ca
                                         where (ActionType)q.ActionType == ActionType.Spikes
                                         select q,
                                Unknown = from q in ca
                                          where ((HarmType)q.HarmType == HarmType.Damage ||
                                                 (HarmType)q.HarmType == HarmType.Drain) &&
                                                 (ActionType)q.ActionType == ActionType.Unknown
                                          select q
                            };

                counterSet = from c in iRows
                             where (c.IsActorIDNull() == false) &&
                                   ((EntityType)c.CombatantsRowByActorCombatantRelation.CombatantType == EntityType.Player ||
                                    (EntityType)c.CombatantsRowByActorCombatantRelation.CombatantType == EntityType.Pet ||
                                    (EntityType)c.CombatantsRowByActorCombatantRelation.CombatantType == EntityType.CharmedMob ||
                                    (EntityType)c.CombatantsRowByActorCombatantRelation.CombatantType == EntityType.Fellow)
                             group c by c.CombatantsRowByActorCombatantRelation into ca
                             orderby ca.Key.CombatantType, ca.Key.CombatantName
                             select new DefenseGroup
                            {
                                Name = ca.Key.CombatantName,
                                DisplayName = ca.Key.CombatantNameOrJobName,
                                ComType = (EntityType)ca.Key.CombatantType,
                                Countered = from q in ca
                                            where (ActionType)q.ActionType == ActionType.Counterattack
                                            select q,
                                Retaliated = from q in ca
                                            where (ActionType)q.ActionType == ActionType.Retaliation
                                            select q,

                            };

                utsuSet = from c in iRows
                          where (c.IsActorIDNull() == false &&
                                 (EntityType)c.CombatantsRowByActorCombatantRelation.CombatantType == EntityType.Player)
                          group c by c.CombatantsRowByActorCombatantRelation into ca
                          orderby ca.Key.CombatantType, ca.Key.CombatantName
                          select new DefenseGroup
                          {
                              Name = ca.Key.CombatantName,
                              DisplayName = ca.Key.CombatantNameOrJobName,
                              ComType = (EntityType)ca.Key.CombatantType,
                              UtsuIchiCast = from s in ca
                                             where ((ActionType)s.ActionType == ActionType.Spell &&
                                                    s.Preparing == true &&
                                                    s.IsActionIDNull() == false &&
                                                    s.ActionsRow.ActionName == lsUtsuIchi)
                                             select s,
                              UtsuIchiFinish = from s in ca
                                               where ((ActionType)s.ActionType == ActionType.Spell &&
                                                      s.Preparing == false &&
                                                      s.IsActionIDNull() == false &&
                                                      s.ActionsRow.ActionName == lsUtsuIchi)
                                               select s,
                              UtsuNiCast = from s in ca
                                           where ((ActionType)s.ActionType == ActionType.Spell &&
                                                  s.Preparing == true &&
                                                  s.IsActionIDNull() == false &&
                                                  s.ActionsRow.ActionName == lsUtsuNi)
                                           select s,
                              UtsuNiFinish = from s in ca
                                             where ((ActionType)s.ActionType == ActionType.Spell &&
                                                    s.Preparing == false &&
                                                    s.IsActionIDNull() == false &&
                                                    s.ActionsRow.ActionName == lsUtsuNi)
                                             select s,
                          };
            }
            else
            {

                // Faster to process this from the combatant side if our mob filter is 'All'

                defenseSet = from c in dataSet.Combatants
                             where (((EntityType)c.CombatantType == EntityType.Player) ||
                                    ((EntityType)c.CombatantType == EntityType.Pet) ||
                                    ((EntityType)c.CombatantType == EntityType.CharmedMob) ||
                                    ((EntityType)c.CombatantType == EntityType.Fellow))
                             orderby c.CombatantType, c.CombatantName
                             select new DefenseGroup
                             {
                                 Name = c.CombatantName,
                                 DisplayName = c.CombatantNameOrJobName,
                                 ComType = (EntityType)c.CombatantType,
                                 Melee = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                                    .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                         where ((ActionType)n.ActionType == ActionType.Melee &&
                                                ((HarmType)n.HarmType == HarmType.Damage ||
                                                 (HarmType)n.HarmType == HarmType.Drain))
                                         select n,
                                 Range = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                                    .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                         where ((ActionType)n.ActionType == ActionType.Ranged &&
                                                ((HarmType)n.HarmType == HarmType.Damage ||
                                                 (HarmType)n.HarmType == HarmType.Drain))
                                         select n,
                                 Spell = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                                    .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                         where ((ActionType)n.ActionType == ActionType.Spell &&
                                                ((HarmType)n.HarmType == HarmType.Damage ||
                                                 (HarmType)n.HarmType == HarmType.Drain) &&
                                                 n.Preparing == false)
                                         select n,
                                 Ability = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                                      .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                           where ((ActionType)n.ActionType == ActionType.Ability &&
                                                ((HarmType)n.HarmType == HarmType.Damage ||
                                                 (HarmType)n.HarmType == HarmType.Drain ||
                                                 (HarmType)n.HarmType == HarmType.Unknown) &&
                                                 n.Preparing == false)
                                           select n,
                                 WSkill = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                                     .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                          where ((ActionType)n.ActionType == ActionType.Weaponskill &&
                                                ((HarmType)n.HarmType == HarmType.Damage ||
                                                 (HarmType)n.HarmType == HarmType.Drain) &&
                                                 n.Preparing == false)
                                          select n,
                                 SC = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                                 .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                      where ((ActionType)n.ActionType == ActionType.Skillchain &&
                                                ((HarmType)n.HarmType == HarmType.Damage ||
                                                 (HarmType)n.HarmType == HarmType.Drain))
                                      select n,
                                 Counter = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                                      .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                             where (ActionType)n.ActionType == ActionType.Counterattack
                                             select n,
                                 Retaliate = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                                        .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                             where (ActionType)n.ActionType == ActionType.Retaliation
                                             select n,
                                 Spikes = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                                     .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                          where (ActionType)n.ActionType == ActionType.Spikes
                                          select n,
                                 Unknown = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                                      .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                           where ((HarmType)n.HarmType == HarmType.Damage ||
                                                  (HarmType)n.HarmType == HarmType.Drain) &&
                                                  (ActionType)n.ActionType == ActionType.Unknown
                                           select n
                             };

                counterSet = from c in dataSet.Combatants
                             where (((EntityType)c.CombatantType == EntityType.Player) ||
                                    ((EntityType)c.CombatantType == EntityType.Pet) ||
                                    ((EntityType)c.CombatantType == EntityType.CharmedMob) ||
                                    ((EntityType)c.CombatantType == EntityType.Fellow))
                             orderby c.CombatantType, c.CombatantName
                             select new DefenseGroup
                             {
                                 Name = c.CombatantName,
                                 DisplayName = c.CombatantNameOrJobName,
                                 ComType = (EntityType)c.CombatantType,
                                 Countered = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                                        .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                             where (ActionType)n.ActionType == ActionType.Counterattack
                                             select n,
                                 Retaliated = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                                         .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                              where (ActionType)n.ActionType == ActionType.Retaliation
                                              select n,
                             };



                utsuSet = from c in dataSet.Combatants
                          where (((EntityType)c.CombatantType == EntityType.Player) ||
                                ((EntityType)c.CombatantType == EntityType.Pet) ||
                                ((EntityType)c.CombatantType == EntityType.CharmedMob) ||
                                ((EntityType)c.CombatantType == EntityType.Fellow))
                          orderby c.CombatantType, c.CombatantName
                          select new DefenseGroup
                          {
                              Name = c.CombatantName,
                              DisplayName = c.CombatantNameOrJobName,
                              ComType = (EntityType)c.CombatantType,
                              UtsuIchiCast = from s in c.GetInteractionsRowsByActorCombatantRelation()
                                                        .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                             where ((ActionType)s.ActionType == ActionType.Spell &&
                                                    s.Preparing == true &&
                                                    s.IsActionIDNull() == false &&
                                                    s.ActionsRow.ActionName == lsUtsuIchi)
                                             select s,
                              UtsuIchiFinish = from s in c.GetInteractionsRowsByActorCombatantRelation()
                                                          .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                               where ((ActionType)s.ActionType == ActionType.Spell &&
                                                      s.Preparing == false &&
                                                      s.IsActionIDNull() == false &&
                                                      s.ActionsRow.ActionName == lsUtsuIchi)
                                               select s,
                              UtsuNiCast = from s in c.GetInteractionsRowsByActorCombatantRelation()
                                                      .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                           where ((ActionType)s.ActionType == ActionType.Spell &&
                                                  s.Preparing == true &&
                                                  s.IsActionIDNull() == false &&
                                                  s.ActionsRow.ActionName == lsUtsuNi)
                                           select s,
                              UtsuNiFinish = from s in c.GetInteractionsRowsByActorCombatantRelation()
                                                        .Where(r => (newRowsOnly == false) || (r.RowState == DataRowState.Added))
                                             where ((ActionType)s.ActionType == ActionType.Spell &&
                                                    s.Preparing == false &&
                                                    s.IsActionIDNull() == false &&
                                                    s.ActionsRow.ActionName == lsUtsuNi)
                                             select s,

                          };
            }
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
                    playerAccum = new MainAccumulator { Name = player.Name, DisplayName = player.DisplayName, CType = player.ComType };
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

                    var playerCounter = counterSet.FirstOrDefault(x =>
                        x.Name == player.Name && x.ComType == player.ComType);
                    if (playerCounter != null)
                    {
                        playerAccum.DefCounter += playerCounter.Countered.Count();
                        playerAccum.DefRetaliate += playerCounter.Retaliated.Count();
                    }

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
            int actionSourceFilterIndex = categoryCombo.CBSelectedIndex();

            List<StringMods> strModList = new List<StringMods>();
            StringBuilder sb = new StringBuilder();

            switch (actionSourceFilterIndex)
            {
                case 0: // All
                    ProcessDamageTaken(ref sb, ref strModList);
                    ProcessDefenses(ref sb, ref strModList);
                    ProcessUtsusemi(ref sb, ref strModList);
                    break;
                case 1: // Damage Taken
                    ProcessDamageTaken(ref sb, ref strModList);
                    break;
                case 2: // Defenses
                    ProcessDefenses(ref sb, ref strModList);
                    break;
                case 3: // Utsusemi
                    ProcessUtsusemi(ref sb, ref strModList);
                    break;
            }

            PushStrings(sb, strModList);
        }

        #region Damage Taken
        private void ProcessDamageTaken(ref StringBuilder sb, ref List<StringMods> strModList)
        {
            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsDamageTaken.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(lsDamageTaken + "\n\n");

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
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsSummaryTitle.Length,
                    Bold = true,
                    Color = Color.Blue
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
                    if (player.TDmg > 0)
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
                        player.TODmg);

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
                        dataAccum.Sum(p => p.TODmg));

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

        private void ProcessDTMeleeAttacks(
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
                    Color = Color.Blue
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

                        sb.Append("\n");
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
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsRangeTitle.Length,
                    Bold = true,
                    Color = Color.Blue
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
                          (player.RCritHits > 0) ? (double)player.RCritDmg / player.RCritHits : 0,
                          (player.RHits > 0) ? (double)player.RCritHits / player.RHits : 0);

                        sb.Append("\n");
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
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsWeaponskillTitle.Length,
                    Bold = true,
                    Color = Color.Blue
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
                                 (player.TWDmg > 0) ? (double)wskill.WDmg / player.TWDmg : 0,
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

        private void ProcessDTAbilityAttacks(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (dataAccum.Any(p => p.Abilities.Count > 0))
            {
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsAbilityTitle.Length,
                    Bold = true,
                    Color = Color.Blue
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
                             player.TADmg,
                             (player.TDmg > 0) ? (double)player.TADmg / player.TDmg : 0,
                             string.Format("{0}/{1}", player.Abilities.Sum(w => w.AHit), player.Abilities.Sum(w => w.AMiss)),
                             (double)player.Abilities.Sum(w => w.AHit) / player.Abilities.Sum(w => w.AHit + w.AMiss),
                             player.Abilities.Sum(a => a.AHit) > 0 ?
                                string.Format("{0}/{1}", player.Abilities.Where(a => a.AHit > 0).Min(w => w.ALow), player.Abilities.Max(w => w.AHi)) :
                                string.Format("{0}/{1}", 0, 0),
                             player.Abilities.Any(w => w.AHit > 0) ? (double)player.TADmg / player.Abilities.Sum(w => w.AHit) : 0);

                        sb.Append("\n");

                        foreach (var abil in player.Abilities.OrderBy(w => w.AName))
                        {
                            sb.AppendFormat(lsAbilityFormat,
                                 string.Concat(" - ", abil.AName),
                                 abil.ADmg,
                                 (player.TADmg > 0) ? (double)abil.ADmg / player.TADmg : 0,
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

        private void ProcessDTSpellsAttacks(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {

            if (dataAccum.Any(p => p.Spells.Count > 0))
            {
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsSpellTitle.Length,
                    Bold = true,
                    Color = Color.Blue
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
                        sb.Append("\n");


                        foreach (var spell in player.Spells.OrderBy(w => w.SName))
                        {
                            sb.AppendFormat(lsSpellFormat,
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
                            sb.Append("\n");
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
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsSkillchainTitle.Length,
                    Bold = true,
                    Color = Color.Blue
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

        private void ProcessDTOtherAttacks(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (dataAccum.Any(p => p.MAENum > 0 || p.RAENum > 0 || p.SpkNum > 0))
            {
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsOtherMagicalTitle.Length,
                    Bold = true,
                    Color = Color.Blue
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
                    Color = Color.Blue
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
                            string.Concat(player.CAHits, "/", player.CAMiss),
                            string.Concat(player.CALow, "/", player.CAHi),
                            player.CAHits > 0 ? (double)player.CADmg / player.CAHits : 0,
                            player.RTDmg,
                            string.Concat(player.RTHits, "/", player.RTMiss),
                            string.Concat(player.RTLow, "/", player.RTHi),
                            player.RTHits > 0 ? (double)player.RTDmg / player.RTHits : 0);
                        sb.Append("\n");
                    }
                }

                sb.Append("\n\n");
            }
        }
        #endregion

        #region Defenses
        private void ProcessDefenses(ref StringBuilder sb, ref List<StringMods> strModList)
        {
            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsDefenses.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(lsDefenses + "\n\n");

            ProcessPassiveDefenses(ref sb, ref strModList);
            ProcessActiveDefenses(ref sb, ref strModList);
        }

        private void ProcessPassiveDefenses(
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (dataAccum.Any(p => p.DefEvasion > 0 || p.DefParry > 0 || p.DefIntimidate > 0 ||
                (p.CAHits + p.CAMiss) > 0))
            {
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsPassiveDefensesTitle.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.Append(lsPassiveDefensesTitle + "\n");

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsPassiveDefensesHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(lsPassiveDefensesHeader + "\n");

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

                    sb.AppendFormat(lsPassiveDefensesFormat,
                         player.DisplayName,
                         player.DefEvasion,
                         evaPool > 0 ? (double)player.DefEvasion / evaPool : 0,
                         player.DefParry,
                         parrPool > 0 ? (double)player.DefParry / parrPool : 0,
                         player.DefCounter,
                         countPool > 0 ? (double)player.DefCounter / countPool : 0,
                         player.DefIntimidate,
                         intimPool > 0 ? (double)player.DefIntimidate / intimPool : 0);
                    sb.Append("\n");
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
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsActiveDefensesTitle.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.Append(lsActiveDefensesTitle + "\n");

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsActiveDefensesHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(lsActiveDefensesHeader + "\n");

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

                    sb.AppendFormat(lsActiveDefensesFormat,
                         player.DisplayName,
                         player.DefShadow,
                         shadowPool > 0 ? (double)player.DefShadow / shadowPool : 0,
                         player.DefAnticipate,
                         anticPool > 0 ? (double)player.DefAnticipate / anticPool : 0,
                         player.DefRetaliate,
                         retalPool > 0 ? (double)player.DefRetaliate / retalPool : 0);
                    sb.Append("\n");

                }

                sb.Append("\n\n");
            }
        }

        #endregion

        #region Utsusemi
        private void ProcessUtsusemi(ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (dataAccum.Any(p => p.UtsuICast > 0 || p.UtsuNCast > 0 || p.UtsuUsed > 0))
            {
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsUtsusemi.Length,
                    Bold = true,
                    Color = Color.Red
                });
                sb.Append(lsUtsusemi + "\n\n");

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsUtsuCastingTitle.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.Append(lsUtsuCastingTitle + "\n");

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsUtsuCastingHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(lsUtsuCastingHeader + "\n");

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


                    sb.AppendFormat(lsUtsuCastingFormat,
                         player.DisplayName,
                         player.UtsuICast,
                         player.UtsuIFin,
                         player.UtsuNCast,
                         player.UtsuNFin,
                         totalCastTimeStr);
                    sb.Append("\n");
                }

                sb.Append("\n\n");


                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsShadowUseTitle.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.Append(lsShadowUseTitle + "\n");

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsShadowUseHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(lsShadowUseHeader + "\n");

                int utsuCast = 0;
                int utsuCastNin = 0;

                foreach (var player in dataAccum
                    .Where(p => p.UtsuUsed > 0)
                    .OrderBy(p => p.Name))
                {
                    utsuCast = player.UtsuIFin * 3 + player.UtsuNFin * 3;
                    utsuCastNin = player.UtsuIFin * 3 + player.UtsuNFin * 4;

                    sb.AppendFormat(lsShadowUseFormat,
                         player.DisplayName,
                         player.UtsuUsed,
                         utsuCast,
                         utsuCastNin,
                         utsuCast > 0 ? (double)player.UtsuUsed / utsuCast : 0,
                         utsuCastNin > 0 ? (double)player.UtsuUsed / utsuCastNin : 0);
                    sb.Append("\n");
                }

            }
        }
        #endregion

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
                ResetAndUpdateAccumulation();
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

                ResetAndUpdateAccumulation();
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

                ResetAndUpdateAccumulation();
                HandleDataset(null);
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
                ResetAndUpdateAccumulation();
                HandleDataset(null);
            }

            flagNoUpdate = false;
        }

        protected void editCustomMobFilter_Click(object sender, EventArgs e)
        {
            MobXPHandler.Instance.ShowCustomMobFilter();
        }

        protected override void OnCustomMobFilterChanged()
        {
            ResetAndUpdateAccumulation();
            HandleDataset(null);
        }
        #endregion

        #region Localization Overrides
        protected override void LoadLocalizedUI()
        {
            catLabel.Text = Resources.PublicResources.CategoryLabel;
            mobsLabel.Text = Resources.PublicResources.MobsLabel;

            categoryCombo.Items.Clear();
            categoryCombo.Items.Add(Resources.PublicResources.All);
            categoryCombo.Items.Add(Resources.Combat.DefensePluginCategoryDamageTaken);
            categoryCombo.Items.Add(Resources.Combat.DefensePluginCategoryDefenses);
            categoryCombo.Items.Add(Resources.Combat.DefensePluginCategoryUtsusemi);
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
            this.tabName = Resources.Combat.DefensePluginTabName;

            // Titles

            lsSummaryTitle = Resources.Combat.DefensePluginTitleSummary;
            lsMeleeTitle = Resources.Combat.DefensePluginTitleMelee;
            lsRangeTitle = Resources.Combat.DefensePluginTitleRanged;
            lsSpellTitle = Resources.Combat.DefensePluginTitleSpell;
            lsAbilityTitle = Resources.Combat.DefensePluginTitleAbility;
            lsWeaponskillTitle = Resources.Combat.DefensePluginTitleWeaponskill;
            lsSkillchainTitle = Resources.Combat.DefensePluginTitleSkillchain;
            lsOtherPhysicalTitle = Resources.Combat.DefensePluginTitleOtherPhysical;
            lsOtherMagicalTitle = Resources.Combat.DefensePluginTitleOtherMagical;

            lsPassiveDefensesTitle = Resources.Combat.DefensePluginTitlePassiveDefenses;
            lsActiveDefensesTitle = Resources.Combat.DefensePluginTitleActiveDefenses;
            lsUtsuCastingTitle = Resources.Combat.DefensePluginTitleUtsuCasting;
            lsShadowUseTitle = Resources.Combat.DefensePluginTitleShadowUse;

            // Headers

            lsSummaryHeader = Resources.Combat.DefensePluginHeaderSummary;
            lsMeleeHeader = Resources.Combat.DefensePluginHeaderMelee;
            lsRangeHeader = Resources.Combat.DefensePluginHeaderRanged;
            lsSpellHeader = Resources.Combat.DefensePluginHeaderSpells;
            lsAbilityHeader = Resources.Combat.DefensePluginHeaderAbility;
            lsWeaponskillHeader = Resources.Combat.DefensePluginHeaderWeaponskill;
            lsSkillchainHeader = Resources.Combat.DefensePluginHeaderSkillchain;
            lsOtherPhysicalHeader = Resources.Combat.DefensePluginHeaderOtherPhysical;
            lsOtherMagicalHeader = Resources.Combat.DefensePluginHeaderOtherMagical;

            lsPassiveDefensesHeader = Resources.Combat.DefensePluginHeaderPassiveDefense;
            lsActiveDefensesHeader = Resources.Combat.DefensePluginHeaderActiveDefense;
            lsUtsuCastingHeader = Resources.Combat.DefensePluginHeaderUtsuCasting;
            lsShadowUseHeader = Resources.Combat.DefensePluginHeaderShadowUse;

            // Formatters

            lsSummaryFormat = Resources.Combat.DefensePluginFormatSummary;
            lsMeleeFormat = Resources.Combat.DefensePluginFormatMelee;
            lsRangeFormat = Resources.Combat.DefensePluginFormatRanged;
            lsSpellFormat = Resources.Combat.DefensePluginFormatSpells;
            lsAbilityFormat = Resources.Combat.DefensePluginFormatAbility;
            lsWeaponskillFormat = Resources.Combat.DefensePluginFormatWeaponskill;
            lsSkillchainFormat = Resources.Combat.DefensePluginFormatSkillchain;
            lsOtherPhysicalFormat = Resources.Combat.DefensePluginFormatOtherPhysical;
            lsOtherMagicalFormat = Resources.Combat.DefensePluginFormatOtherMagical;

            lsPassiveDefensesFormat = Resources.Combat.DefensePluginFormatPassiveDefense;
            lsActiveDefensesFormat = Resources.Combat.DefensePluginFormatActiveDefense;
            lsUtsuCastingFormat = Resources.Combat.DefensePluginFormatUtsuCasting;
            lsShadowUseFormat = Resources.Combat.DefensePluginFormatShadowUse;

            // Misc

            lsTotal = Resources.PublicResources.Total;
            lsDamageTaken = Resources.Combat.DefensePluginCategoryDamageTaken;
            lsDefenses = Resources.Combat.DefensePluginCategoryDefenses;
            lsUtsusemi = Resources.Combat.DefensePluginCategoryUtsusemi;

            // Spell names
            lsUtsuIchi = Resources.ParsedStrings.UtsuIchi;
            lsUtsuNi = Resources.ParsedStrings.UtsuNi;
        }
        #endregion

    }
}
