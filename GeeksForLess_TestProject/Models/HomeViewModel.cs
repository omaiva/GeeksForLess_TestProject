using Newtonsoft.Json.Linq;
using System.Data;
using Microsoft.Data.SqlClient;

namespace GeeksForLess_TestProject.Models
{
    public class HomeViewModel
    {
        public static DataTable ConvertToDataTable(string json)
        {
            var jsonObject = JObject.Parse(json);
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ObjectId", typeof(int));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("ParentId", typeof(int));

            AddRows(jsonObject, dataTable, ref idCounter);
            return dataTable;
        }

        private static int idCounter = 1;

        private static void AddRows(JObject jsonObject, DataTable dataTable, ref int idCounter, int parentId = 0)
        {
            foreach (var property in jsonObject.Properties())
            {
                int currentId = idCounter++;
                DataRow row = dataTable.NewRow();
                row["ObjectId"] = currentId;
                row["ParentId"] = parentId;
                row["Name"] = property.Name;
                dataTable.Rows.Add(row);

                if (property.Value.Type == JTokenType.Object)
                {
                    var nestedObject = (JObject)property.Value;
                    AddRows(nestedObject, dataTable, ref idCounter, currentId);
                }
                else
                {
                    DataRow valueRow = dataTable.NewRow();
                    valueRow["ObjectId"] = idCounter++;
                    valueRow["ParentId"] = currentId;
                    valueRow["Name"] = property.Value.ToString();
                    dataTable.Rows.Add(valueRow);
                }
            }
        }

        public static void UploadDataTableToSql(DataTable dataTable, string destinationTableName)
        {
            string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"D:\\Programming\\c#\\projects\\ASP.NET Core\\GeeksForLess_TestProject\\GeeksForLess_TestProject\\Database.mdf\";Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True";


            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                using (SqlCommand command = new SqlCommand($"TRUNCATE TABLE {destinationTableName}", sqlConnection))
                {
                    command.ExecuteNonQuery();
                }

                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(sqlConnection))
                {
                    sqlBulkCopy.DestinationTableName = destinationTableName;

                    foreach (DataColumn column in dataTable.Columns)
                    {
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName, column.ColumnName));
                    }

                    sqlBulkCopy.WriteToServer(dataTable);
                }
            }
        }
    }
}
