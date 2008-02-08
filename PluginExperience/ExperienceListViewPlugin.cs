using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WaywardGamers.KParser.Plugin
{
    class ExperienceListViewPlugin : BasePluginControlListView
    {
        public override string TabName
        {
            get { return "Experience2"; }
        }

        public override void Reset()
        {
            // Format the listView control to our needs
            // Total Exp
            // Total Fights
            // Start Time
            // End Time
            // Exp/Hour
            // Avg Fight Length
            
            listView.Columns.Clear();
            listView.Columns.Add("Field", 150, System.Windows.Forms.HorizontalAlignment.Left);
            listView.Columns.Add("Data", 150, System.Windows.Forms.HorizontalAlignment.Left);
            listView.AllowColumnReorder = false;

            listView.Sorting = System.Windows.Forms.SortOrder.None;
            
            listView.Groups.Add(new ListViewGroup("Rates", HorizontalAlignment.Left));
            listView.Groups.Add(new ListViewGroup("Uddd", HorizontalAlignment.Left));


            
            string[] values = new string[] {
                "Total Experience", "Total Fights", "Start Time", "End Time",
                "Exp/Hour", "Exp/Minute", "Exp/Fight",
                "Avg Fight Length", "Avg Time/Fight"};

            ListViewItem lvi;
            foreach (string val in values)
            {
                if (val.StartsWith("Total"))
                    lvi = new ListViewItem(val, listView.Groups["Uddd"]);
                else
                    lvi = new ListViewItem(val, listView.Groups["Rates"]);

                listView.Items.Add(lvi);
            }

            listView.ShowGroups = true;
            
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseODataSet datasetToUse)
        {
            if (e.DatasetChanges.Battles.Any(b => b.Killed == true))
            {
                datasetToUse = e.FullDataset;
                return true;
            }

            datasetToUse = null;
            return false;
        }


    }
}
