using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection;

namespace ETLWorkerService.Infrastructure.Data.BulkInsert
{
    public static class BulkInsertHelper
    {
        public static async Task BulkInsertAsync<T>(
            IEnumerable<T> entities,
            string connectionString,
            string tableName,
            int batchSize = 10000) where T : class
        {
            if (!entities.Any())
                return;

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = tableName,
                BatchSize = batchSize,
                BulkCopyTimeout = 300
            };

            var dataTable = ConvertToDataTable(entities);
            
            foreach (DataColumn column in dataTable.Columns)
            {
                bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            }

            await bulkCopy.WriteToServerAsync(dataTable);
        }

        private static DataTable ConvertToDataTable<T>(IEnumerable<T> entities) where T : class
        {
            var dataTable = new DataTable();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var columnType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                dataTable.Columns.Add(prop.Name, columnType);
            }

            foreach (var entity in entities)
            {
                var row = dataTable.NewRow();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(entity);
                    row[prop.Name] = value ?? DBNull.Value;
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        public static async Task TruncateTableAsync(string connectionString, string tableName)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            
            using var command = new SqlCommand($"TRUNCATE TABLE {tableName}", connection);
            await command.ExecuteNonQueryAsync();
        }

        public static async Task<Dictionary<TKey, TValue>> ExecuteQueryToDictionaryAsync<TKey, TValue>(
            string connectionString,
            string query) where TKey : notnull
        {
            var result = new Dictionary<TKey, TValue>();
            
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            
            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var keyValue = reader.GetValue(0);
                var valueValue = reader.GetValue(1);
                
                var key = (TKey)Convert.ChangeType(keyValue, typeof(TKey));
                var value = (TValue)Convert.ChangeType(valueValue, typeof(TValue));
                result[key] = value;
            }
            
            return result;
        }
    }
}
