using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WaywardGamers.KParser.Forms
{
    public partial class ImportType : Form
    {
        public ImportType()
        {
            InitializeComponent();
        }

        private void ImportType_Load(object sender, EventArgs e)
        {
            optionDVSParse.Checked = true;
        }

        internal ImportSourceType ImportSource
        {
            get
            {
                if (optionDVSParse.Checked == true)
                    return ImportSourceType.DVSParse;

                return ImportSourceType.KParser;
            }
        }
    }
}
