using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WaywardGamers.KParser.Database;

namespace WaywardGamers.KParser.Forms
{
    public partial class PlayerInfoDlg : Form
    {
        PlayerInfo[] playerDataList;
        string databaseFilename;
        ParserWindow parentWindow;

        #region Constructor
        public PlayerInfoDlg(ParserWindow parentWin)
        {
            InitializeComponent();

            parentWindow = parentWin;

            // Load information from the database and work with it
            // in a disconnected state.

            if (DatabaseManager.Instance.IsDatabaseOpen == false)
                throw new InvalidOperationException();

            databaseFilename = DatabaseManager.Instance.DatabaseFilename;

            using (AccessToTheDatabase db = new AccessToTheDatabase("PlayerInfo"))
            {
                if (db.Database.Combatants.Count > 0)
                {
                    var playerData = from com in db.Database.Combatants
                                     where ((EntityType)com.CombatantType == EntityType.Player ||
                                        (EntityType)com.CombatantType == EntityType.Pet ||
                                        (EntityType)com.CombatantType == EntityType.Fellow ||
                                        (EntityType)com.CombatantType == EntityType.CharmedMob)
                                     orderby com.CombatantName
                                     select new PlayerInfo
                                     {
                                         Name = com.CombatantName,
                                         CombatantType = (EntityType)com.CombatantType,
                                         Info = com.PlayerInfo
                                     };

                    // Put the acquired data in the listbox

                    playerDataList = playerData.ToArray();

                    combatantListBox.Items.Clear();
                    foreach (var player in playerDataList)
                    {
                        combatantListBox.Items.Add(player.Name);
                    }

                    combatantListBox.SelectedIndex = 0;
                }
            }
        }
        #endregion

        #region Event handlers
        private void ok_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void combatantListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var player = playerDataList[combatantListBox.SelectedIndex];

            combatantType.Text = player.CombatantType.ToString();

            combatantDescription.Text = player.Info;
        }

        private void combatantDescription_TextChanged(object sender, EventArgs e)
        {
            var player = playerDataList[combatantListBox.SelectedIndex];

            player.Info = combatantDescription.Text;
        }

        private void PlayerInfo_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (e.CloseReason == CloseReason.UserClosing ||
                    e.CloseReason == CloseReason.None)
                {
                    if (this.DialogResult == DialogResult.OK)
                    {
                        if (playerDataList.Length > 0)
                        {
                            // Make sure the database is still open
                            if (DatabaseManager.Instance.IsDatabaseOpen == false)
                            {
                                MessageBox.Show("The parse file is no longer open.", "Cannot save",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            // Make sure it's the same database file as originally loaded
                            if (DatabaseManager.Instance.DatabaseFilename != databaseFilename)
                            {
                                MessageBox.Show("The current parse file is not the same one as was used to open this dialog.",
                                    "Cannot save", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            List<PlayerInfo> playerInfoList = playerDataList.ToList<PlayerInfo>();
                            DatabaseManager.Instance.UpdatePlayerInfo(playerInfoList);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
            finally
            {
                parentWindow.RemoveMonitorChanging();
            }
        }
        #endregion

    }
}
