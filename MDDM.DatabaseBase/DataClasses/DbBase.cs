using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using MDDM.DatabaseBase.Interfaces;

namespace MDDM.DatabaseBase.DataClasses
{
    public class DbBase : IDbBase
    {
        protected IsolationLevel DefaultIsolationLevel = IsolationLevel.Unspecified;

        protected readonly string CONN_STRING;
        private SqlConnection SqlConnection => this.sqlConnection ??= new SqlConnection(CONN_STRING);

        private SqlConnection sqlConnection;
        private SqlTransaction transaction;

        protected DbBase(string connectionString)
        {
            CONN_STRING = connectionString ?? throw new ArgumentNullException($"Argument '{nameof(connectionString)}' cannot be null!");
        }


        public void OpenConnection()
        {
            if (this.SqlConnection.State != ConnectionState.Open)
            {
                this.SqlConnection.Open();
            }
        }

        public void CloseConnection()
        {
            this.transaction = null;

            if (this.SqlConnection.State != ConnectionState.Closed)
            {
                this.SqlConnection.Close();
            }
        }

        public void BeginTransaction(IsolationLevel? isolationLevel = null, string transactionName = default)
        {
            OpenConnection();

            this.transaction = this.sqlConnection.BeginTransaction(isolationLevel ?? DefaultIsolationLevel, transactionName);
        }

        public void CommitTransaction()
        {
            this.transaction.Commit();
            CloseConnection();
        }

        public void RollbackTransaction()
        {
            this.transaction?.Rollback();
            CloseConnection();
        }

        protected T GetValueFromDataReader<T>(SqlDataReader reader, int index, T nullValue = default)
        {
            return reader[index] != DBNull.Value ? ((T)reader[index]) : nullValue;
        }
        protected async Task<T> GetValueFromDataReaderAsync<T>(SqlDataReader reader, int index, T nullValue = default, CancellationToken cancellationToken = default)
        {
            return await reader.IsDBNullAsync(index, cancellationToken)
                ? nullValue
                : await reader.GetFieldValueAsync<T>(index, cancellationToken);
        }

        protected string GetStringFromDataReader(SqlDataReader reader, string columnName, string nullValue = "")
        {
            return reader[columnName] != DBNull.Value ? ((string)reader[columnName]) : nullValue;
        }
        protected async Task<string> GetStringFromDataReaderAsync(SqlDataReader reader, string columnName, string nullValue = "", CancellationToken cancellationToken = default)
        {
            return await reader.IsDBNullAsync(columnName, cancellationToken)
                ? nullValue
                : await reader.GetFieldValueAsync<string>(columnName, cancellationToken);
        }
        protected DateTime GetDateTimeFromDataReader(SqlDataReader reader, string columnName, DateTime nullValue = default)
        {
            return GetDateTimeFromDataReaderNullable(reader, columnName) ?? nullValue;
        }
        protected T GetValueFromDataReader<T>(SqlDataReader reader, string columnName, T nullValue = default) where T : struct
        {
            return GetValueFromDataReaderNullable<T>(reader, columnName) ?? nullValue;
        }
        protected async Task<T> GetValueFromDataReaderAsync<T>(SqlDataReader reader, string columnName, T nullValue = default, CancellationToken cancellationToken = default) where T : struct
        {
            return await GetValueFromDataReaderNullableAsync<T>(reader, columnName, cancellationToken) ?? nullValue;
        }

        protected DateTime? GetDateTimeFromDataReaderNullable(SqlDataReader reader, string columnName)
        {
            var colValue = reader[columnName];
            if (colValue != DBNull.Value)
            {
                return DateTime.SpecifyKind((DateTime)colValue, DateTimeKind.Utc);
            }
            else
            {
                return null;
            }
        }
        protected T? GetValueFromDataReaderNullable<T>(SqlDataReader reader, string columnName) where T : struct
        {
            var colValue = reader[columnName];
            if (colValue != DBNull.Value)
            {
                return (T)colValue;
            }
            else
            {
                return null;
            }
        }
        protected async Task<T?> GetValueFromDataReaderNullableAsync<T>(SqlDataReader reader, string columnName, CancellationToken cancellationToken = default) where T : struct
        {
            return !await reader.IsDBNullAsync(columnName, cancellationToken)
                ? (T?) await reader.GetFieldValueAsync<T>(columnName, cancellationToken)
                : null;
        }

        protected async Task<SqlDataReader> ExecuteProcedureCommandAsync(SqlCommand command, CancellationToken token = default)
        {
            // use the connection here
            command.Connection = this.SqlConnection;
            command.CommandType = CommandType.StoredProcedure;
            command.Transaction = this.transaction; // null if not transaction

            if (command.Connection.State != ConnectionState.Open)
            {
                await command.Connection.OpenAsync(token);
            }

            var commandBehavior = this.transaction != null
                ? CommandBehavior.Default
                : CommandBehavior.CloseConnection;
            
            return await command.ExecuteReaderAsync(
                commandBehavior,
                token).ConfigureAwait(false);
        }

        protected SqlDataReader ExecuteProcedureCommand(SqlCommand command)
        {
            // use the connection here
            command.Connection = this.SqlConnection;
            command.CommandType = CommandType.StoredProcedure;

            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
            }
            
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        protected async Task<SqlDataReader> ExecuteSelectCommandAsync(SqlCommand command, CancellationToken token = default)
        {
            // use the connection here
            command.Connection = new SqlConnection(CONN_STRING); // nevytvaret novou instanci
            command.CommandType = CommandType.Text;

            if (command.Connection.State != ConnectionState.Open)
            {
                await command.Connection.OpenAsync(token);
            }


            return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, token).ConfigureAwait(false);
        }

        protected SqlDataReader ExecuteSelectCommand(SqlCommand command)
        {
            // use the connection here
            command.Connection = this.SqlConnection;
            command.CommandType = CommandType.Text;

            OpenConnection();
            
            return command.ExecuteReader();
        }

        /// <summary>
        /// Executes the insert command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>ID of inserted row</returns>
        protected int ExecuteInsertCommand(SqlCommand command)
        {
            // use the connection here
            command.Connection = this.SqlConnection;
            command.CommandType = CommandType.Text;
            command.Transaction = this.transaction;

            OpenConnection();

            return (int)(command.ExecuteScalar() ?? -1);
        }

        /// <summary>
        /// Executes the adjust command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>Count of affected rows</returns>
        protected int ExecuteAdjustCommand(SqlCommand command)
        {
            // use the connection here
            command.Connection = this.SqlConnection;
            command.CommandType = CommandType.Text;
            command.Transaction = this.transaction;

            OpenConnection();
            
            return command.ExecuteNonQuery();
        }

        protected async Task<int> ExecuteAdjustCommandAsync(SqlCommand command, CancellationToken token = default)
        {
            // use the connection here
            command.Connection = this.SqlConnection;
            command.CommandType = CommandType.Text;
            command.Transaction = this.transaction;

            OpenConnection();
            
            return await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }
    }
}
