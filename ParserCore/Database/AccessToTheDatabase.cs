using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace WaywardGamers.KParser.Database
{
    public class AccessToTheDatabase : IDisposable
    {
        private string regionName;
        private KPDatabaseDataSet databaseRef;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileRegion"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public AccessToTheDatabase(string name)
        {
            regionName = name;
            Debug.WriteLine(string.Concat("Entering database access region (", regionName, ")."));

            databaseRef = DatabaseManager.Instance.GetDatabaseForReading();
        }

        public KPDatabaseDataSet Database
        {
            get
            {
                return databaseRef;
            }
        }


        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="ProfileRegion"/> is reclaimed by garbage collection.
        /// </summary>
        ~AccessToTheDatabase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        public void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                DatabaseManager.Instance.DoneReadingDatabase();

                Debug.WriteLine(string.Concat("Exiting database access region (", regionName, ")."));
            }
        }
    }

}
