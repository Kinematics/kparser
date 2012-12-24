using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WaywardGamers.KParser.Database;

namespace WaywardGamers.KParser.Plugin
{
    public partial class BasePluginControl : UserControl, IPlugin
    {
        #region Protected Variables
        protected string tabName;
        protected RichTextBox dummyRichTextBox = new RichTextBox();
        #endregion

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

            IsActive = false;
            MobXPHandler.Instance.CustomMobFilterChanged += this.CustomMobFilterChanged;

            // Don't call this during the base constructor.  Let the plugins call it themselves.
            // While it has the same effect, putting it in the constructor allows for
            // an explicit reminder of what needs to be set.
            //LoadLocalizedUI();
            LoadResources();
        }
        #endregion

        #region Properties to be set by main window
        public bool IsActive { get; set; }
        #endregion

        #region IPlugin Members

        public virtual string TabName
        {
            get { return tabName; }
        }

        public UserControl Control
        {
            get { return (this as UserControl); }
        }

        public string TextContents
        {
            get { return this.richTextBox.Text; }
        }

        public string TextContentsAsRTF
        {
            get { return this.richTextBox.Rtf; }
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

        public void NotifyOfCultureChange()
        {
            HandleCultureChange();

            if (IsActive)
                NotifyOfUpdate();
        }

        public virtual void NotifyOfUpdate()
        {
            ResetTextBox();
            HandleDataset(null);
        }

        public virtual void CustomMobFilterChanged(object sender, EventArgs e)
        {
            if (IsActive)
            {
                OnCustomMobFilterChanged();
            }
        }

        protected virtual void OnCustomMobFilterChanged()
        {
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

                //BeginInvoke(handleDataset, dbChanges);
                Invoke(handleDataset, dbChanges);
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

                    if (databaseChanges == null)
                    {
                        using (AccessToTheDatabase dbAccess = new AccessToTheDatabase())
                        {
                            if (dbAccess.HasAccess)
                                ProcessData(dbAccess.Database);
                        }
                    }
                    else
                    {
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

        /// <summary>
        /// Call the available overridable functions for reloading localizable elements
        /// for each plugin.
        /// </summary>
        protected virtual void HandleCultureChange()
        {
            LoadResources();
            LoadLocalizedUI();
        }

        /// <summary>
        /// Overridable function to allow reloading resource strings for general elements.
        /// </summary>
        protected virtual void LoadResources()
        {
        }

        /// <summary>
        /// Overridable function to allow reloading resource strings for UI elements.
        /// </summary>
        protected virtual void LoadLocalizedUI()
        {
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

        /// <summary>
        /// Function to update the rich text box with the specified string, and color/adjust fonts
        /// based on the included list of StringMods.
        /// </summary>
        /// <param name="sb">StringBuilder containing the string to add to the rich text box.</param>
        /// <param name="strModList">List of adjustments to the output for setting font colors/etc.</param>
        protected void PushStrings(StringBuilder sb, List<StringMods> strModList)
        {
            // Supplant with quick update version of this function.
            PushStringsQ(sb, strModList);
            return;

            /*
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
            */
        }

        /// <summary>
        /// A 'quick' version of the PushStrings function, this does all the updating work
        /// on a hidden rich text box to avoid constant visible repainting.  It's quick
        /// only in the sense that the user doesn't have to see the visible updates.  The
        /// process of adding/coloring/etc the text takes just as long as the normal version.
        /// Once that's complete, though, the update to the visible window is nearly instant.
        /// </summary>
        /// <param name="sb">StringBuilder containing the string to add to the rich text box.</param>
        /// <param name="strModList">List of adjustments to the output for setting font colors/etc.</param>
        protected void PushStringsQ(StringBuilder sb, List<StringMods> strModList)
        {
            dummyRichTextBox.Clear();
            dummyRichTextBox.Rtf = richTextBox.Rtf;

            int start = dummyRichTextBox.Text.Length;

            dummyRichTextBox.AppendText(sb.ToString());
            dummyRichTextBox.Select(start, sb.Length);
            dummyRichTextBox.SelectionFont = normFont;
            dummyRichTextBox.SelectionColor = Color.Black;

            if (strModList != null)
            {
                foreach (var strMod in strModList)
                {
                    dummyRichTextBox.Select(strMod.Start + start, strMod.Length);

                    if ((strMod.Bold == true) && (strMod.Underline == true))
                    {
                        dummyRichTextBox.SelectionFont = buFont;
                    }
                    else if (strMod.Bold == true)
                    {
                        dummyRichTextBox.SelectionFont = boldFont;
                    }
                    else if (strMod.Underline == true)
                    {
                        dummyRichTextBox.SelectionFont = underFont;
                    }
                    //else
                    //{
                    //    dummyRichTextBox.SelectionFont = normFont;
                    //}

                    dummyRichTextBox.SelectionColor = strMod.Color;
                }
            }
            
            richTextBox.Rtf = dummyRichTextBox.Rtf;
            richTextBox.Select(0, 0);
        }

        /// <summary>
        /// Preliminary work on constructing the raw underlying RTF code from scratch
        /// rather than updaing the background rich text box.  Updating manually and
        /// then setting the text box's RTF would be vastly faster than running a loop
        /// calling the control for each update, but will also be vastly more complicated
        /// to implement, especially for dealing with the various fonts (mainly for
        /// the chat window, but also for any localized version).
        /// </summary>
        /// <param name="sb">StringBuilder containing the string to add to the rich text box.</param>
        /// <param name="strModList">List of adjustments to the output for setting font colors/etc.</param>
        protected void PushStringsQM(StringBuilder sb, List<StringMods> strModList)
        {
            dummyRichTextBox.Clear();
            dummyRichTextBox.Rtf = richTextBox.Rtf;

            int start = dummyRichTextBox.Text.Length;

            dummyRichTextBox.AppendText(sb.ToString());
            dummyRichTextBox.Select(start, sb.Length);
            dummyRichTextBox.SelectionFont = normFont;
            dummyRichTextBox.SelectionColor = Color.Black;

            if (strModList != null)
            {
                foreach (var strMod in strModList)
                {
                    dummyRichTextBox.Select(strMod.Start + start, strMod.Length);

                    if ((strMod.Bold == true) && (strMod.Underline == true))
                    {
                        dummyRichTextBox.SelectionFont = buFont;
                    }
                    else if (strMod.Bold == true)
                    {
                        dummyRichTextBox.SelectionFont = boldFont;
                    }
                    else if (strMod.Underline == true)
                    {
                        dummyRichTextBox.SelectionFont = underFont;
                    }
                    else
                    {
                        dummyRichTextBox.SelectionFont = normFont;
                    }

                    dummyRichTextBox.SelectionColor = strMod.Color;
                }
            }

            string rawRTF = dummyRichTextBox.Rtf;

            List<Color> colorList = GetRTFColorTableColors(rawRTF);
            AddModListColors(colorList, strModList);
            string rtfColorTable = GenerateRTFColorTable(colorList);

            richTextBox.Rtf = dummyRichTextBox.Rtf;
            richTextBox.Select(0, 0);
        }

        private string GenerateRTFColorTable(List<Color> colorList)
        {
            StringBuilder sb = new StringBuilder();

            // {\\colortbl ;\\red128\\green0\\blue128;\\red0\\green0\\blue0;}

            sb.Append("{\\colortbl ;");

            foreach (var color in colorList)
            {
                sb.AppendFormat("\\red{0}\\green{1}\\blue{2};", color.R, color.G, color.B);
            }

            sb.Append("}");

            return sb.ToString();
        }

        private List<Color> GetRTFColorTableColors(string rawRTF)
        {
            List<Color> colorList = new List<Color>();

            // {\\colortbl ;\\red128\\green0\\blue128;\\red0\\green0\\blue0;}

            Regex colorTableRegex = new Regex(@"\{\\colortbl\s?;(?<colors>(\\red\d{1,3}\\green\d{1,3}\\blue\d{1,3};)+)}");
            Match colorTableRegexMatch = colorTableRegex.Match(rawRTF);

            Regex colorEntry = new Regex(@"(?<aColor>\\red\d{1,3}\\green\d{1,3}\\blue\d{1,3};)(?<remainder>.*)");
            Match colorEntryMatch;

            Regex colorBreakdown = new Regex(@"\\red(?<red>\d{1,3})\\green(?<green>\d{1,3})\\blue(?<blue>\d{1,3});");

            if (colorTableRegexMatch.Success)
            {
                string colorSets = colorTableRegexMatch.Groups["colors"].Value;

                colorEntryMatch = colorEntry.Match(colorSets);
                while (colorEntryMatch.Success)
                {
                    string aColor = colorEntryMatch.Groups["aColor"].Value;
                    Match aColorBreakdown = colorBreakdown.Match(aColor);

                    if (aColorBreakdown.Success)
                    {
                        int red = int.Parse(aColorBreakdown.Groups["red"].Value);
                        int green = int.Parse(aColorBreakdown.Groups["green"].Value);
                        int blue = int.Parse(aColorBreakdown.Groups["blue"].Value);

                        Color color = Color.FromArgb(red, green, blue);

                        colorList.Add(color);
                    }

                    colorSets = colorEntryMatch.Groups["remainder"].Value;
                    colorEntryMatch = colorEntry.Match(colorSets);
                }
            }

            return colorList;
        }

        private void AddModListColors(List<Color> colorList, List<StringMods> strModList)
        {
            var modListColors = from m in strModList
                                group m by m.Color;

            foreach (var q in modListColors)
            {
                // Any colors imported from the raw RTF don't have names, so
                // convert named colors to raw RGB colors so that the hash
                // set can avoid duplicates easily.
                // -- Now converted on creation of the StringMod object.
                //Color simpleColor = Color.FromArgb(q.Key.R, q.Key.G, q.Key.B);
                
                if (colorList.Contains(q.Key) == false)
                    colorList.Add(q.Key);
            }
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
                if (dbAccess.HasAccess)
                {
                    var speakers = from s in dbAccess.Database.ChatSpeakers
                                   orderby s.SpeakerName
                                   select s.SpeakerName;

                    foreach (var speaker in speakers)
                        speakerStrings.Add(speaker);
                }
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
            playerStrings.Add(Resources.PublicResources.All);

            using (Database.AccessToTheDatabase dbAccess = new AccessToTheDatabase())
            {
                if (dbAccess.HasAccess)
                {
                    var playersFighting = from b in dbAccess.Database.Combatants
                                          where (((EntityType)b.CombatantType == EntityType.Player ||
                                                 (EntityType)b.CombatantType == EntityType.Pet ||
                                                 (EntityType)b.CombatantType == EntityType.Fellow) &&
                                                 b.GetInteractionsRowsByActorCombatantRelation().Any() == true)
                                          orderby b.CombatantName
                                          select b.CombatantName;

                    foreach (var player in playersFighting)
                        playerStrings.Add(player);
                }
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
            mobStrings.Add(Resources.PublicResources.All);

            // Group enemies check

            using (Database.AccessToTheDatabase dbAccess = new AccessToTheDatabase())
            {
                if (dbAccess.HasAccess)
                {
                    if (groupMobs == true)
                    {
                        // Enemy group listing

                        var mobsKilled = from b in dbAccess.Database.Battles
                                         where ((b.DefaultBattle == false) &&
                                                (b.IsEnemyIDNull() == false) &&
                                                ((EntityType)b.CombatantsRowByEnemyCombatantRelation.CombatantType == EntityType.Mob))
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
            }

            return mobStrings.ToArray();
        }
        #endregion

        #region Event Handlers
        private void richTextBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
        #endregion
    }

    public static class BasePluginExtensionMethods
    {
        public static void UpdateWithSpeakerList(this ToolStripComboBox combo)
        {
            if (combo.ComboBox.InvokeRequired)
            {
                Action<ToolStripComboBox> thisFunc = UpdateWithSpeakerList;
                combo.ComboBox.Invoke(thisFunc, new object[] { combo });
                return;
            }

            string[] currentSpeakerList = new string[combo.Items.Count];
            combo.Items.CopyTo(currentSpeakerList, 0);


            List<string> speakerStrings = new List<string>();
            speakerStrings.Add(Resources.PublicResources.All);

            using (Database.AccessToTheDatabase dbAccess = new AccessToTheDatabase())
            {
                if (dbAccess.HasAccess)
                {
                    var speakers = from s in dbAccess.Database.ChatSpeakers
                                   orderby s.SpeakerName
                                   select s.SpeakerName;

                    foreach (var speaker in speakers)
                        speakerStrings.Add(speaker);
                }
            }

            string[] newSpeakerList = speakerStrings.ToArray();

            if (Array.Equals(currentSpeakerList, newSpeakerList) == true)
                return;

            combo.Items.Clear();
            combo.Items.AddRange(newSpeakerList);
        }

        public static void UpdateWithPlayerList(this ToolStripComboBox combo)
        {
            if (combo.ComboBox.InvokeRequired)
            {
                Action<ToolStripComboBox> thisFunc = UpdateWithPlayerList;
                combo.ComboBox.Invoke(thisFunc, new object[] { combo });
                return;
            }

            string[] currentPlayerList = new string[combo.Items.Count];
            combo.Items.CopyTo(currentPlayerList, 0);


            List<string> playerStrings = new List<string>();
            playerStrings.Add(Resources.PublicResources.All);

            using (Database.AccessToTheDatabase dbAccess = new AccessToTheDatabase())
            {
                if (dbAccess.HasAccess)
                {
                    var playersFighting = from b in dbAccess.Database.Combatants
                                          where (((EntityType)b.CombatantType == EntityType.Player ||
                                                 (EntityType)b.CombatantType == EntityType.Pet ||
                                                 (EntityType)b.CombatantType == EntityType.Fellow ||
                                                 (EntityType)b.CombatantType == EntityType.CharmedMob) &&
                                                 b.GetInteractionsRowsByActorCombatantRelation().Any() == true)
                                          orderby b.CombatantName
                                          select b.CombatantName;

                    foreach (var player in playersFighting)
                        playerStrings.Add(player);
                }
            }

            string[] newPlayerList = playerStrings.ToArray();


            if (Array.Equals(currentPlayerList, newPlayerList) == true)
                return;


            combo.Items.Clear();

            combo.Items.AddRange(newPlayerList);
        }

        public static void UpdateWithMobList(this ToolStripComboBox combo, bool groupMobs, bool exclude0XPMobs)
        {
            if (combo.ComboBox.InvokeRequired)
            {
                Action<ToolStripComboBox, bool, bool> thisFunc = UpdateWithMobList;
                combo.ComboBox.Invoke(thisFunc, new object[] { combo, groupMobs, exclude0XPMobs });
                return;
            }

            string[] currentMobList = new string[combo.Items.Count];
            combo.Items.CopyTo(currentMobList, 0);

            MobXPHandler.Instance.Update();

            List<string> mobStrings = new List<string>();
            mobStrings.Add(Resources.PublicResources.All);

            if (MobXPHandler.Instance.CompleteMobList.Count > 0)
            {
                if (groupMobs == true)
                {
                    // Enemy group listing

                    var mobsKilledX = from b in MobXPHandler.Instance.CompleteMobList
                                      orderby b.Name
                                      group b by b.Name into bn
                                      select new
                                      {
                                          Name = bn.Key,
                                          XP = from xb in bn
                                               group xb by xb.BaseXP into xbn
                                               orderby xbn.Key
                                               select xbn
                                      };


                    foreach (var mob in mobsKilledX)
                    {
                        if (mob.XP.Count() > 1)
                        {
                            if (exclude0XPMobs == true)
                            {
                                if (mob.XP.Any(x => x.Key > 0) == true)
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
                                if (xp.Key > 0)
                                    mobStrings.Add(string.Format("{0} ({1})", mob.Name, xp.Key));
                            }
                            else
                            {
                                mobStrings.Add(string.Format("{0} ({1})", mob.Name, xp.Key));
                            }
                        }

                    }

                }
                else
                {
                    // Enemy battle listing

                    var mobsKilled = from b in MobXPHandler.Instance.CompleteMobList
                                     orderby b.BattleID
                                     select b;

                    foreach (var mob in mobsKilled)
                    {
                        if (exclude0XPMobs == true)
                        {
                            if (mob.XP > 0)
                                mobStrings.Add(string.Format("{0,3}: {1}", mob.BattleID, mob.Name));
                        }
                        else
                        {
                            mobStrings.Add(string.Format("{0,3}: {1}", mob.BattleID, mob.Name));
                        }
                    }

                }
            }

            string[] newMobList = mobStrings.ToArray();

            if (Array.Equals(currentMobList, newMobList) == true)
                return;

            combo.Items.Clear();
            combo.Items.AddRange(newMobList);
        }
    }
}
