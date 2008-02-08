using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;

namespace WaywardGamers.KParser.Plugin
{
    /// <summary>
    /// Class to handle a LINQ query result so that it can be passed
    /// as a function argument.
    /// </summary>
    internal class AttackGroup
    {
        internal ActionType ActionSource { get; set; }
        internal IEnumerable<IGrouping<KPDatabaseODataSet.CombatantsRow, KPDatabaseODataSet.CombatDetailsRow>>
            CombatGroup { get; set; }

        public AttackGroup(ActionType key,
            IEnumerable<IGrouping<KPDatabaseODataSet.CombatantsRow, KPDatabaseODataSet.CombatDetailsRow>> grouping)
        {
            ActionSource = key;
            CombatGroup = grouping;
        }
    }
}
