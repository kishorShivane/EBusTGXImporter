using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBusTGXImporter.DataProvider
{
    public static class DbHelper
    {
        public static void BulkCopyDataToTable<T>(string tableName, List<T> dataToCopy, SqlConnection connection,SqlTransaction transaction)
        {
            SqlBulkCopy bulkCopy = null;
            DataTable table = null;
            try
            {
                table = ToDataTable<T>(dataToCopy);
                bulkCopy = new SqlBulkCopy(connection,SqlBulkCopyOptions.Default, transaction);
                bulkCopy.BulkCopyTimeout = 0;   // Sets the timeout to unlimited

                // Iterates through each column in the datatable
                foreach (DataColumn column in table.Columns)
                {
                    // Makes a connection map between the datatable and the database table
                    bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                // Sets the desitination table
                bulkCopy.DestinationTableName = tableName;
                bulkCopy.WriteToServer(table);
                bulkCopy.Close();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                bulkCopy = null;
                table.Dispose();
            }
        }

        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }
    }
}
