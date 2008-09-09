﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace WaywardGamers.KParser.Plugin
{
    /// <summary>
    /// Class to define thread-safe extension methods for plugin UI elements.
    /// </summary>
    public static class PluginExtensions
    {
        #region Combo Box manipulation
        public static void CBReset(this ToolStripComboBox combo)
        {
            if (combo.ComboBox.InvokeRequired)
            {
                Action<ToolStripComboBox> thisFunc = CBReset;
                combo.ComboBox.Invoke(thisFunc);
                return;
            }

            combo.Items.Clear();
        }

        public static void CBAddStrings(this ToolStripComboBox combo, string[] strings)
        {
            if (combo.ComboBox.InvokeRequired)
            {
                Action<ToolStripComboBox, string[]> thisFunc = CBAddStrings;
                combo.ComboBox.Invoke(thisFunc, new object[] { strings });
                return;
            }

            if (strings == null)
                throw new ArgumentNullException("strings");

            if (strings.Length == 0)
                return;

            combo.Items.AddRange(strings);
        }

        public static string[] CBGetStrings(this ToolStripComboBox combo)
        {
            if (combo.ComboBox.InvokeRequired)
            {
                Func<ToolStripComboBox, string[]> thisFunc = CBGetStrings;
                return (string[])combo.ComboBox.Invoke(thisFunc);
            }

            string[] stringList = new string[combo.Items.Count];
            for (int i = 0; i < combo.Items.Count; i++)
            {
                stringList[i] = combo.Items[i].ToString();
            }

            return stringList;
        }

        public static void CBSelectIndex(this ToolStripComboBox combo, int index)
        {
            if (combo.ComboBox.InvokeRequired)
            {
                Action<ToolStripComboBox, int> thisFunc = CBSelectIndex;
                combo.ComboBox.Invoke(thisFunc, new object[] { index });
                return;
            }

            if (index < 0)
                index = combo.Items.Count - 1;

            if (index >= 0)
                combo.SelectedIndex = index;
        }

        public static void CBSelectString(this ToolStripComboBox combo, string name)
        {
            if (combo.ComboBox.InvokeRequired)
            {
                Action<ToolStripComboBox, string> thisFunc = CBSelectString;
                combo.ComboBox.Invoke(thisFunc, new object[] { name });
                return;
            }

            if (name == null)
                throw new ArgumentNullException("name");

            if (combo.Items.Count > 0)
                combo.SelectedItem = name;
        }

        public static int CBSelectedIndex(this ToolStripComboBox combo)
        {
            if (combo.ComboBox.InvokeRequired)
            {
                Func<ToolStripComboBox, int> thisFunc = CBSelectedIndex;
                return (int)combo.ComboBox.Invoke(thisFunc);
            }

            return combo.SelectedIndex;
        }

        public static string CBSelectedItem(this ToolStripComboBox combo)
        {
            if (combo.ComboBox.InvokeRequired)
            {
                Func<ToolStripComboBox, string> thisFunc = CBSelectedItem;
                return (string)combo.ComboBox.Invoke(thisFunc);
            }

            if (combo.SelectedIndex >= 0)
                return combo.SelectedItem.ToString();
            else
                return string.Empty;
        }
        #endregion
    }
}

