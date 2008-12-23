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
using WaywardGamers.KParser.Forms;

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
        private List<IPlugin> activePluginList = new List<IPlugin>();
        private List<TabPage> tabList = new List<TabPage>();

        TabPage currentTab = null;

        private ReparseMode ReparseMode;
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
            this.Size = windowSettings.mainWindowSize;
            this.Location = windowSettings.mainWindowPosition;
            if (windowSettings.mainWindowMaximized == true)
                this.WindowState = FormWindowState.Maximized;
            else
                this.WindowState = FormWindowState.Normal;

            if (windowSettings.activePluginList == null)
                windowSettings.activePluginList = new StringCollection();

            // Cleanup in case of corruption:
            StringCollection tmpStrings = new StringCollection();
            foreach (string str in windowSettings.activePluginList)
            {
                if (tmpStrings.Contains(str) == false)
                    tmpStrings.Add(str);
            }

            windowSettings.activePluginList.Clear();
            foreach (string str in tmpStrings)
                windowSettings.activePluginList.Add(str);
            // End cleanup

            if (windowSettings.fullPluginList == null)
                windowSettings.fullPluginList = new StringCollection();

            // Set default save directory
            defaultSaveDirectory = appSettings.DefaultParseSaveDirectory;
            if (defaultSaveDirectory == string.Empty)
            {
                defaultSaveDirectory = Application.CommonAppDataPath;
                appSettings.DefaultParseSaveDirectory = defaultSaveDirectory;
                appSettings.Save();
            }


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
        }
        #endregion

        #region Menu Popup Handlers
        private void fileMenu_Popup(object sender, EventArgs e)
        {
            bool monitorRunning = Monitor.IsRunning;
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
            toolsReparseMenuItem.Enabled = (Monitor.IsRunning == false);

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
            
            Monitor.Continue();

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
                            File.Copy(DatabaseManager.Instance.DatabaseFilename, sfd.FileName);
                            OpenFile(sfd.FileName);
                            break;
                        case 2:
                            File.Copy(DatabaseManager.Instance.DatabaseFilename, sfd.FileName);
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
            Options optionsForm = new Options(Monitor.IsRunning);
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

        #endregion

        #region Reparse/Import functions
        /// <summary>
        /// Initiate reparsing an existing KParse database file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void databaseReparse_Click(object sender, EventArgs e)
        {
            if (Monitor.IsRunning == true)
            {
                MessageBox.Show("Cannot reparse while another parse is running.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string inFilename = string.Empty;
            string outFilename = string.Empty;

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
                ReparseMode = ReparseMode.Reparse;

                try
                {
                    AttachReparseStatus("Reparse: ", "Parsing");

                    try
                    {
                        Monitor.Reparse(inFilename, outFilename);
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
        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Monitor.IsRunning == true)
            {
                MessageBox.Show("Cannot import while another parse is running.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ImportType importTypeForm = new ImportType();
            if (importTypeForm.ShowDialog(this) == DialogResult.Cancel)
                return;

            ImportSource importSource = importTypeForm.ImportSource;

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
                ReparseMode = ReparseMode.Import;

                try
                {
                    try
                    {
                        AttachReparseStatus("Import: ", "Parsing");

                        Monitor.Import(inFilename, outFilename, importSource);
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
        private void MonitorReparse(object sender, DatabaseReparseEventArgs e)
        {
            if (this.InvokeRequired)
            {
                Action<object, DatabaseReparseEventArgs> thisFunc = MonitorReparse;
                Invoke(thisFunc, new object[] { sender, e });
                return;
            }

            if (e.Complete == true)
            {
                string completedText;

                if (ReparseMode == ReparseMode.Reparse)
                    completedText = "Reparse complete.";
                else
                    completedText = "Import complete.";

                DetachReparseStatus(completedText);

                OpenFile(DatabaseManager.Instance.DatabaseFilename);
            }
            else
            {
                Monitoring.DatabaseReader dr = sender as Monitoring.DatabaseReader;

                // If reader sent message, we're in parsing mode
                if (dr != null)
                {
                    if (Monitoring.DatabaseReader.Instance.IsRunning == true)
                    {
                        UpdateProgressMonitor("Parsing -", e.RowsRead, e.TotalRows);
                    }
                    else
                    {
                        // ie: reparse was cancelled or errored out
                        // stop monitoring
                        Cursor.Current = Cursors.Default;

                        DetachReparseStatus("Aborted.");

                        // reload original database file
                        OpenFile(DatabaseReadingManager.Instance.DatabaseFilename);
                    }

                    return;
                }

                DatabaseManager dm = sender as DatabaseManager;

                // If db manager sent message, we're in saving mode
                if (dm != null)
                {
                    UpdateProgressMonitor("Saving -", e.RowsRead, e.TotalRows);

                    return;
                }

                UpdateProgressMonitor("Unknown -", e.RowsRead, e.TotalRows);
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


            ToolStripItem[] itemSearch;

            itemSearch = statusStrip.Items.Find("Reparse State", false);

            if (itemSearch.Length == 1)
            {
                ToolStripStatusLabel reparseState = itemSearch[0] as ToolStripStatusLabel;
                if (reparseState != null)
                {
                    reparseState.Text = currentState;
                }
            }

            itemSearch = statusStrip.Items.Find("Reparse Progress", false);

            if (itemSearch.Length == 1)
            {
                ToolStripProgressBar reparseProgress = itemSearch[0] as ToolStripProgressBar;
                if (reparseProgress != null)
                {
                    reparseProgress.Maximum = maxProgress;
                    reparseProgress.Value = progress;
                }
            }
        }

        /// <summary>
        /// Add an extra label plus a progress meter to the status bar to track
        /// progress during a reparse.  Attach event listeners to appropriate classes.
        /// </summary>
        /// <param name="statusLabel">The string to set the default statusbar text to.</param>
        /// <param name="initialState">The string to set the extra statusbar text to.</param>
        private void AttachReparseStatus(string statusLabel, string initialState)
        {
            Monitoring.DatabaseReader.Instance.ReparseProgressChanged += MonitorReparse;
            DatabaseManager.Instance.ReparseProgressChanged += MonitorReparse;

            toolStripStatusLabel.Text = statusLabel;

            ToolStripStatusLabel reparseState = new ToolStripStatusLabel(initialState);
            ToolStripProgressBar reparseProgress = new ToolStripProgressBar();
            reparseProgress.Minimum = 0;

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
            Monitoring.DatabaseReader.Instance.ReparseProgressChanged -= MonitorReparse;
            DatabaseManager.Instance.ReparseProgressChanged -= MonitorReparse;

            ToolStripItem[] itemSearch;

            itemSearch = statusStrip.Items.Find("Reparse Progress", false);

            if (itemSearch.Length == 1)
                statusStrip.Items.Remove(itemSearch[0]);

            itemSearch = statusStrip.Items.Find("Reparse State", false);

            if (itemSearch.Length == 1)
                statusStrip.Items.Remove(itemSearch[0]);

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
                a = Assembly.LoadFrom(file);

                // Don't look in the core for plugins [change this to plugin base dll later]
                if (a.ManifestModule.Name != "WaywardGamers.KParser.ParserCore.dll")
                {
                    // Check the types in each one
                    foreach (Type t in a.GetTypes())
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
                if (windowSettings.fullPluginList.Contains(plug.TabName) == false)
                {
                    windowSettings.fullPluginList.Add(plug.TabName);
                    windowSettings.activePluginList.Add(plug.TabName);
                    activePluginList.Add(plug);
                }
                else if (windowSettings.activePluginList.Contains(plug.TabName) == true)
                {
                    activePluginList.Add(plug);
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
                tsmi.CheckedChanged += new EventHandler(tabMenuItem_CheckedChanged);
                windowsMenu.DropDownItems.Add(tsmi);

                TabPage tp = new TabPage(pluginList[i].TabName);
                tp.Tag = i.ToString();
                tabList.Add(tp);

                BuildTab(tp, pluginList[i]);

                tsmi.Checked = activePluginList.Contains(pluginList[i]);
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
                    lock (activePluginList)
                    {
                        var plugin = pluginList.FirstOrDefault(p => p.TabName == tsmi.Text);
                        if (plugin != null)
                        {
                            activePluginList.Remove(plugin);
                            windowSettings.activePluginList.Remove(plugin.TabName);
                        }
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

                        lock (activePluginList)
                        {
                            if (activePluginList.Contains(plugin) == false)
                                activePluginList.Add(plugin);
                            if (!windowSettings.activePluginList.Contains(plugin.TabName))
                                windowSettings.activePluginList.Add(plugin.TabName);
                        }

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
                    int index = windowsMenu.DropDownItems.IndexOfKey(currentTab.Text);

                    for (int i = 2; i < windowsMenu.DropDownItems.Count; i++)
                    {
                        if (i != index)
                        {
                            ToolStripMenuItem windowsTSMI = windowsMenu.DropDownItems[i] as ToolStripMenuItem;
                            if (windowsTSMI != null)
                                windowsTSMI.Checked = false;
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

            if (appSettings.SpecifyPID == true)
            {
                SelectPOLProcess selectPID = new SelectPOLProcess();
                selectPID.ShowDialog();
            }

            // Let the database notify us of changes, and we'll notify the active plugins.
            DatabaseManager.Instance.DatabaseChanging += MonitorDatabaseChanging;
            DatabaseManager.Instance.DatabaseChanged += MonitorDatabaseChanged;

            bool reopeningFile = (outputFileName != string.Empty) && File.Exists(outputFileName);

            // Reset all plugins
            lock (activePluginList)
            {
                foreach (IPlugin plugin in activePluginList)
                {
                    plugin.Reset();
                }
            }

            try
            {
                Monitor.Start(outputFileName);

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

            if (Monitor.IsRunning == true)
            {
                Monitor.Stop();
                toolStripStatusLabel.Text = "Status: Stopped.";

                DatabaseManager.Instance.DatabaseChanging -= MonitorDatabaseChanging;
                DatabaseManager.Instance.DatabaseChanged -= MonitorDatabaseChanged;

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
                    lock (activePluginList)
                    {
                        foreach (IPlugin plugin in activePluginList)
                        {
                            using (new ProfileRegion("Opening " + plugin.TabName))
                            {
                                plugin.NotifyOfUpdate();
                            }
                        }
                    }
                }
                else
                {
                    lock (activePluginList)
                    {
                        foreach (IPlugin plugin in activePluginList)
                        {
                            plugin.NotifyOfUpdate();
                        }
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
            Monitor.Stop();
            toolStripStatusLabel.Text = "Status: Stopped.";
        }

        private void MonitorDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            lock (activePluginList)
            {
                foreach (IPlugin plugin in activePluginList)
                {
                    plugin.WatchDatabaseChanging(sender, e);
                }
            }
        }

        private void MonitorDatabaseChanged(object sender, DatabaseWatchEventArgs e)
        {
            lock (activePluginList)
            {
                foreach (IPlugin plugin in activePluginList)
                {
                    plugin.WatchDatabaseChanged(sender, e);
                }
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
            //MMHook.Hook("92,02,00,80808080,00002ccc,00003447,002c,00,01,00,00,’Time remaining: 10 minutes (Earth time).1");

            //Debug.WriteLine(string.Format("Company Name: {0}\n", Application.CompanyName));
            //Debug.WriteLine(string.Format("Product Name: {0}\n", Application.ProductName));
            //Debug.WriteLine(string.Format("Product Version: {0}\n", Application.ProductVersion));
            //Application.CommonAppDataPath;
            //Application.UserAppDataPath;

            //Monitor.ScanRAM();


            //using (new ProfileRegion("database copy x100"))
            //{
            //    KPDatabaseDataSet db = DatabaseManager.Instance.Database;
            //    KPDatabaseDataSet dbCopy;

            //    if (db != null)
            //    {
            //        for (int i = 0; i < 100; i++)
            //        {
            //            dbCopy = (KPDatabaseDataSet)db.Copy();
            //        }
            //    }
            //}

            int arraySize = 5000000;
            byte[] baseArray = new byte[arraySize];
            byte[] copyToArray = new byte[arraySize];

            // fill starting array
            for (int i = 0; i < arraySize; i++)
            {
                baseArray[i] = (byte)(i % 256);
            }

            using (new ProfileRegion("array copy x100"))
            {
                for (int i = 0; i < 100; i++)
                {
                    for (int j = 0; j < arraySize; j++)
                    {
                        copyToArray[i] = baseArray[i];
                    }
                }
            }


            using (new ProfileRegion("blockcopy x100"))
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