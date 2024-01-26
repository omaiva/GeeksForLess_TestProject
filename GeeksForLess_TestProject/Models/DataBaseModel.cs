using Microsoft.Data.SqlClient;
using System.Data;

namespace GeeksForLess_TestProject.Models
{
    public class DataBaseModel
    {
        public static void UploadDataTableToSql(DataTable dataTable, string destinationTableName, string webRootPath)
        {
            string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"" + webRootPath + "\\Database.mdf\";Integrated Security=True;Connect Timeout=30";

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                using (SqlCommand command = new SqlCommand($"DROP TABLE {destinationTableName}", sqlConnection))
                {
                    command.ExecuteNonQuery();
                }

                CreateTable(sqlConnection, destinationTableName);

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

        private static void CreateTable(SqlConnection sqlConnection, string destinationTableName)
        {
            using (SqlCommand command = new SqlCommand($"CREATE TABLE {destinationTableName} (" +
                $"ObjectId int IDENTITY PRIMARY KEY," +
                $"Name nvarchar(max)," +
                $"ParentId int FOREIGN KEY REFERENCES {destinationTableName}(ObjectId) );", sqlConnection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
