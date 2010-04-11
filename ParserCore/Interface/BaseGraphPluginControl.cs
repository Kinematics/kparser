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
    public partial class BaseGraphPluginControl : BasePluginControl
    {
        public BaseGraphPluginControl()
        {
            InitializeComponent();


            IsActive = false;
            MobXPHandler.Instance.CustomMobFilterChanged += this.CustomMobFilterChanged;

            // Don't call this during the base constructor.  Let the plugins call it themselves.
            // While it has the same effect, putting it in the constructor allows for
            // an explicit reminder of what needs to be set.
            //LoadLocalizedUI();
            LoadResources();

        }

        protected void ResetGraph()
        {
        }
    }
}
