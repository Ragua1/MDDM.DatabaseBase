using System.Data;
using System.Data.SqlClient;
using MDDM.DatabaseBase.DataClasses;
using MDDM.DatabaseBase.Test.Models;

namespace MDDM.DatabaseBase.Test
{
    internal class ExampleDb : SqlBase
    {
        internal ExampleDb(string connectionString, IsolationLevel defaultIsolationLevel = IsolationLevel.Unspecified) : base(connectionString, defaultIsolationLevel)
        {
        }

        internal int InsertData(Table_1 data)
        {
            var cmd = new SqlCommand("INSERT INTO [Table_1] ([ColText], [ColInt], [ColDate]) VALUES (@ColText, @ColInt, @ColDate); SELECT SCOPE_IDENTITY() ");

            cmd.Parameters.AddWithValue("ColText", (object?)data.ColText ?? DBNull.Value);
            cmd.Parameters.AddWithValue("ColInt", (object?)data.ColInt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("ColDate", (object?)data.ColDate ?? DBNull.Value);

            var newId = ExecuteInsertCommand(cmd);
            return Convert.ToInt32(newId);
        }

        internal async Task<int> InsertDataAsync(Table_1 data)
        {
            var cmd = new SqlCommand("INSERT INTO [Table_1] ([ColText], [ColInt], [ColDate]) VALUES (@ColText, @ColInt, @ColDate); SELECT SCOPE_IDENTITY() ");

            cmd.Parameters.AddWithValue("ColText", (object?)data.ColText ?? DBNull.Value);
            cmd.Parameters.AddWithValue("ColInt", (object?)data.ColInt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("ColDate", (object?)data.ColDate ?? DBNull.Value);

            var newId = await ExecuteInsertCommandAsync(cmd);
            return Convert.ToInt32(newId);
        }

        internal Table_1? SelectData(int id)
        {
            var cmd = new SqlCommand("SELECT [Id], [Updated], [ColText], [ColInt], [ColDate] FROM [Table_1] WHERE Id = @Id");
            cmd.Parameters.AddWithValue("Id", id);

            var reader = ExecuteSelectCommand(cmd);
            if (reader.Read())
            {
                var data = new Table_1
                {
                    Id = GetValueFromDataReader(reader, nameof(Table_1.Id), -1),
                    Updated = GetDateTimeFromDataReader(reader, nameof(Table_1.Updated)),
                    ColText = GetStringFromDataReader(reader, nameof(Table_1.ColText)),
                    ColInt = GetValueFromDataReader(reader, nameof(Table_1.ColInt), -1),
                    ColDate = GetDateTimeFromDataReader(reader, nameof(Table_1.ColDate)),
                };
                return data;
            }

            return null;
        }

        internal async Task<Table_1?> SelectDataAsync(int id)
        {
            var cmd = new SqlCommand("SELECT [Id], [Updated], [ColText], [ColInt], [ColDate] FROM [Table_1] WHERE Id = @Id");
            cmd.Parameters.AddWithValue("Id", id);

            var reader = await ExecuteSelectCommandAsync(cmd);
            if (reader.Read())
            {
                var data = new Table_1
                {
                    Id = await GetValueFromDataReaderAsync(reader, nameof(Table_1.Id), -1),
                    Updated = await GetDateTimeFromDataReaderAsync(reader, nameof(Table_1.Updated)),
                    ColText = await GetStringFromDataReaderAsync(reader, nameof(Table_1.ColText)),
                    ColInt = await GetValueFromDataReaderNullableAsync<int>(reader, nameof(Table_1.ColInt)),
                    ColDate = await GetDateTimeFromDataReaderNullableAsync(reader, nameof(Table_1.ColDate)),
                };
                return data;
            }

            return null;
        }

        internal bool DeleteData(int id)
        {
            var cmd = new SqlCommand("DELETE FROM [dbo].[Table_1] WHERE Id = @Id");
            cmd.Parameters.AddWithValue("Id", id);

            var reader = ExecuteAdjustCommand(cmd);

            return reader > 0;
        }

        internal async Task<bool> DeleteDataAsync(int id)
        {
            var cmd = new SqlCommand("DELETE FROM [dbo].[Table_1] WHERE Id = @Id");
            cmd.Parameters.AddWithValue("Id", id);

            var reader = await ExecuteAdjustCommandAsync(cmd);

            return reader > 0;
        }


    }
}
