using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
//using Microsoft.Office.Interop.Excel;


namespace WaywardGamers.KParser
{
    /// <summary>
    /// Class to convert a dataset to an html stream which can be used to display the dataset
    /// in MS Excel.
    /// </summary>
    public class ExcelExport
    {
        /// <summary>
        /// Convert the first table of the dataset.
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="response"></param>
        public static void Convert(DataSet dataSet, HttpResponse response)
        {
            foreach (DataTable table in dataSet.Tables)
            {
                Convert(table, response);
            }
        }

        /// <summary>
        /// Convert the specified table number of the dataset.
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="tableIndex"></param>
        /// <param name="response"></param>
        public static void Convert(DataSet dataSet, int tableIndex, HttpResponse response)
        {
            DataTable table = dataSet.Tables[tableIndex];
            Convert(table, response);
        }

        /// <summary>
        /// Convert the specified named table of the dataset.
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="tableName"></param>
        /// <param name="response"></param>
        public static void Convert(DataSet dataSet, string tableName, HttpResponse response)
        {
            DataTable table = dataSet.Tables[tableName];
            Convert(table, response);
        }

        /// <summary>
        /// Convert the provided datatable.
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="response"></param>
        public static void Convert(DataTable dataTable, HttpResponse response)
        {
            response.Clear();
            //response.Charset = "";

            // Set the response mime type for excel
            response.ContentType = "application/vnd.ms-excel";

            StringWriter sw = new StringWriter();

            // Create an htmltextwriter which uses the stringwriter
            Html32TextWriter htmlSW = new Html32TextWriter(sw);

            // Create a DataGrid for intermediary transition of data.
            DataGrid dataGrid = new DataGrid();

            dataGrid.DataSource = dataTable;

            dataGrid.DataBind();

            // Render the data the datagrid contains to our textwriter.
            dataGrid.RenderControl(htmlSW);

            // Output the textwriter text to our response object.
            response.Write(sw.ToString());
            //response.End();
        }
    }
}
