using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WaywardGamers.KParser;

namespace WaywardGamers.KParser.Plugin
{
    public class DefensePlugin : BasePluginControl
    {
        #region Constructor

        #region Member Variables
        Dictionary<string, int> playerDamage = new Dictionary<string, int>();

        string incAttacksHeader = "Player           Melee   Range   Abil/Ws   Spells   Unknown   Total   Attack# %   Avoided   Avoid %\n";
        string incDamageHeader = "Player           M.Dmg   Avg M.Dmg   R.Dmg  Avg R.Dmg   S.Dmg  Avg S.Dmg   A/WS.Dmg  Avg A/WS.Dmg   Damage %\n";
        string standardDefHeader = "Player           M.Evade  M.Evade %   R.Evade  R.Evade %   Shadow  Shadow %   Parry  Parry %\n";
        string otherDefHeader = "Player           Intimidate  Intimidate %   Anticipate  Anticipate %   Counter  Counter %   Retaliate  Retaliate %\n";

        string utsuHeader = "Player           Shadows Used   Ichi Cast  Ichi Fin  Ni Cast  Ni Fin   Count  Count(N)  Efficiency  Effic.(N)\n";

        bool flagNoUpdate;
        bool groupMobs = true;
        bool exclude0XPMobs = false;
        ToolStripComboBox categoryCombo = new ToolStripComboBox();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();
        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();
        #endregion

        public DefensePlugin()
        {
            ToolStripLabel catLabel = new ToolStripLabel();
            catLabel.Text = "Category:";
            toolStrip.Items.Add(catLabel);

            categoryCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryCombo.Items.Add("All");
            categoryCombo.Items.Add("Attacks");
            categoryCombo.Items.Add("Damage");
            categoryCombo.Items.Add("Evasion");
            categoryCombo.Items.Add("Other Defense");
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
        }

        public override void DatabaseOpened(KPDatabaseDataSet dataSet)
        {
            UpdateMobList(dataSet, false);

            mobsCombo.CBSelectIndex(0);
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if (e.DatasetChanges.Battles.Count > 0)
            {
                string selectedItem = mobsCombo.CBSelectedItem();
                UpdateMobList(e.Dataset, true);

                flagNoUpdate = true;
                mobsCombo.CBSelectItem(selectedItem);
            }

            if (e.DatasetChanges.Interactions.Count != 0)
            {
                datasetToUse = e.Dataset;
                return true;
            }

            datasetToUse = null;
            return false;
        }
        #endregion


        #region Private Methods
        private void UpdateMobList()
        {
            UpdateMobList(DatabaseManager.Instance.Database, false);
            mobsCombo.CBSelectIndex(0);
        }

        private void UpdateMobList(KPDatabaseDataSet dataSet, bool overrideGrouping)
        {
            mobsCombo.CBReset();
            mobsCombo.CBAddStrings(GetMobListing(dataSet, groupMobs, exclude0XPMobs));
        }
        #endregion

        #region Processing sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            ResetTextBox();

            IEnumerable<DefenseGroup> incAttacks;
            MobFilter mobFilter = mobsCombo.CBGetMobFilter();

            #region LINQ query
            incAttacks = from c in dataSet.Combatants
                         where ((c.CombatantType == (byte)EntityType.Player) ||
                               (c.CombatantType == (byte)EntityType.Pet) ||
                               (c.CombatantType == (byte)EntityType.Fellow))
                         orderby c.CombatantType, c.CombatantName
                         select new DefenseGroup
                         {
                             Name = c.CombatantName,
                             AllAttacks = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                          where ((da.HarmType == (byte)HarmType.Damage) ||
                                                 (da.HarmType == (byte)HarmType.Drain)) &&
                                                 mobFilter.CheckFilterMobActor(da) == true
                                          select da,
                             Melee = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                     where (((da.HarmType == (byte)HarmType.Damage) ||
                                             (da.HarmType == (byte)HarmType.Drain)) &&
                                            ((da.ActionType == (byte)ActionType.Melee) ||
                                             (da.ActionType == (byte)ActionType.Counterattack))) &&
                                            mobFilter.CheckFilterMobActor(da) == true
                                     select da,
                             Range = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                     where (((da.HarmType == (byte)HarmType.Damage) ||
                                             (da.HarmType == (byte)HarmType.Drain)) &&
                                            (da.ActionType == (byte)ActionType.Ranged)) &&
                                            mobFilter.CheckFilterMobActor(da) == true
                                     select da,
                             Abil = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                    where (((da.HarmType == (byte)HarmType.Damage) ||
                                            (da.HarmType == (byte)HarmType.Drain)) &&
                                           ((da.ActionType == (byte)ActionType.Ability) ||
                                            (da.ActionType == (byte)ActionType.Weaponskill))) &&
                                           mobFilter.CheckFilterMobActor(da) == true
                                    select da,
                             Spell = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                     where (((da.HarmType == (byte)HarmType.Damage) ||
                                             (da.HarmType == (byte)HarmType.Drain)) &&
                                            (da.ActionType == (byte)ActionType.Spell)) &&
                                            mobFilter.CheckFilterMobActor(da) == true
                                     select da,
                             Unknown = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                       where (((da.HarmType == (byte)HarmType.Damage) ||
                                               (da.HarmType == (byte)HarmType.Drain)) &&
                                              (da.ActionType == (byte)ActionType.Unknown)) &&
                                              mobFilter.CheckFilterMobActor(da) == true
                                       select da,
                             Retaliations = from da in c.GetInteractionsRowsByActorCombatantRelation()
                                            where da.ActionType == (byte)ActionType.Retaliation &&
                                                   mobFilter.CheckFilterMobActor(da) == true
                                            select da,
                         };
            #endregion

            if ((incAttacks != null) && (incAttacks.Count() > 0))
            {
                AppendText("Defense\n\n", Color.Red, true, false);

                switch (categoryCombo.CBSelectedIndex())
                {
                    case 0:
                        // All
                        ProcessDefenseAttacks(incAttacks);
                        ProcessDefenseDamage(incAttacks);
                        ProcessDefenseStandard(incAttacks);
                        ProcessDefenseOther(incAttacks);
                        ProcessUtsusemi(dataSet, mobFilter);
                        break;
                    case 1:
                        // Attacks
                        ProcessDefenseAttacks(incAttacks);
                        break;
                    case 2:
                        // Damage
                        ProcessDefenseDamage(incAttacks);
                        break;
                    case 3:
                        // Evasion
                        ProcessDefenseStandard(incAttacks);
                        break;
                    case 4:
                        // Other
                        ProcessDefenseOther(incAttacks);
                        break;
                    case 5:
                        // Utsusemi
                        ProcessUtsusemi(dataSet, mobFilter);
                        break;
                }

                AppendText("\n");
            }
        }

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

        private void ProcessDefenseDamage(IEnumerable<DefenseGroup> incAttacks)
        {
            //Player           M.Dmg   Avg M.Dmg   R.Dmg  Avg R.Dmg   S.Dmg  Avg S.Dmg   A/WS.Dmg  Avg A/WS.Dmg   Damage %

            int totalDmg = 0;
            playerDamage.Clear();
            foreach (var player in incAttacks)
            {
                playerDamage[player.Name] = player.Melee.Concat(player.Range.Concat(player.Spell.Concat(player.Abil))).
                    Sum(a => a.Amount);

                totalDmg += playerDamage[player.Name];
            }

            if (totalDmg > 0)
            {
                AppendText("Damage To:\n", Color.Blue, true, false);
                AppendText(incDamageHeader, Color.Black, true, true);

                StringBuilder sb = new StringBuilder();

                foreach (var player in incAttacks)
                {
                    if (playerDamage[player.Name] > 0)
                    {
                        sb.Append(player.Name.PadRight(16));
                        sb.Append(" ");

                        int mDmg = 0;
                        double mAvg = 0;
                        int rDmg = 0;
                        double rAvg = 0;
                        int sDmg = 0;
                        double sAvg = 0;
                        int aDmg = 0;
                        double aAvg = 0;

                        int numHits;

                        if (player.Melee.Count() > 0)
                        {
                            mDmg = player.Melee.Sum(a => a.Amount);
                            numHits = player.Melee.Count(a => a.DefenseType == (byte)DefenseType.None);
                            if (numHits > 0)
                                mAvg = (double)mDmg / numHits;
                        }

                        if (player.Range.Count() > 0)
                        {
                            rDmg = player.Range.Sum(a => a.Amount);
                            numHits = player.Range.Count(a => a.DefenseType == (byte)DefenseType.None);
                            if (numHits > 0)
                                rAvg = (double)rDmg / numHits;
                        }

                        if (player.Spell.Count() > 0)
                        {
                            sDmg = player.Spell.Sum(a => a.Amount);
                            numHits = player.Spell.Count(a => a.DefenseType == (byte)DefenseType.None);
                            if (numHits > 0)
                                sAvg = (double)sDmg / numHits;
                        }

                        if (player.Abil.Count() > 0)
                        {
                            aDmg = player.Abil.Sum(a => a.Amount);
                            numHits = player.Abil.Count(a => a.DefenseType == (byte)DefenseType.None);
                            if (numHits > 0)
                                aAvg = (double)aDmg / numHits;
                        }

                        double dmgPerc = 0;
                        if (totalDmg > 0)
                            dmgPerc = (double)playerDamage[player.Name] / totalDmg;

                        sb.AppendFormat("{0,5}{1,12:f2}{2,8}{3,11:f2}{4,8}{5,11:f2}{6,11}{7,14:f2}{8,11:p2}\n",
                            mDmg, mAvg, rDmg, rAvg, sDmg, sAvg, aDmg, aAvg, dmgPerc);
                    }
                }

                sb.Append("\n\n");
                AppendText(sb.ToString());
            }
        }

        private void ProcessDefenseStandard(IEnumerable<DefenseGroup> incAttacks)
        {
            bool headerPrinted = false;

            StringBuilder sb = new StringBuilder();

            foreach (var player in incAttacks)
            {
                if ((player.Melee.Count() + player.Range.Count()) > 0)
                {
                    int mEvaded = 0;
                    int rEvaded = 0;
                    int blinkedAttacks = 0;
                    int parriedAttacks = 0;
                    double mEvadePerc = 0;
                    double rEvadePerc = 0;
                    double blinkPerc = 0;
                    double parryPerc = 0;

                    var blinkableAttacks = player.Melee.Concat(
                                           player.Range.Concat(
                                           player.Spell.Concat(
                                           player.Abil.Concat(
                                           player.Unknown)))).Where(a =>
                            a.DefenseType != (byte)DefenseType.Evasion &&
                            a.DefenseType != (byte)DefenseType.Parry &&
                            a.DefenseType != (byte)DefenseType.Intimidate);

                    var parryableAttacks = player.Melee.Concat(
                                           player.Unknown).Where(a =>
                            a.DefenseType != (byte)DefenseType.Evasion);


                    int mEvadableCount = player.Melee.Count() + player.Unknown.Count();
                    int rEvadableCount = player.Range.Count();
                    int parryableCount = parryableAttacks.Count();
                    int blinkableCount = blinkableAttacks.Count();


                    if (player.Melee.Count() > 0)
                    {
                        mEvaded = player.Melee.Count(h => h.DefenseType == (byte)DefenseType.Evasion);
                        mEvadePerc = (double)mEvaded / mEvadableCount;
                    }

                    if (player.Range.Count() > 0)
                    {
                        rEvaded = player.Range.Count(h => h.DefenseType == (byte)DefenseType.Evasion);
                        rEvadePerc = (double)rEvaded / rEvadableCount;
                    }

                    if (parryableCount > 0)
                    {
                        parriedAttacks = parryableAttacks.Count(a => a.DefenseType == (byte)DefenseType.Parry);
                        parryPerc = (double)parriedAttacks / parryableCount;
                    }

                    if (blinkableCount > 0)
                    {
                        blinkedAttacks = blinkableAttacks.Count(a => a.DefenseType == (byte)DefenseType.Shadow);
                        blinkPerc = (double)blinkedAttacks / blinkableCount;
                    }


                    if ((mEvaded + rEvaded + blinkedAttacks) > 0)
                    {
                        if (headerPrinted == false)
                        {
                            AppendText("Standard Defenses\n", Color.Blue, true, false);
                            AppendText(standardDefHeader, Color.Black, true, true);

                            headerPrinted = true;
                        }

                        sb.AppendFormat("{0,-17}{1,7}{2,11:p2}{3,10}{4,11:p2}{5,9}{6,10:p2}{7,8}{8,9:p2}\n",
                            player.Name, mEvaded, mEvadePerc, rEvaded, rEvadePerc,
                            blinkedAttacks, blinkPerc, parriedAttacks, parryPerc);
                    }
                }
            }

            if (headerPrinted == true)
            {
                sb.Append("\n\n");
                AppendText(sb.ToString());
            }
        }

        private void ProcessDefenseOther(IEnumerable<DefenseGroup> incAttacks)
        {
            bool headerPrinted = false;

            StringBuilder sb = new StringBuilder();

            foreach (var player in incAttacks)
            {
                var anticableAttacks = player.Melee.Concat(
                                       player.Abil.Concat(
                                       player.Unknown)).Where(a =>
                    a.DefenseType != (byte)DefenseType.Evasion &&
                    a.DefenseType != (byte)DefenseType.Parry &&
                    a.DefenseType != (byte)DefenseType.Shadow &&
                    a.DefenseType != (byte)DefenseType.Intimidate);

                var counterableAttacks = player.Melee.Where(a =>
                    a.DefenseType != (byte)DefenseType.Evasion &&
                    a.DefenseType != (byte)DefenseType.Parry &&
                    a.DefenseType != (byte)DefenseType.Shadow &&
                    a.DefenseType != (byte)DefenseType.Intimidate).Concat(
                                         player.Unknown.Where(a =>
                                             a.DefenseType == (byte)DefenseType.Anticipate));

                var retaliableAttacks = player.Melee.Where(a =>
                    a.DefenseType == (byte)DefenseType.None);

                var intimidateableAttacks = player.Melee.Concat(player.Unknown);

                int anticibleCount = anticableAttacks.Count();
                int counterableCount = counterableAttacks.Count();
                int intimidatableCount = intimidateableAttacks.Count();
                int retaliableCount = retaliableAttacks.Count();

                int anticipatedAttacks = 0;
                int counteredAttacks = 0;
                int intimidatedAttacks = 0;
                int retaliatedAttacks = 0;

                double antiPerc = 0;
                double counterPerc = 0;
                double intimidatedPerc = 0;
                double retaliatedPerc = 0;


                if ((intimidatableCount + anticibleCount + counterableCount + retaliableCount) > 0)
                {
                    if (intimidatableCount > 0)
                    {
                        intimidatedAttacks = intimidateableAttacks.Count(a => a.DefenseType == (byte)DefenseType.Intimidate);
                        intimidatedPerc = (double)intimidatedAttacks / intimidatableCount;
                    }

                    if (anticibleCount > 0)
                    {
                        anticipatedAttacks = anticableAttacks.Count(a => a.DefenseType == (byte)DefenseType.Anticipate);
                        antiPerc = (double)anticipatedAttacks / anticibleCount;
                    }

                    if (counterableCount > 0)
                    {
                        counteredAttacks = counterableAttacks.Count(a => a.DefenseType == (byte)DefenseType.Counter);
                        counterPerc = (double)counteredAttacks / counterableCount;
                    }

                    if (retaliableCount > 0)
                    {
                        retaliatedAttacks = player.Retaliations.Count();
                        retaliatedPerc = (double)retaliatedAttacks / retaliableCount;
                    }


                    if ((intimidatedAttacks + anticipatedAttacks + counteredAttacks + retaliatedAttacks) > 0)
                    {
                        if (headerPrinted == false)
                        {
                            AppendText("Other Defenses\n", Color.Blue, true, false);
                            AppendText(otherDefHeader, Color.Black, true, true);
                            headerPrinted = true;
                        }

                        sb.Append(player.Name.PadRight(17));

                        sb.Append(intimidatedAttacks.ToString().PadLeft(10));
                        sb.Append(intimidatedPerc.ToString("P2").PadLeft(14));
                        sb.Append(anticipatedAttacks.ToString().PadLeft(13));
                        sb.Append(antiPerc.ToString("P2").PadLeft(14));
                        sb.Append(counteredAttacks.ToString().PadLeft(10));
                        sb.Append(counterPerc.ToString("P2").PadLeft(11));
                        sb.Append(retaliatedAttacks.ToString().PadLeft(12));
                        sb.Append(retaliatedPerc.ToString("P2").PadLeft(13));

                        sb.Append("\n");
                    }
                }
            }

            if (headerPrinted == true)
            {
                sb.Append("\n\n");
                AppendText(sb.ToString());
            }
        }

        private void ProcessUtsusemi(KPDatabaseDataSet dataSet, MobFilter mobFilter)
        {
            var utsu1 = dataSet.Actions.FirstOrDefault(a => a.ActionName == "Utsusemi: Ichi");
            var utsu2 = dataSet.Actions.FirstOrDefault(a => a.ActionName == "Utsusemi: Ni");

            if ((utsu1 == null) && (utsu2 == null))
                return;

            KPDatabaseDataSet.InteractionsRow[] utsu1Rows;
            KPDatabaseDataSet.InteractionsRow[] utsu2Rows;

            if (utsu1 != null)
                utsu1Rows = utsu1.GetInteractionsRows();
            else
                utsu1Rows = new KPDatabaseDataSet.InteractionsRow[0];

            if (utsu2 != null)
                utsu2Rows = utsu2.GetInteractionsRows();
            else
                utsu2Rows = new KPDatabaseDataSet.InteractionsRow[0];

            var utsuByPlayer = from c in dataSet.Combatants
                               where c.CombatantType == (byte)EntityType.Player
                               orderby c.CombatantName
                               select new
                               {
                                   Player = c.CombatantName,
                                   ShadowsUsed = from uc in c.GetInteractionsRowsByTargetCombatantRelation()
                                                 where ((uc.DefenseType == (byte)DefenseType.Shadow) &&
                                                        (uc.ShadowsUsed > 0)) &&
                                                        mobFilter.CheckFilterMobBattle(uc)
                                                 select uc,
                                   UtsuIchi = from i in utsu1Rows
                                              where (i.CombatantsRowByActorCombatantRelation == c) &&
                                                     mobFilter.CheckFilterMobBattle(i)
                                              select i,
                                   UtsuNi = from i in utsu2Rows
                                            where (i.CombatantsRowByActorCombatantRelation == c) &&
                                                   mobFilter.CheckFilterMobBattle(i)
                                            select i,
                               };


            int shadsUsed;
            int ichiCast;
            int niCast;
            int ichiFin;
            int niFin;
            int numShads;
            int numShadsN;
            double effNorm;
            double effNin;


            if (utsuByPlayer.Count() > 0)
            {
                AppendText("Utsusemi\n\n", Color.Red, true, false);
                AppendText(utsuHeader, Color.Black, true, true);

                StringBuilder sb = new StringBuilder();

                foreach (var player in utsuByPlayer)
                {
                    shadsUsed = 0;
                    ichiCast = 0;
                    niCast = 0;
                    ichiFin = 0;
                    niFin = 0;
                    numShads = 0;
                    numShadsN = 0;
                    effNorm = 0;
                    effNin = 0;

                    shadsUsed = player.ShadowsUsed.Sum(u => u.ShadowsUsed);

                    if (player.UtsuIchi != null)
                    {
                        ichiCast = player.UtsuIchi.Count(u => u.Preparing == true);
                        ichiFin = player.UtsuIchi.Count(u => u.Preparing == false);
                    }

                    if (player.UtsuNi != null)
                    {
                        niCast = player.UtsuNi.Count(u => u.Preparing == true);
                        niFin = player.UtsuNi.Count(u => u.Preparing == false);
                    }

                    numShads = ichiFin * 3 + niFin * 3;
                    numShadsN = ichiFin * 3 + niFin * 4;

                    if (numShads > 0)
                    {
                        effNorm = (double)shadsUsed / numShads;
                        effNin = (double)shadsUsed / numShadsN;
                    }

                    if ((numShads + shadsUsed + ichiCast + niCast) > 0)
                    {
                        sb.Append(player.Player.PadRight(16));
                        sb.Append(" ");

                        sb.Append(shadsUsed.ToString().PadLeft(12));
                        sb.Append(ichiCast.ToString().PadLeft(12));
                        sb.Append(ichiFin.ToString().PadLeft(10));
                        sb.Append(niCast.ToString().PadLeft(9));
                        sb.Append(niFin.ToString().PadLeft(8));
                        sb.Append(numShads.ToString().PadLeft(8));
                        sb.Append(numShadsN.ToString().PadLeft(10));
                        sb.Append(effNorm.ToString("P2").PadLeft(12));
                        sb.Append(effNin.ToString("P2").PadLeft(11));

                        sb.Append("\n");
                    }
                }

                sb.Append("\n");
                AppendText(sb.ToString());
            }
        }
        #endregion

        #region Event Handlers
        protected void categoryCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(DatabaseManager.Instance.Database);

            flagNoUpdate = false;
        }

        protected void mobsCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(DatabaseManager.Instance.Database);

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
                flagNoUpdate = true;
                UpdateMobList();

                HandleDataset(DatabaseManager.Instance.Database);
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
                flagNoUpdate = true;
                UpdateMobList();

                HandleDataset(DatabaseManager.Instance.Database);
            }

            flagNoUpdate = false;
        }

        #endregion

    }
}
