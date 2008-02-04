using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser
{
    /// <summary>
    /// Class to hold any 'special' values that are set due to some
    /// random determination.
    /// </summary>
    public static class MagicNumbers
    {
        // The minimum time that SQLCE server can handle in a Date/Time field is
        // January 01, 1753.  Using the standard Windows DateTime.MinValue (January 01, 0001)
        // generates an overflow error.  Since I'm not sure if the 1753 date accounts
        // for variations in time zones, I added 1 day to set the magic value.
        public static readonly DateTime MinSQLDateTime = DateTime.Parse("January 02, 1753 0:00 AM");
    }
}
