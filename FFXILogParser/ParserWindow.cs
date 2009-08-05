using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;
using WaywardGamers.KParser.Plugin;
using WaywardGamers.KParser.Database;
using WaywardGamers.KParser.Forms;
using WaywardGamers.KParser.Monitoring;

namespace WaywardGamers.KParser
{
    public partial class ParserWindow : Form
    {
        #region Main Entry Point
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.Run(new ParserWindow());
            }
            catch (Exception e)
            {
                Logger.Instance.FatalLog(e);
            }
        }
        #endregion

        #region Member Variables
        private string applicationDirectory;
        private string defaultSaveDirectory;

        Properties.WindowSettings windowSettings = new WaywardGamers.KParser.Properties.WindowSettings();
        Properties.Settings appSettings = new WaywardGamers.KParser.Properties.Settings();

        private List<IPlugin> pluginList = new List<IPlugin>();
        private List<TabPage> tabList = new List<TabPage>();

        TabPage currentTab = null;

        private ImportMode ReparseMode;
        private string revertToThisDatabaseFile = string.Empty;
        bool reparseComplete;

        ToolStripStatusLabel reparseState = new ToolStripStatusLabel();
        ToolStripProgressBar reparseProgress = new ToolStripProgressBar();

        #endregion

        #region Constructor
        public ParserWindow()
        {
            InitializeComponent();

            applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        #endregion

        #region Load/Close Event handlers for saving window state
        private void ParserWindow_Load(object sender, EventArgs e)
        {
            // Upgrade settings files if necessary
            if (windowSettings.UpgradeRequired)
            {
                windowSettings.Upgrade();
                windowSettings.UpgradeRequired = false;
                windowSettings.Save();
            }

            if (appSettings.UpgradeRequired)
            {
                appSettings.Upgrade();
                appSettings.UpgradeRequired = false;
                appSettings.Save();
            }

            // End upgrade

            // Restore main window position and size.

            this.Size = windowSettings.mainWindowSize;
            this.Location = windowSettings.mainWindowPosition;
            if (windowSettings.mainWindowMaximized == true)
                this.WindowState = FormWindowState.Maximized;
            else
                this.WindowState = FormWindowState.Normal;

            // Verify that the window hasn't been pushed out of visibility.

            Screen workingScreen = Screen.FromControl(this);

            if ((this.Location.X > workingScreen.Bounds.Right) ||
                (this.Location.Y > workingScreen.Bounds.Bottom) ||
                ((this.Size.Width + this.Location.X) < workingScreen.Bounds.Left) ||
                ((this.Size.Height + this.Location.Y) < workingScreen.Bounds.Top))
            {
                this.Location = new Point(0, 0);
            }


            // Cleanup in case of corruption:

            if (windowSettings.activePluginList == null)
            {
                windowSettings.activePluginList = new StringCollection();
            }
            else
            {
                HashSet<string> tmpHash = new HashSet<string>();
                foreach (string str in windowSettings.activePluginList)
                {
                    tmpHash.Add(str);
                }

                windowSettings.activePluginList.Clear();
                foreach (string str in tmpHash)
                    windowSettings.activePluginList.Add(str);
            }
            
            // End cleanup


            // Set default save directory
            if (appSettings.DefaultParseSaveDirectory == string.Empty)
            {
                appSettings.DefaultParseSaveDirectory = Application.CommonAppDataPath;
            }
            
            defaultSaveDirectory = appSettings.DefaultParseSaveDirectory;


            // Load plugins on startup and add them to the Windows menu
            FindAndLoadPlugins();
            PopulateTabsMenu();

            // Handle any command line arguments, to allow us to open files
            // directed at us.
            string[] cla = Environment.GetCommandLineArgs();
            if (cla.Length > 1)
                OpenFile(cla[1]);
        }

        private void ParserWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                windowSettings.mainWindowMaximized = true;
                windowSettings.mainWindowPosition = this.RestoreBounds.Location;
                windowSettings.mainWindowSize = this.RestoreBounds.Size;
            }
            else
            {
                windowSettings.mainWindowMaximized = false;
                windowSettings.mainWindowPosition = this.Location;
                windowSettings.mainWindowSize = this.Size;
            }

            windowSettings.Save();
            appSettings.Save();
        }
        #endregion

        #region Menu Popup Handlers
        private void fileMenu_Popup(object sender, EventArgs e)
        {
            bool monitorRunning = Monitor.Instance.IsRunning;
            bool databaseOpen = DatabaseManager.Instance.IsDatabaseOpen;

            // Can't start a parse if one is running.
            beginDefaultParseMenuItem.Enabled = !monitorRunning;
            beginParseAndSaveDataMenuItem.Enabled = !monitorRunning;
            openSavedDataMenuItem.Enabled = !monitorRunning;

            // Can only stop a parse if one is running.
            quitParsingMenuItem.Enabled = monitorRunning;

            // Can only continue or save if none running, and database is opened
            continueParsingMenuItem.Enabled = (!monitorRunning) && (databaseOpen);
            saveCurrentDataAsMenuItem.Enabled = (!monitorRunning) && (databaseOpen);
            saveReportMenuItem.Enabled = (!monitorRunning) && (databaseOpen);

            // Can't import if a parse is running.
            importToolStripMenuItem.Enabled = !monitorRunning;
        }

        private void toolsMenu_Popup(object sender, EventArgs e)
        {
            toolsReparseMenuItem.Enabled = (Monitor.Instance.IsRunning == false);

#if DEBUG
            toolsTestFunctionMenuItem.Visible = true;
#else
            toolsTestFunctionMenuItem.Visible = false;
#endif
        }

        private void windowsMenu_Popup(object sender, EventArgs e)
        {
            appSettings.Reload();

            bool inDebugMode = appSettings.DebugMode;

#if DEBUG
            inDebugMode = true;
#endif

            // If any tabs open, enable menu item to close all tabs
            closeAllTabsToolStripMenuItem.Enabled = pluginList.Any(p => p.IsActive == true);


            if (inDebugMode == true)
            {
                for (int i = 2; i < windowsMenu.DropDownItems.Count; i++)
                {
                    windowsMenu.DropDownItems[i].Enabled = true;
                }
            }
            else
            {
                IPlugin plugin;
                for (int i = 2; i < windowsMenu.DropDownItems.Count; i++)
                {
                    plugin = pluginList[i - 2];
                    if (plugin.IsDebug == true)
                    {
                        ToolStripMenuItem tsmi = windowsMenu.DropDownItems[i] as ToolStripMenuItem;
                        if (tsmi != null)
                        {
                            tsmi.Checked = false;
                        }

                        windowsMenu.DropDownItems[i].Enabled = false;
                    }
                    else
                    {
                        windowsMenu.DropDownItems[i].Enabled = true;
                    }
                }
            }
        }

        #endregion

        #region Menu Action Handlers
        /// <summary>
        /// Get the requested filename to save the database as and start
        /// parsing to that output file.
        /// </summary>
        private void menuBeginParseWithSave_Click(object sender, EventArgs e)
        {
            string outFilename;
            if (GetParseFileName(out outFilename) == true)
                StartParsing(outFilename);
        }

        /// <summary>
        /// Initiate parsing with no output file provided.
        /// </summary>
        private void menuBeginDefaultParse_Click(object sender, EventArgs e)
        {
            StartParsing("");
        }

        /// <summary>
        /// Stop any active parsing.
        /// </summary>
        private void menuStopParse_Click(object sender, EventArgs e)
        {
            StopParsing();
        }

        private void menuContinueParse_Click(object sender, EventArgs e)
        {
            // Let the database notify us of changes, and we'll notify the active plugins.
            DatabaseManager.Instance.DatabaseChanging += MonitorDatabaseChanging;
            DatabaseManager.Instance.DatabaseChanged += MonitorDatabaseChanged;

            Monitor.Instance.Continue(appSettings.ParseMode);

            toolStripStatusLabel.Text = string.Format("Continuing parse: {0}.",
                (new FileInfo(DatabaseManager.Instance.DatabaseFilename)).Name);
        }

        private void menuOpenSavedData_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = defaultSaveDirectory;
            ofd.Multiselect = false;
            ofd.DefaultExt = "sdf";
            ofd.Title = "Select file to parse...";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                StopParsing();

                OpenFile(ofd.FileName);
            }
        }

        private void menuSaveDataAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = defaultSaveDirectory;
            sfd.DefaultExt = "sdf";
            sfd.Title = "Select file to save parse data to...";
            sfd.Filter = "Complete copy (*.sdf)|*.sdf|Copy without Chat Logs (*.sdf)|*.sdf||";
            sfd.FilterIndex = 1;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (sfd.FileName == DatabaseManager.Instance.DatabaseFilename)
                {
                    MessageBox.Show("Can't save the database file back onto itself.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    Cursor.Current = Cursors.WaitCursor;

                    switch (sfd.FilterIndex)
                    {
                        case 1:
                            File.Copy(DatabaseManager.Instance.DatabaseFilename, sfd.FileName, true);
                            OpenFile(sfd.FileName);
                            break;
                        case 2:
                            File.Copy(DatabaseManager.Instance.DatabaseFilename, sfd.FileName, true);
                            OpenFile(sfd.FileName);
                            DatabaseManager.Instance.PurgeChatInfo();
                            OpenFile(sfd.FileName);
                            break;
                        default:
                            string errmsg = string.Format("Unknown save format (index {0})", sfd.FilterIndex);
                            throw new InvalidOperationException(errmsg);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Logger.Instance.Log(ex);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void menuSaveReport_Click(object sender, EventArgs e)
        {
            // Save report generated by the current plugin
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = defaultSaveDirectory;
            sfd.DefaultExt = "sdf";
            sfd.Title = "Select file to save parse data to...";
            //sfd.Filter = "Plain text (Current tab) (*.txt)|*.txt|Plain text (All tabs) (*.txt)|*.txt|Excel Spreadsheet (Current tab) (*.xls)|*.xls||";
            sfd.Filter = "Plain text (Current tab) (*.txt)|*.txt|Plain text (All tabs) (*.txt)|*.txt||";
            sfd.FilterIndex = 1;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    IPlugin tabPlugin;

                    switch (sfd.FilterIndex)
                    {
                        case 1:
                            // Save as raw text
                            tabPlugin = pluginTabs.SelectedTab.Controls.OfType<IPlugin>().FirstOrDefault();

                            if (tabPlugin != null)
                            {
                                string textContents = tabPlugin.TextContents;

                                using (StreamWriter sw = new StreamWriter(sfd.FileName))
                                {
                                    sw.Write(textContents);
                                }
                            }
                            break;
                        case 2:
                            // Save raw text of all tabs into one text file.
                            using (StreamWriter sw = new StreamWriter(sfd.FileName))
                            {
                                foreach (IPlugin tab in pluginTabs.TabPages.OfType<IPlugin>())
                                {
                                    sw.WriteLine(tab.TabName);
                                    sw.WriteLine();

                                    string textContents = tab.TextContents;
                                    sw.Write(textContents);

                                    sw.WriteLine();
                                    sw.WriteLine();
                                }
                            }
                            break;
                        case 3:
                            // Save generated output from the tab into an excel spreadsheet.
                            tabPlugin = pluginTabs.SelectedTab.Controls.OfType<IPlugin>().FirstOrDefault();
                            
                            if (tabPlugin != null)
                            {
                                if (tabPlugin.GeneratedDataTableForExcel != null)
                                {
                                    using (TextWriter tx = new StreamWriter(sfd.FileName))
                                    {
                                        System.Web.HttpResponse response = new System.Web.HttpResponse(tx);
                                        ExcelExport.Convert(tabPlugin.GeneratedDataTableForExcel, response);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show(string.Format("The {0} tab is not set up to export data to Excel at this time.",
                                        tabPlugin.TabName), "Cannot export", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                }
                            }
                            break;
                        default:
                            string errmsg = string.Format("Unknown save format (index {0})", sfd.FilterIndex);
                            throw new InvalidOperationException(errmsg);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Logger.Instance.Log(ex);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            this.Shutdown();
            Application.Exit();
        }

        private void menuOptions_Click(object sender, EventArgs e)
        {
            Options optionsForm = new Options(Monitor.Instance.IsRunning);
            if (optionsForm.ShowDialog(this) == DialogResult.OK)
            {
                windowSettings.Reload();

                windowsMenu_Popup(windowsMenu, null);

                // Reload possibly changed save directory.
                defaultSaveDirectory = appSettings.DefaultParseSaveDirectory;
            }
        }

        private void menuAbout_Click(object sender, EventArgs e)
        {
            AboutBox aboutForm = new AboutBox();
            aboutForm.ShowDialog();
        }

        private void copyTabInfoAsTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IPlugin tabPlugin = pluginTabs.SelectedTab.Controls.OfType<IPlugin>().FirstOrDefault();

            if (tabPlugin != null)
            {
                string tabContents = tabPlugin.TextContents;
                if (tabContents != string.Empty)
                    Clipboard.SetText(tabContents);
            }
        }

        private void copyTabInfoAsHTMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IPlugin tabPlugin = pluginTabs.SelectedTab.Controls.OfType<IPlugin>().FirstOrDefault();

            Utility.RTFConverter rtfConverter = new WaywardGamers.KParser.Utility.RTFConverter();

            try
            {
                if (tabPlugin != null)
                {
                    string tabContentsAsRTF = tabPlugin.TextContentsAsRTF;
                    string tabContentsAsHTML = rtfConverter.ConvertRTFToHTML(tabContentsAsRTF);
                    if (tabContentsAsHTML != string.Empty)
                        Clipboard.SetText(tabContentsAsHTML);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
        }

        private void copyTabInfoAsBBCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IPlugin tabPlugin = pluginTabs.SelectedTab.Controls.OfType<IPlugin>().FirstOrDefault();

            Utility.RTFConverter rtfConverter = new WaywardGamers.KParser.Utility.RTFConverter();

            try
            {
                if (tabPlugin != null)
                {
                    string tabContentsAsRTF = tabPlugin.TextContentsAsRTF;
                    string tabContentsAsBBCode = rtfConverter.ConvertRTFToBBCode(tabContentsAsRTF);
                    if (tabContentsAsBBCode != string.Empty)
                        Clipboard.SetText(tabContentsAsBBCode);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
        }

        private void copyTabInfoAsRTFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IPlugin tabPlugin = pluginTabs.SelectedTab.Controls.OfType<IPlugin>().FirstOrDefault();

            if (tabPlugin != null)
            {
                string tabContents = tabPlugin.TextContentsAsRTF;
                //Clipboard.SetText(tabContents, TextDataFormat.Rtf);
                if (tabContents != string.Empty)
                    Clipboard.SetText(tabContents);
            }
        }

        private void playerInformationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool databaseOpen = DatabaseManager.Instance.IsDatabaseOpen;

            if (databaseOpen == false)
            {
                MessageBox.Show("You must open or start a parse first.", "No parse file.",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            AddMonitorChanging();
            PlayerInfo infoForm = new PlayerInfo(this);
            infoForm.Show(this);
        }

        private void closeAllTabsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < windowsMenu.DropDownItems.Count; i++)
                {
                    ToolStripMenuItem windowsTSMI = windowsMenu.DropDownItems[i] as ToolStripMenuItem;

                    if (windowsTSMI != null)
                    {
                        if (windowsTSMI.Tag != null)
                        {
                            if (windowsTSMI.Tag.ToString() == "tab")
                            {
                                windowsTSMI.Checked = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
        }

        #endregion

        #region Reparse/Import functions
        /// <summary>
        /// Initiate reparsing an existing KParse database file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void reparseDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReparseDatabase(false);
        }

        private void upgradeDatabaseTimestampToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReparseDatabase(true);
        }

        private void ReparseDatabase(bool withTimestampUpgrade)
        {
            if (Monitor.Instance.IsRunning == true)
            {
                MessageBox.Show("Cannot reparse while another parse is running.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string inFilename = string.Empty;
            string outFilename = string.Empty;
            revertToThisDatabaseFile = string.Empty;

            if (DatabaseManager.Instance.IsDatabaseOpen)
            {
                DialogResult reparse = MessageBox.Show("Do you want to reparse the current data?", "Reparse current data?",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

                if (reparse == DialogResult.Cancel)
                    return;

                if (reparse == DialogResult.Yes)
                {
                    inFilename = DatabaseManager.Instance.DatabaseFilename;
                }

                revertToThisDatabaseFile = DatabaseManager.Instance.DatabaseFilename;
            }

            if (inFilename == string.Empty)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = defaultSaveDirectory;
                ofd.Multiselect = false;
                ofd.DefaultExt = "sdf";
                ofd.Title = "Select file to reparse...";

                if (ofd.ShowDialog() == DialogResult.Cancel)
                {
                    return;
                }
                else
                {
                    inFilename = ofd.FileName;
                }
            }

            if (GetParseFileName(out outFilename) == true)
            {
                if (outFilename == inFilename)
                {
                    MessageBox.Show("Cannot save to the same file you want to reparse.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;
                ReparseMode = ImportMode.Reparse;

                try
                {
                    AttachReparseStatus("Reparse: ", "Parsing");

                    try
                    {
                        if (withTimestampUpgrade)
                            Monitor.Instance.UpgradeTimestampImport(inFilename, outFilename, ImportSourceType.KParser);
                        else
                            Monitor.Instance.Import(inFilename, outFilename, ImportSourceType.KParser);
                    }
                    catch (Exception ex)
                    {
                        StopParsing();
                        DetachReparseStatus("Error trying to reparse.");
                        Logger.Instance.Log(ex);
                        MessageBox.Show(ex.Message, "Error while attempting to reparse.",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);

                        return;
                    }

                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex);
                }
            }
        }

        /// <summary>
        /// Initiate importing a DirectParse/DVSParse database file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Monitor.Instance.IsRunning == true)
            {
                MessageBox.Show("Cannot import while another parse is running.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ImportType importTypeForm = new ImportType();
            if (importTypeForm.ShowDialog(this) == DialogResult.Cancel)
                return;

            ImportSourceType importSource = importTypeForm.ImportSource;

            string inFilename = string.Empty;
            string outFilename = string.Empty;

            if (inFilename == string.Empty)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = defaultSaveDirectory;
                ofd.Multiselect = false;
                ofd.Filter = "Direct Parse Files (*.dpd)|*.dpd|DVS/Direct Parse Files (*.dvsd)|*.dvsd|Database Files (*.sdf)|*.sdf|All Files (*.*)|*.*";
                ofd.FilterIndex = 0;
                ofd.Title = "Select file to import...";

                if (ofd.ShowDialog() == DialogResult.Cancel)
                {
                    return;
                }
                else
                {
                    inFilename = ofd.FileName;
                }
            }

            outFilename = Path.Combine(defaultSaveDirectory, Path.GetFileNameWithoutExtension(inFilename));
            outFilename = Path.ChangeExtension(outFilename, "sdf");

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = defaultSaveDirectory;
            sfd.Filter = "Database Files (*.sdf)|*.sdf|All Files (*.*)|*.*";
            sfd.FilterIndex = 0;
            sfd.DefaultExt = "sdf";
            sfd.FileName = outFilename;
            sfd.Title = "Select database file to save parse to...";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                outFilename = sfd.FileName;

                if (outFilename == inFilename)
                {
                    MessageBox.Show("Cannot save to the same file you want to import.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;
                ReparseMode = ImportMode.Import;

                try
                {
                    try
                    {
                        AttachReparseStatus("Import: ", "Parsing");

                        Monitor.Instance.Import(inFilename, outFilename, importSource);
                    }
                    catch (Exception ex)
                    {
                        StopParsing();
                        DetachReparseStatus("Error trying to import.");
                        Logger.Instance.Log(ex);
                        MessageBox.Show(ex.Message, "Error while attempting to import.",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);

                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex);
                }
            }
        }

        /// <summary>
        /// Function that gets attached as an event listener for reparsing/importing
        /// database info.
        /// </summary>
        /// <param name="sender">Who sent the update.</param>
        /// <param name="e">DatabaseReparseEventArgs</param>
        private void MonitorReparse(object sender, ReaderStatusEventArgs e)
        {
            if (this.InvokeRequired)
            {
                Action<object, ReaderStatusEventArgs> thisFunc = MonitorReparse;
                BeginInvoke(thisFunc, new object[] { sender, e });
                return;
            }
            try
            {

                if (sender is Monitoring.DatabaseReader)
                {
                    if (e.Completed == true)
                    {
                        reparseComplete = true;
                    }
                    else if (e.Failed == true)
                    {
                        // ie: reparse was cancelled or errored out
                        // stop monitoring

                        DetachReparseStatus("Aborted.");

                        // reload original database file, or the file we were reparsing if no original
                        if (revertToThisDatabaseFile != string.Empty)
                            OpenFile(revertToThisDatabaseFile);
                        else
                            OpenFile(KParserReadingManager.Instance.DatabaseFilename);
                    }
                    else
                    {
                        string msg = string.Format("Parsing {0}/{1}  ", e.ProcessedItems, e.TotalItems);
                        UpdateProgressMonitor(msg, e.ProcessedItems, e.TotalItems);
                    }
                }
                else if (sender is DatabaseManager)
                {
                    if (e.Completed == true)
                    {
                        if (reparseComplete == true)
                        {
                            string completedText;

                            if (ReparseMode == ImportMode.Reparse)
                                completedText = "Reparse complete.";
                            else
                                completedText = "Import complete.";

                            DetachReparseStatus(completedText);

                            OpenFile(DatabaseManager.Instance.DatabaseFilename);
                        }
                    }
                    else if (e.Failed == true)
                    {
                        DetachReparseStatus("Aborted.");

                        // reload original database file, or the file we were reparsing if no original
                        if (revertToThisDatabaseFile != string.Empty)
                            OpenFile(revertToThisDatabaseFile);
                        else
                            OpenFile(KParserReadingManager.Instance.DatabaseFilename);
                    }
                    else
                    {
                        UpdateProgressMonitor(string.Format("Saving {0}/{1}  ", e.ProcessedItems, e.TotalItems),
                            e.ProcessedItems, e.TotalItems);

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
                throw;
            }
        }

        /// <summary>
        /// Update the label and progress meter during a reparse/import operation.
        /// </summary>
        /// <param name="currentState">The string to update the extra label to, indicating the current state.</param>
        /// <param name="progress">The current progress value.</param>
        /// <param name="maxProgress">The maximum progress value.</param>
        private void UpdateProgressMonitor(string currentState, int progress, int maxProgress)
        {
            if (this.InvokeRequired)
            {
                Action<string, int, int> thisFunc = UpdateProgressMonitor;
                Invoke(thisFunc, new object[] { currentState, progress, maxProgress });
                return;
            }

            if (maxProgress < 1)
                throw new ArgumentOutOfRangeException("maxProgress", maxProgress,
                    "Maximum progress must be a positive value.");

            if (progress > maxProgress)
                throw new ArgumentOutOfRangeException("progress", progress,
                    string.Format("Progress value {0} is greater than maximum allowed ({1}).", progress, maxProgress));

            reparseState.Text = currentState;

            reparseProgress.Maximum = maxProgress;
            reparseProgress.Value = progress;
        }

        /// <summary>
        /// Add an extra label plus a progress meter to the status bar to track
        /// progress during a reparse.  Attach event listeners to appropriate classes.
        /// </summary>
        /// <param name="statusLabel">The string to set the default statusbar text to.</param>
        /// <param name="initialState">The string to set the extra statusbar text to.</param>
        private void AttachReparseStatus(string statusLabel, string initialState)
        {
            Monitoring.Monitor.Instance.ReaderStatusChanged += MonitorReparse;
            DatabaseManager.Instance.ReparseProgressChanged += MonitorReparse;

            reparseComplete = false;

            toolStripStatusLabel.Text = statusLabel;

            reparseState.Text = initialState;
            reparseProgress.Minimum = 0;
            reparseProgress.Value = 0;

            reparseState.Name = "Reparse State";
            reparseProgress.Name = "Reparse Progress";

            statusStrip.Items.Add(reparseState);
            statusStrip.Items.Add(reparseProgress);
        }

        /// <summary>
        /// Remove the extra label and progress meter from the status bar after
        /// a reparse is complete or aborted.  Remove event listeners from appropriate classes.
        /// </summary>
        /// <param name="statusLabel">The string to set the default statusbar text to after completion.</param>
        private void DetachReparseStatus(string statusLabel)
        {
            Monitoring.Monitor.Instance.ReaderStatusChanged -= MonitorReparse;
            DatabaseManager.Instance.ReparseProgressChanged -= MonitorReparse;

            Monitoring.Monitor.Instance.Stop();

            statusStrip.Items.Remove(reparseState);
            statusStrip.Items.Remove(reparseProgress);

            toolStripStatusLabel.Text = statusLabel;
        }
        #endregion

        #region Menu Support Functions
        /// <summary>
        /// Gets the filename to save the parse output to.  By default it uses
        /// the current date and a numeric progression.
        /// </summary>
        /// <param name="fileName">The name of the file to save the parse to.</param>
        /// <returns>True if the user ok'd the filename, false if it was cancelled.</returns>
        private bool GetParseFileName(out string fileName)
        {
            string baseDateName = string.Format("{0:D2}-{1:D2}-{2:D2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            string dateNameFilter = baseDateName + "_???.sdf";

            string[] files = Directory.GetFiles(defaultSaveDirectory, dateNameFilter);

            int index = 1;

            try
            {
                if (files.Length > 0)
                {
                    Array.Sort(files);

                    string lastFullFileName = files[files.Length - 1];

                    FileInfo fi = new FileInfo(lastFullFileName);

                    string lastFileName = fi.Name;

                    Regex rx = new Regex(@"\d{2}-\d{2}-\d{2}_(\d{3}).sdf");

                    Match match = rx.Match(lastFileName);

                    if (match.Success == true)
                    {
                        if (Int32.TryParse(match.Groups[1].Value, out index) == false)
                        {
                            index = files.Length;
                        }

                        index++;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format(ex.Message + "\nUsing date index 1.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error));
            }

            string dateName = Path.Combine(defaultSaveDirectory, string.Format("{0}_{1:D3}.sdf", baseDateName, index));

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = defaultSaveDirectory;
            sfd.DefaultExt = "sdf";
            sfd.FileName = dateName;
            sfd.Title = "Select database file to save parse to...";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                fileName = sfd.FileName;
                return true;
            }

            fileName = "";
            return false;
        }

        private void OpenFile(string fileName)
        {
            if (File.Exists(fileName) == false)
                return;

            try
            {
                DatabaseManager.Instance.OpenDatabase(fileName);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
                MessageBox.Show("Unable to open database.  You may need to reparse or upgrade the database file.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                MobXPHandler.Instance.Reset();

                NotifyPlugins(true);

                toolStripStatusLabel.Text = string.Format("Current open parse: {0}.",
                    (new FileInfo(fileName)).Name);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        #endregion

        #region Plugin Tab/Window Management
        /// <summary>
        /// Search all DLLs in the application directory for classes derived from the
        /// abstract plugin class.  If one exists, create an instance of that class
        /// and add it to the list of available plugins.
        /// </summary>
        private void FindAndLoadPlugins()
        {
            // Get the DLLs in the application directory
            string dllFilter = "*.dll";
            string[] files = Directory.GetFiles(applicationDirectory, dllFilter);

            Assembly a;
            Type pluginInterfaceType = typeof(WaywardGamers.KParser.Plugin.IPlugin);
            Type userControlType = typeof(UserControl);

            foreach (string file in files)
            {
                try
                {
                    a = Assembly.LoadFrom(file);
                }
                catch (BadImageFormatException)
                {
                    continue;
                }

                // Don't look in the core for plugins [change this to plugin base dll later]
                if (a.ManifestModule.Name != "WaywardGamers.KParser.ParserCore.dll")
                {
                    Type[] exportedTypes = a.GetExportedTypes();

                    // Check the types in each one
                    foreach (Type t in exportedTypes)
                    {
                        // If they're of type PluginBase, and aren't the abstract parent type,
                        // add them to our list of valid plugins.
                        if ((t.IsPublic == true) &&
                            (t.IsSubclassOf(userControlType) == true) &&
                            (pluginInterfaceType.IsAssignableFrom(t) == true))
                        {
                            pluginList.Add((IPlugin)Activator.CreateInstance(t));
                        }
                    }
                }
            }

            foreach (var plug in pluginList)
            {
                if (windowSettings.activePluginList.Contains(plug.TabName) == true)
                {
                    plug.IsActive = true;
                }
            }
        }

        /// <summary>
        /// Called on startup, this adds the names of the plugins to the Window
        /// menu so that the user can enable/disable individual plugins.
        /// </summary>
        private void PopulateTabsMenu()
        {
            // This is only run once.

            // Add a separator under the About menu item if we have
            // any plugins available.
            if (pluginList.Count > 0)
            {
                windowsMenu.DropDownItems.Add(windowsToolStripSeparator);
            }
            else
            {
                return;
            }

            pluginList = pluginList.OrderBy(p => p.TabName).ToList();

            // Create a menu item and tab page for each plugin, with synced indexes.
            for (int i = 0; i < pluginList.Count; i++)
            {
                ToolStripMenuItem tsmi = new ToolStripMenuItem(pluginList[i].TabName);
                tsmi.Name = pluginList[i].TabName;
                tsmi.CheckOnClick = true;
                tsmi.Tag = "tab";
                tsmi.CheckedChanged += new EventHandler(tabMenuItem_CheckedChanged);
                windowsMenu.DropDownItems.Add(tsmi);

                TabPage tp = new TabPage(pluginList[i].TabName);
                tp.Tag = i.ToString();
                tabList.Add(tp);

                BuildTab(tp, pluginList[i]);

                tsmi.Checked = pluginList[i].IsActive;
            }

            if (pluginTabs.TabCount > 0)
                pluginTabs.SelectedIndex = 0;
        }

        /// <summary>
        /// Configure the tab the will contain the specified plugin control.
        /// </summary>
        /// <param name="tp">The tab that gets the plugin.</param>
        /// <param name="iPlugin">The plugin that goes in the tab.</param>
        private void BuildTab(TabPage tp, IPlugin iPlugin)
        {
            iPlugin.Reset();
            UserControl control = iPlugin.Control;

            control.Anchor = (AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top);
            tp.Controls.Add(control);
            control.Location = new System.Drawing.Point(2, 2);
            control.Size = new Size(tp.Width - 4, tp.Height - 4);
        }

        /// <summary>
        /// When a plugin is checked/unchecked from the Window menu, add or
        /// remove it from active plugin list, then update the visible tabs.
        /// </summary>
        void tabMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;

            if (tsmi == null)
                return;

            TabPage tabFromMenu = tabList.FirstOrDefault(t => t.Text == tsmi.Text);

            if (tabFromMenu != null)
            {
                if (tsmi.Checked == false)
                {
                    var plugin = pluginList.FirstOrDefault(p => p.TabName == tsmi.Text);
                    if (plugin != null)
                    {
                        plugin.IsActive = false;
                        windowSettings.activePluginList.Remove(plugin.TabName);
                    }

                    if (tabFromMenu != null)
                        pluginTabs.TabPages.Remove(tabFromMenu);
                }
                else
                {
                    var plugin = pluginList.FirstOrDefault(p => p.TabName == tsmi.Text);

                    if (plugin != null)
                    {
                        pluginTabs.TabPages.Add(tabFromMenu);
                        plugin.IsActive = true;

                        if (!windowSettings.activePluginList.Contains(plugin.TabName))
                            windowSettings.activePluginList.Add(plugin.TabName);

                        if (DatabaseManager.Instance.IsDatabaseOpen)
                        {
                            try
                            {
                                Cursor.Current = Cursors.WaitCursor;

                                plugin.NotifyOfUpdate();
                            }
                            finally
                            {
                                Cursor.Current = Cursors.Default;
                            }
                        }

                        pluginTabs.SelectedTab = tabFromMenu;
                    }
                }
            }
        }

        /// <summary>
        /// Close the tab currently under the cursor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentTab != null)
                {
                    int index = windowsMenu.DropDownItems.IndexOfKey(currentTab.Text);

                    if (index > 1)
                    {
                        ToolStripMenuItem windowsTSMI = windowsMenu.DropDownItems[index] as ToolStripMenuItem;
                        if (windowsTSMI != null)
                            windowsTSMI.Checked = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
        }

        /// <summary>
        /// Close all tabs other than the one currently under the cursor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeOtherTabsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentTab != null)
                {
                    int currentTabIndex = windowsMenu.DropDownItems.IndexOfKey(currentTab.Text);

                    for (int i = 2; i < windowsMenu.DropDownItems.Count; i++)
                    {
                        ToolStripMenuItem windowsTSMI = windowsMenu.DropDownItems[i] as ToolStripMenuItem;

                        if (windowsTSMI != null)
                        {
                            if ((i != currentTabIndex) && (windowsTSMI.Tag.ToString() == "tab"))
                            {
                                windowsTSMI.Checked = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
        }

        /// <summary>
        /// Determine which tab is currently under the cursor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pluginTabs_MouseMove(object sender, MouseEventArgs e)
        {
            if (pluginTabs == sender)
            {
                // tab height
                int height = pluginTabs.ItemSize.Height;

                if (e.Y > height)
                    return;

                currentTab = null;

                for (int index = 0; index < pluginTabs.TabCount; index++)
                {
                    if (pluginTabs.GetTabRect(index).Contains(e.X, e.Y))
                    {
                        currentTab = pluginTabs.TabPages[index];
                        break;
                    }
                }
            }
        }
        #endregion

        #region Parsing Control Methods
        private void StartParsing(string outputFileName)
        {
            appSettings.Reload();

            // Let the database notify us of changes, and we'll notify the active plugins.
            DatabaseManager.Instance.DatabaseChanging += MonitorDatabaseChanging;
            DatabaseManager.Instance.DatabaseChanged += MonitorDatabaseChanged;

            bool reopeningFile = (outputFileName != string.Empty) && File.Exists(outputFileName);

            // Reset all plugins
            foreach (IPlugin plugin in pluginList)
            {
                if (plugin.IsActive)
                    plugin.Reset();
            }

            // Reset the xp handler
            MobXPHandler.Instance.Reset();

            try
            {
                Monitor.Instance.Start(appSettings.ParseMode, outputFileName);

                if (reopeningFile == true)
                {
                    NotifyPlugins(false);
                }

                if ((outputFileName == null) || (outputFileName == string.Empty))
                    toolStripStatusLabel.Text = "Parsing to default file.";
                else
                    toolStripStatusLabel.Text = string.Format("Parsing to {0}.", (new FileInfo(outputFileName)).Name);
            }
            catch (Exception e)
            {
                StopParsing();
                toolStripStatusLabel.Text = "Error.  Parsing stopped.";
                Logger.Instance.Log(e);
                MessageBox.Show(e.Message, "Error while attempting to initiate parse.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StopParsing()
        {
            Cursor.Current = Cursors.Default;

            if (Monitor.Instance.IsRunning == true)
            {
                Monitor.Instance.Stop();
                toolStripStatusLabel.Text = "Status: Stopped.";

                DatabaseManager.Instance.DatabaseChanging -= MonitorDatabaseChanging;
                DatabaseManager.Instance.DatabaseChanged -= MonitorDatabaseChanged;

                if (Monitor.Instance.ParseMode == DataSource.Database)
                {
                    DetachReparseStatus("Reparse cancelled");
                }

                NotifyPlugins(false);
            }
        }

        /// <summary>
        /// Notify all plugins to update their data.  This is called when opening,
        /// reopening or ending a parse.
        /// </summary>
        /// <param name="profile">Indicate whether to do profile timing per plugin.</param>
        private void NotifyPlugins(bool profile)
        {
            try
            {
                if (profile == true)
                {
                    foreach (IPlugin plugin in pluginList)
                    {
                        if (plugin.IsActive)
                        {
                            using (new RegionProfiler("Opening " + plugin.TabName))
                            {
                                plugin.NotifyOfUpdate();
                            }
                        }
                    }
                }
                else
                {
                    foreach (IPlugin plugin in pluginList)
                    {
                        if (plugin.IsActive)
                            plugin.NotifyOfUpdate();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e);
                MessageBox.Show(e.Message, "Error while attempting to stop parse.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Shutdown()
        {
            Monitor.Instance.Stop();
            toolStripStatusLabel.Text = "Status: Stopped.";
        }

        private void MonitorDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            foreach (IPlugin plugin in pluginList)
            {
                if (plugin.IsActive)
                    plugin.WatchDatabaseChanging(sender, e);
            }
        }

        private void MonitorDatabaseChanged(object sender, DatabaseWatchEventArgs e)
        {
            foreach (IPlugin plugin in pluginList)
            {
                if (plugin.IsActive)
                    plugin.WatchDatabaseChanged(sender, e);
            }
        }

        private void AddMonitorChanging()
        {
            DatabaseManager.Instance.DatabaseChanging += MonitorDatabaseChanging;
        }

        public void RemoveMonitorChanging()
        {
            DatabaseManager.Instance.DatabaseChanging -= MonitorDatabaseChanging;
        }
        #endregion

        #region Code for testing stuff
        private void menuTestItem_Click(object sender, EventArgs e)
        {
            //Application.CommonAppDataPath;
            //Application.UserAppDataPath;

            //TestParsingOfChatLine();

            //ScanMemoryForUpdatedMemloc();

            //TestArrayCopySpeed();
        }

        private static void TestParsingOfChatLine()
        {
            //MMHook.Hook("92,02,00,80808080,00002ccc,00003447,002c,00,01,00,00,’Time remaining: 10 minutes (Earth time).1");
            MMHook.Hook("98,02,01,80808080,00000207,00000216,0039,00,01,01,00,・23:55:12] Rediroq : This is entrance. One way only.");
        }

        private static void ScanMemoryForUpdatedMemloc()
        {
            Monitor.Instance.ScanRAM();
        }

        private static void TestArrayCopySpeed()
        {

            int arraySize = 5000000;
            byte[] baseArray = new byte[arraySize];
            byte[] copyToArray = new byte[arraySize];

            // fill starting array
            for (int i = 0; i < arraySize; i++)
            {
                baseArray[i] = (byte)(i % 256);
            }

            using (new RegionProfiler("array copy x100"))
            {
                for (int i = 0; i < 100; i++)
                {
                    for (int j = 0; j < arraySize; j++)
                    {
                        copyToArray[i] = baseArray[i];
                    }
                }
            }


            using (new RegionProfiler("blockcopy x100"))
            {
                for (int i = 0; i < 100; i++)
                {
                    Buffer.BlockCopy(baseArray, 0, copyToArray, 0, arraySize);
                }
            }
        }
        #endregion


    }
}