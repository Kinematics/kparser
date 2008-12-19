using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WaywardGamers.KParser.Database;

namespace WaywardGamers.KParser.Plugin
{
    public partial class BasePluginControl : UserControl, IPlugin
    {
        #region Font Variables
        protected Font normFont;
        protected Font boldFont;
        protected Font underFont;
        protected Font buFont;
        #endregion

        #region Constructor
        private object privateLock = new object();

        public BasePluginControl()
        {
            InitializeComponent();

            normFont = new Font(FontFamily.GenericMonospace, 9.00f, FontStyle.Regular);
            boldFont = new Font(FontFamily.GenericMonospace, 9.00f, FontStyle.Bold);
            underFont = new Font(FontFamily.GenericMonospace, 9.00f, FontStyle.Underline);
            buFont = new Font(FontFamily.GenericMonospace, 9.00f, FontStyle.Bold | FontStyle.Underline);
        }
        #endregion

        #region IPlugin Members

        public virtual string TabName
        {
            get { throw new NotImplementedException(); }
        }

        public UserControl Control
        {
            get { return (this as UserControl); }
        }

        public string TextContents
        {
            get { return this.richTextBox.Text; }
        }

        public virtual DataTable GeneratedDataTableForExcel
        {
            get { return null; }
        }

        public virtual void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            //HandleDataset(e.DatasetChanges);
        }

        public virtual void WatchDatabaseChanged(object sender, DatabaseWatchEventArgs e)
        {
            //HandleDataset(null);
        }

        public virtual void NotifyOfUpdate()
        {
            ResetTextBox();
            HandleDataset(null);
        }

        public virtual void Reset()
        {
            throw new NotImplementedException();
        }

        public virtual bool IsDebug
        {
            get { return false; }
        }

        #endregion

        #region Accessory IPlugin Functions
        protected void HandleDataset(KPDatabaseDataSet databaseChanges)
        {
            if (InvokeRequired)
            {
                Action<KPDatabaseDataSet> handleDataset = new Action<KPDatabaseDataSet>(HandleDataset);
                object[] dbChanges = new object[1] { databaseChanges };

                BeginInvoke(handleDataset, dbChanges);
                //Invoke(reReadDatabase, passDataset);
                return;
            }

            // Lock in case a UI update sends a message to start reprocessing
            // at the same time as the code is processing a message from the
            // database being updated, or vice versa.
            lock (privateLock)
            {
                try
                {
                    // Retain current cursor position and/or selection
                    int currentSelectionStart = richTextBox.SelectionStart;
                    int currentSelectionLength = richTextBox.SelectionLength;

                    richTextBox.SuspendLayout();

                    using (AccessToTheDatabase dbAccess = new AccessToTheDatabase())
                    {
                        if (databaseChanges == null)
                            ProcessData(dbAccess.Database);
                        else
                            ProcessData(databaseChanges);
                    }

                    // After processing, re-select the same section as before, or
                    // set it to the start of the display.
                    if (currentSelectionStart <= richTextBox.Text.Length)
                        richTextBox.Select(currentSelectionStart, currentSelectionLength);
                    else
                        richTextBox.Select(0, 0);

                    //richTextBox.Select(richTextBox.Text.Length, richTextBox.Text.Length);

                    richTextBox.ResumeLayout();
                }
                catch (Exception e)
                {
                    Logger.Instance.Log(e);
                    MessageBox.Show("Error while processing plugin: \n" + e.Message);
                }
            }
        }

        protected virtual void ProcessData(KPDatabaseDataSet dataSet)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Helper functions for RTF TextBox
        protected void ResetTextBox()
        {
            if (this.InvokeRequired)
            {
                Action thisFunc = ResetTextBox;
                Invoke(thisFunc);
                return;
            }

            this.richTextBox.Clear();
        }

        protected void AppendText(string textToInsert)
        {
            AppendText(textToInsert, Color.Black, false, false);
        }

        protected void AppendText(string textToInsert, Color color)
        {
            AppendText(textToInsert, color, false, false);
        }

        protected void AppendText(string textToInsert, bool bold, bool underline)
        {
            AppendText(textToInsert, Color.Black, bold, underline);
        }

        protected void AppendText(string textToInsert, Color color, bool bold, bool underline)
        {
            if (this.InvokeRequired)
            {
                Action<string, Color, bool, bool> thisFunc = AppendText;
                Invoke(thisFunc, new object[] { textToInsert, color, bold, underline });
                return;
            }

            int start = richTextBox.Text.Length;
            richTextBox.AppendText(textToInsert);
            richTextBox.Select(start, textToInsert.Length);

            richTextBox.SelectionColor = color;

            // Set font to use
            if ((bold == true) && (underline == true))
            {
                richTextBox.SelectionFont = buFont;
            }
            else if (bold == true)
            {
                richTextBox.SelectionFont = boldFont;
            }
            else if (underline == true)
            {
                richTextBox.SelectionFont = underFont;
            }
            else
            {
                richTextBox.SelectionFont = normFont;
            }

            richTextBox.Select(0, 0);
        }

        protected void PushStrings(StringBuilder sb, List<StringMods> strModList)
        {
            int start = richTextBox.Text.Length;

            richTextBox.AppendText(sb.ToString());
            richTextBox.Select(start, sb.Length);
            richTextBox.SelectionFont = normFont;
            richTextBox.SelectionColor = Color.Black;

            if (strModList != null)
            {
                foreach (var strMod in strModList)
                {
                    richTextBox.Select(strMod.Start + start, strMod.Length);

                    if ((strMod.Bold == true) && (strMod.Underline == true))
                    {
                        richTextBox.SelectionFont = buFont;
                    }
                    else if (strMod.Bold == true)
                    {
                        richTextBox.SelectionFont = boldFont;
                    }
                    else if (strMod.Underline == true)
                    {
                        richTextBox.SelectionFont = underFont;
                    }
                    else
                    {
                        richTextBox.SelectionFont = normFont;
                    }

                    richTextBox.SelectionColor = strMod.Color;
                }
            }

            richTextBox.Select(0, 0);
        }
        #endregion

        #region General helper functions
        /// <summary>
        /// Gets a list of all speakers in the chat table.
        /// </summary>
        /// <returns></returns>
        protected string[] GetSpeakerListing()
        {
            List<string> speakerStrings = new List<string>();
            speakerStrings.Add("All");

            using (Database.AccessToTheDatabase dbAccess = new AccessToTheDatabase())
            {
                var speakers = from s in dbAccess.Database.ChatSpeakers
                               orderby s.SpeakerName
                               select new
                               {
                                   Name = s.SpeakerName
                               };

                foreach (var speaker in speakers)
                    speakerStrings.Add(speaker.Name);
            }

            return speakerStrings.ToArray();
        }

        /// <summary>
        /// Gets a string array of all players & pets among the combatants.
        /// </summary>
        /// <returns>A string array.</returns>
        protected string[] GetPlayerListing()
        {
            List<string> playerStrings = new List<string>();
            playerStrings.Add("All");

            using (Database.AccessToTheDatabase dbAccess = new AccessToTheDatabase())
            {
                var playersFighting = from b in dbAccess.Database.Combatants
                                      where ((b.CombatantType == (byte)EntityType.Player ||
                                             b.CombatantType == (byte)EntityType.Pet ||
                                             b.CombatantType == (byte)EntityType.Fellow) &&
                                             b.GetInteractionsRowsByActorCombatantRelation().Any() == true)
                                      orderby b.CombatantName
                                      select new
                                      {
                                          Name = b.CombatantName
                                      };

                foreach (var player in playersFighting)
                    playerStrings.Add(player.Name);
            }

            return playerStrings.ToArray();
        }

        /// <summary>
        /// Gets a string array of mob names with formatting for either fight number or XP gained.
        /// </summary>
        /// <param name="groupMobs">Whether mobs should be grouped by name/XP.</param>
        /// <param name="exclude0XPMobs">Whether to include mobs that gave no XP.</param>
        /// <returns>A string array.</returns>
        protected string[] GetMobListing(bool groupMobs, bool exclude0XPMobs)
        {
            List<string> mobStrings = new List<string>();
            mobStrings.Add("All");

            // Group enemies check

            using (Database.AccessToTheDatabase dbAccess = new AccessToTheDatabase())
            {
                if (groupMobs == true)
                {
                    // Enemy group listing

                    var mobsKilled = from b in dbAccess.Database.Battles
                                     where ((b.DefaultBattle == false) &&
                                            (b.IsEnemyIDNull() == false) &&
                                            (b.CombatantsRowByEnemyCombatantRelation.CombatantType == (byte)EntityType.Mob))
                                     orderby b.CombatantsRowByEnemyCombatantRelation.CombatantName
                                     group b by b.CombatantsRowByEnemyCombatantRelation.CombatantName into bn
                                     select new
                                     {
                                         Name = bn.Key,
                                         XP = from xb in bn
                                              group xb by xb.MinBaseExperience() into xbn
                                              orderby xbn.Key
                                              select new { BaseXP = xbn.Key }
                                     };

                    foreach (var mob in mobsKilled)
                    {
                        if (mob.XP.Count() > 1)
                        {
                            if (exclude0XPMobs == true)
                            {
                                if (mob.XP.Any(x => x.BaseXP > 0) == true)
                                    mobStrings.Add(mob.Name);
                            }
                            else
                            {
                                mobStrings.Add(mob.Name);
                            }
                        }

                        foreach (var xp in mob.XP)
                        {
                            if (exclude0XPMobs == true)
                            {
                                if (xp.BaseXP > 0)
                                    mobStrings.Add(string.Format("{0} ({1})", mob.Name, xp.BaseXP));
                            }
                            else
                            {
                                mobStrings.Add(string.Format("{0} ({1})", mob.Name, xp.BaseXP));
                            }
                        }
                    }
                }
                else
                {
                    // Enemy battle listing

                    var mobsKilled = from b in dbAccess.Database.Battles
                                     where ((b.DefaultBattle == false) &&
                                            (b.IsEnemyIDNull() == false) &&
                                            ((EntityType)b.CombatantsRowByEnemyCombatantRelation.CombatantType == EntityType.Mob))
                                     orderby b.BattleID
                                     select new
                                     {
                                         Name = b.CombatantsRowByEnemyCombatantRelation.CombatantName,
                                         Battle = b.BattleID,
                                         XP = b.BaseExperience()
                                     };

                    foreach (var mob in mobsKilled)
                    {
                        if (exclude0XPMobs == true)
                        {
                            if (mob.XP > 0)
                                mobStrings.Add(string.Format("{0,3}: {1}", mob.Battle, mob.Name));
                        }
                        else
                        {
                            mobStrings.Add(string.Format("{0,3}: {1}", mob.Battle, mob.Name));
                        }
                    }
                }
            }

            return mobStrings.ToArray();
        }
        #endregion
    }
}
