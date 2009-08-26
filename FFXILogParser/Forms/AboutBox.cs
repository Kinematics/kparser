using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using WaywardGamers.KParser.Database;

namespace WaywardGamers.KParser.Forms
{
    partial class AboutBox : Form
    {
        #region Member variables
        string readmePath;
        string changeLogPath;
        string errorLogPath;
        #endregion

        public AboutBox()
        {
            InitializeComponent();
            this.Text = String.Format("About {0}", AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = String.Format("Version {0}", AssemblyVersion);
            this.labelCopyright.Text = AssemblyCopyright;
            this.labelCompanyName.Text = AssemblyCompany;

            this.textBoxDescription.Text = AssemblyDescription;
            this.textBoxDescription.Text += DatabaseDescription;
            this.textBoxDescription.Text += OpenDatabaseDescription;


            string applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            readmePath = Path.Combine(applicationDirectory, "readme.txt");
            changeLogPath = Path.Combine(applicationDirectory, "changelog.txt");
            errorLogPath = Path.Combine(applicationDirectory, "error.log");

            errorLog.Enabled = File.Exists(errorLogPath);
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

        #region Other Info Accessors
        public string DatabaseDescription
        {
            get
            {
                return string.Format("\r\n\r\nUsing database version {0}.\r\n",
                    DatabaseManager.Instance.DatabaseVersion);
            }
        }

        public string OpenDatabaseDescription
        {
            get
            {
                if (DatabaseManager.Instance.IsDatabaseOpen)
                {
                    string dbParseVer = string.Empty;
                    string dbName = string.Empty;

                    using (Database.AccessToTheDatabase dbAccess = new AccessToTheDatabase())
                    {
                        if (dbAccess.Database.Version.Count > 0)
                        {
                            dbParseVer = dbAccess.Database.Version[0].ParserVersion;
                        }
                    }

                    dbName = (new System.IO.FileInfo(DatabaseManager.Instance.DatabaseFilename)).Name;

                    return string.Format("\r\nCurrent database: {0}\r\n  Parsed using parser version {1}\r\n",
                            dbName, dbParseVer);
                }

                return string.Empty;
            }
        }
        #endregion

        #region Open various text files
        private void readme_Click(object sender, EventArgs e)
        {
            if (File.Exists(readmePath))
            {
                Process.Start(readmePath);
            }
            else
            {
                MessageBox.Show("Unable to locate readme.txt file.");
            }
        }

        private void releaseNotes_Click(object sender, EventArgs e)
        {
            if (File.Exists(changeLogPath))
            {
                Process.Start(changeLogPath);
            }
            else
            {
                MessageBox.Show("Unable to locate changelog.txt file.");
            }
        }

        private void errorLog_Click(object sender, EventArgs e)
        {
            if (File.Exists(errorLogPath))
            {
                Process.Start(errorLogPath);
            }
        }
        #endregion
    }
}
