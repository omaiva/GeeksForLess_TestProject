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
            AddColumns(jsonObject, dataTable, "");
            AddRows(jsonObject, dataTable, "");
            return dataTable;
        }

        private static void AddColumns(JObject jsonObject, DataTable dataTable, string prefix)
        {
            foreach (var property in jsonObject.Properties())
            {
                var columnName = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}_{property.Name}";

                if (property.Value.Type == JTokenType.Object)
                {
                    var nestedObject = (JObject)property.Value;
                    AddColumns(nestedObject, dataTable, columnName);
                }
                else
                {
                    dataTable.Columns.Add(columnName);
                }
            }
        }

        private static void AddRows(JObject jsonObject, DataTable dataTable, string prefix)
        {
            var row = dataTable.NewRow();

            foreach (var property in jsonObject.Properties())
            {
                var columnName = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}_{property.Name}";

                if (property.Value.Type == JTokenType.Object)
                {
                    var nestedObject = (JObject)property.Value;
                    AddRows(nestedObject, dataTable, columnName);
                }
                else
                {
                    row[columnName] = property.Value.ToString();
                }
            }

            dataTable.Rows.Add(row);
        }

        public static void UploadDataTableToSql(DataTable dataTable, string destinationTableName)
        {
            string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"D:\\Programming\\c#\\projects\\ASP.NET Core\\GeeksForLess_TestProject\\GeeksForLess_TestProject\\Database.mdf\";Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True";


            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                using (SqlCommand command = new SqlCommand($"DROP TABLE {destinationTableName}", sqlConnection))
                {
                    command.ExecuteNonQuery();
                }

                CreateDestinationTable(sqlConnection, dataTable, destinationTableName);

                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(sqlConnection))
                {
                    sqlBulkCopy.DestinationTableName = destinationTableName;

                    foreach (DataColumn column in dataTable.Columns)
                    {
                        sqlBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }

                    sqlBulkCopy.WriteToServer(dataTable);
                }
            }
        }

        private static void CreateDestinationTable(SqlConnection sqlConnection, DataTable dataTable, string tableName)
        {
            using (SqlCommand command = new SqlCommand(BuildCreateTableQuery(dataTable, tableName), sqlConnection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static string BuildCreateTableQuery(DataTable dataTable, string tableName)
        {
            string createTableQuery = $"CREATE TABLE {tableName} (";

            foreach (DataColumn column in dataTable.Columns)
            {
                createTableQuery += $"{column.ColumnName} {GetSqlDbType(column.DataType)}, ";
            }
            
            createTableQuery = createTableQuery.TrimEnd(',', ' ') + ")";

            return createTableQuery;
        }

        private static string GetSqlDbType(Type dataType)
        {
            if (dataType == typeof(int))
                return "INT";
            else if (dataType == typeof(string))
                return "NVARCHAR(MAX)";

            return "NVARCHAR(MAX)";
        }
    }
}
