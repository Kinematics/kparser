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

        private List<IPlugin> pluginList = new List<IPlugin>();
        private List<IPlugin> activePluginList = new List<IPlugin>();
        private List<TabPage> tabList = new List<TabPage>();
        #endregion

        #region Constructor
        public ParserWindow()
        {
            InitializeComponent();

            Properties.Settings appSettings = new WaywardGamers.KParser.Properties.Settings();

            applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            defaultSaveDirectory = Application.CommonAppDataPath;
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
            if (windowSettings.fullPluginList == null)
                windowSettings.fullPluginList = new StringCollection();

            // Load plugins on startup and add them to the Windows menu
            FindAndLoadPlugins();
            PopulatePluginMenu();
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
            bool enableMenus = (DatabaseManager.Instance.Database != null) &&
                (Monitor.IsRunning == false);
            menuContinueParse.Enabled = enableMenus;
            menuSaveDataAs.Enabled = enableMenus;
        }

        private void databaseToolsMenu_Popup(object sender, EventArgs e)
        {
            databaseReparse.Enabled = (DatabaseManager.Instance.Database != null);
        }

        private void toolsMenu_Popup(object sender, EventArgs e)
        {
#if DEBUG
            menuTestItem.Visible = true;
#else
            menuTestItem.Visible = false;
#endif
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
            Monitor.Continue();
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

                try
                {
                    Cursor.Current = Cursors.WaitCursor;

                    DatabaseManager.Instance.OpenDatabase(ofd.FileName);

                    lock (activePluginList)
                    {
                        foreach (IPlugin plugin in activePluginList)
                        {
                            using (new ProfileRegion("Opening " + plugin.TabName))
                            {
                                plugin.DatabaseOpened(DatabaseManager.Instance.Database);
                            }
                        }
                    }
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void databaseReparse_Click(object sender, EventArgs e)
        {
            string outFilename;
            if (GetParseFileName(out outFilename) == true)
            {
                if (outFilename == DatabaseManager.Instance.DatabaseFilename)
                {
                    MessageBox.Show("Cannot save to the same file you want to reparse.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;

                try
                {
                    // Adjust what menu options are available
                    menuStopParse.Enabled = true;
                    menuBeginDefaultParse.Enabled = false;
                    menuBeginParseWithSave.Enabled = false;
                    menuOpenSavedData.Enabled = false;

                    Monitoring.DatabaseReader.Instance.ReparseProgressChanged += MonitorReparse;

                    try
                    {
                        toolStripStatusLabel.Text = "Reparsing...";

                        using (new ProfileRegion("Reparse"))
                        {
                            Monitor.Reparse(outFilename);
                        }
                    }
                    catch (Exception ex)
                    {
                        StopParsing();
                        toolStripStatusLabel.Text = "Error.  Parsing stopped.";
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

        private void menuSaveDataAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = defaultSaveDirectory;
            sfd.DefaultExt = "sdf";
            sfd.Title = "Select file to save parse data to...";

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
                    File.Copy(DatabaseManager.Instance.DatabaseFilename, sfd.FileName);
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
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            this.Shutdown();
            this.Close();
        }

        private void menuOptions_Click(object sender, EventArgs e)
        {
            Options optionsForm = new Options(Monitor.IsRunning);
            optionsForm.ShowDialog();
        }

        private void menuAbout_Click(object sender, EventArgs e)
        {
            AboutBox aboutForm = new AboutBox();
            aboutForm.ShowDialog();
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

        private void MonitorReparse(object sender, Monitoring.DatabaseReparseEventArgs e)
        {
            if (this.InvokeRequired)
            {
                Action<object, Monitoring.DatabaseReparseEventArgs> thisFunc = MonitorReparse;
                Invoke(thisFunc, new object[] { sender, e });
                return;
            }

            if (e.Complete == true)
            {
                // Adjust what menu options are available
                menuStopParse.Enabled = false;
                menuBeginDefaultParse.Enabled = true;
                menuBeginParseWithSave.Enabled = true;
                menuOpenSavedData.Enabled = true;

                toolStripStatusLabel.Text = "Status: Reparse complete.";
                //DatabaseManager.Instance.OpenDatabase(outFilename);

                lock (activePluginList)
                {
                    foreach (IPlugin plugin in activePluginList)
                    {
                        plugin.DatabaseOpened(DatabaseManager.Instance.Database);
                    }
                }

                Cursor.Current = Cursors.Default;
                Monitoring.DatabaseReader.Instance.ReparseProgressChanged -= MonitorReparse;

            }
            else
            {
                if (Monitoring.DatabaseReader.Instance.IsRunning == true)
                {
                    if (e.RowsRead == e.TotalRows)
                    {
                        toolStripStatusLabel.Text = string.Format("Status: Reparsing {0}/{1} -- Saving...",
                            e.RowsRead, e.TotalRows);
                    }
                    else
                    {
                        toolStripStatusLabel.Text = string.Format("Status: Reparsing {0}/{1}",
                            e.RowsRead, e.TotalRows);
                    }
                }
                else
                {
                    // ie: reparse was cancelled or errored out
                    // stop monitoring
                    Cursor.Current = Cursors.Default;
                    Monitoring.DatabaseReader.Instance.ReparseProgressChanged -= MonitorReparse;

                    // reload original database file
                    string oldDBFile = DatabaseReadingManager.Instance.DatabaseFilename;
                    if ((oldDBFile != null) && (oldDBFile != string.Empty))
                        DatabaseManager.Instance.OpenDatabase(oldDBFile);
                }
            }
        }

        #endregion

        #region Plugin/Window Management
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
        private void PopulatePluginMenu()
        {
            // This is only run once.

            // Add a separator under the About menu item if we have
            // any plugins available.
            if (pluginList.Count > 0)
            {
                MenuItem sep = new MenuItem("-");
                windowMenu.MenuItems.Add(sep);
            }
            else
            {
                return;
            }

            // Create a menu item and tab page for each plugin, with synced indexes.
            for (int i = 0; i < pluginList.Count; i++)
            {
                MenuItem mi = new MenuItem(pluginList[i].TabName);
                mi.Checked = activePluginList.Contains(pluginList[i]);
                mi.Click += new EventHandler(menuPlugin_Click);

                windowMenu.MenuItems.Add(i + 2, mi);

                TabPage tp = new TabPage(pluginList[i].TabName);
                tp.Tag = i.ToString();
                tabList.Add(tp);

                BuildTab(tp, pluginList[i]);
            }

            // Make sure active plugin tabs are visible.
            UpdateTabs();
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
        /// When a plugin is checked/unchecked from the Window menu, update
        /// the visible tabs.
        /// </summary>
        private void UpdateTabs()
        {
            TabPage tabToCheckFor;

            foreach (var plugin in pluginList)
            {
                tabToCheckFor = tabList[pluginList.IndexOf(plugin)];

                if (activePluginList.Contains(plugin))
                {
                    if (pluginTabs.Contains(tabToCheckFor) == false)
                    {
                        pluginTabs.TabPages.Add(tabToCheckFor);
                        tabToCheckFor.Focus();
                    }
                }
                else
                {
                    if (pluginTabs.Contains(tabToCheckFor) == true)
                    {
                        pluginTabs.TabPages.Remove(tabToCheckFor);
                    }
                }
            }
        }

        /// <summary>
        /// When a plugin is checked/unchecked from the Window menu, add or
        /// remove it from active plugin list, then update the visible tabs.
        /// </summary>
        private void menuPlugin_Click(object sender, EventArgs e)
        {
            MenuItem mi = sender as MenuItem;

            if (mi == null)
                return;

            // Toggle the checkmark
            mi.Checked = mi.Checked ^ true;

            if (mi.Checked == false)
            {
                lock (activePluginList)
                {
                    var plugin = pluginList[mi.Index - 2];
                    activePluginList.Remove(plugin);
                    windowSettings.activePluginList.Remove(plugin.TabName);
                }
            }
            else
            {
                var plugin = pluginList[mi.Index - 2];

                lock (activePluginList)
                {
                    activePluginList.Add(plugin);
                    windowSettings.activePluginList.Add(plugin.TabName);
                }

                if (DatabaseManager.Instance.Database != null)
                {
                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;

                        plugin.DatabaseOpened(DatabaseManager.Instance.Database);
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                    }
                }
            }

            UpdateTabs();
        }
        #endregion

        #region Parsing Control Methods
        private void StartParsing(string outputFileName)
        {
            // Adjust what menu options are available
            menuStopParse.Enabled = true;
            menuBeginDefaultParse.Enabled = false;
            menuBeginParseWithSave.Enabled = false;
            menuOpenSavedData.Enabled = false;

            // Let the database notify us of changes, and we'll notify the active plugins.
            DatabaseManager.Instance.DatabaseChanging += MonitorDatabaseChanging;
            DatabaseManager.Instance.DatabaseChanged += MonitorDatabaseChanged;

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
            // Adjust what menu options are available
            menuStopParse.Enabled = false;
            menuBeginDefaultParse.Enabled = true;
            menuBeginParseWithSave.Enabled = true;
            menuOpenSavedData.Enabled = true;

            Cursor.Current = Cursors.Default;

            Monitor.Stop();
            toolStripStatusLabel.Text = "Status: Stopped.";

            DatabaseManager.Instance.DatabaseChanging -= MonitorDatabaseChanging;
            DatabaseManager.Instance.DatabaseChanged -= MonitorDatabaseChanged;
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
        #endregion

        #region Code for testing stuff
        private void menuTestItem_Click(object sender, EventArgs e)
        {
            //MMHook.Hook("14,e8,12,80c08080,000000f6,0000010f,0019,00,01,02,00,Motenten uses Judgment.");

            Debug.WriteLine(string.Format("Company Name: {0}\n", Application.CompanyName));
            Debug.WriteLine(string.Format("Product Name: {0}\n", Application.ProductName));
            Debug.WriteLine(string.Format("Product Version: {0}\n", Application.ProductVersion));
            //Application.CommonAppDataPath;
            //Application.UserAppDataPath;
        }

        private void UpgradeCode()
        {

            // Recreate salvage parse uniqueness.

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                string inputDB = @"C:\Documents and Settings\David\My Documents\Visual Studio 2008\Projects\FFXILogParser\FFXILogParser\bin\Debug\SaveLogs\Salvage.sdf";

                DatabaseManager.Instance.OpenDatabase(inputDB);

                KPDatabaseDataSet readDataSet = DatabaseManager.Instance.Database;

                List<ChatLine> chatLineList = new List<ChatLine>(readDataSet.RecordLog.Count);

                foreach (var logLine in readDataSet.RecordLog)
                {
                    ChatLine chat = new ChatLine(logLine.MessageText, logLine.Timestamp);
                    chatLineList.Add(chat);
                }

                var grpChat = from c in chatLineList
                              group c by c.ChatText into uc
                              select uc;

                var uniqChat = from c in grpChat
                               orderby c.First().ChatText.Substring(27, 8)
                               select c.First();


                string outputDB = @"C:\Documents and Settings\David\My Documents\Visual Studio 2008\Projects\FFXILogParser\FFXILogParser\bin\Debug\SaveLogs\Salvage-Redo.sdf";

                DatabaseManager.Instance.CreateDatabase(outputDB);

                KPDatabaseDataSet writeDataSet = DatabaseManager.Instance.Database;

                foreach (var chat in uniqChat)
                {
                    writeDataSet.RecordLog.AddRecordLogRow(chat.Timestamp, chat.ChatText, false);
                }

                DatabaseManager.Instance.CloseDatabase();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            stopwatch.Stop();
            MessageBox.Show(string.Format("Completed convert in {0} seconds", stopwatch.Elapsed.TotalSeconds));
        }
        #endregion
    }
}