using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using MDDM.DatabaseBase.Interfaces;

namespace MDDM.DatabaseBase.DataClasses
{
    public abstract class DbBase : IDbBase
    {
        private readonly IsolationLevel defaultIsolationLevel;

        protected readonly string CONN_STRING;
        protected DbConnection DbConnection { get; set; }
        protected DbTransaction DbTransaction { get; set; }
        
        protected DbBase(string connectionString, IsolationLevel defaultIsolationLevel = IsolationLevel.Unspecified)
        {
            CONN_STRING = connectionString ?? throw new ArgumentNullException($"Argument '{nameof(connectionString)}' cannot be null!");
            this.defaultIsolationLevel = defaultIsolationLevel;
        }


        public void OpenConnection()
        {
            if (this.DbConnection.State != ConnectionState.Open)
            {
                this.DbConnection.Open();
            }
        }
        public async Task OpenConnectionAsync(CancellationToken token = default)
        {
            if (this.DbConnection.State != ConnectionState.Open)
            {
                await DbConnection.OpenAsync(token).ConfigureAwait(false);
            }
        }

        public void CloseConnection()
        {
            this.DbTransaction = null;

            if (this.DbConnection.State != ConnectionState.Closed)
            {
                this.DbConnection.Close();
            }
        }

        public async Task CloseConnectionAsync()
        {
            this.DbTransaction = null;

            if (this.DbConnection.State != ConnectionState.Closed)
            {
                await DbConnection.CloseAsync().ConfigureAwait(false);
            }
        }

        public virtual void BeginTransaction(IsolationLevel? isolationLevel = null)
        {
            OpenConnection();

            this.DbTransaction = this.DbConnection.BeginTransaction(isolationLevel ?? defaultIsolationLevel);
        }

        public async Task BeginTransactionAsync(IsolationLevel? isolationLevel = null, CancellationToken token = default)
        {
            await OpenConnectionAsync(token).ConfigureAwait(false);

            this.DbTransaction = await DbConnection.BeginTransactionAsync(isolationLevel ?? defaultIsolationLevel, token).ConfigureAwait(false);
        }

        public virtual void CommitTransaction()
        {
            this.DbTransaction.Commit();
            CloseConnection();
        }

        public async Task CommitTransactionAsync(CancellationToken token = default)
        {
            if (DbTransaction != null)
            {
                await DbTransaction.CommitAsync(token).ConfigureAwait(false);
            }
            await CloseConnectionAsync().ConfigureAwait(false);
        }

        public virtual void RollbackTransaction()
        {
            this.DbTransaction?.Rollback();
            CloseConnection();
        }

        public async Task RollbackTransactionAsync(CancellationToken token = default)
        {
            if (DbTransaction != null)
            {
                await DbTransaction.RollbackAsync(token).ConfigureAwait(false);
            }
            await CloseConnectionAsync().ConfigureAwait(false);
        }

        protected T GetValueFromDataReader<T>(DbDataReader reader, int index, T nullValue = default)
        {
            return reader[index] != DBNull.Value ? ((T)reader[index]) : nullValue;
        }
        protected async Task<T> GetValueFromDataReaderAsync<T>(DbDataReader reader, int index, T nullValue = default, CancellationToken cancellationToken = default)
        {
            return await reader.IsDBNullAsync(index, cancellationToken).ConfigureAwait(false)
                ? nullValue
                : await reader.GetFieldValueAsync<T>(index, cancellationToken).ConfigureAwait(false);
        }

        protected string GetStringFromDataReader(DbDataReader reader, string columnName, string nullValue = "")
        {
            return reader[columnName] != DBNull.Value ? ((string)reader[columnName]) : nullValue;
        }
        protected async Task<string> GetStringFromDataReaderAsync(DbDataReader reader, string columnName, string nullValue = "", CancellationToken cancellationToken = default)
        {
            return await reader.IsDBNullAsync(columnName, cancellationToken).ConfigureAwait(false)
                ? nullValue
                : await reader.GetFieldValueAsync<string>(columnName, cancellationToken).ConfigureAwait(false);
        }
        protected DateTime GetDateTimeFromDataReader(DbDataReader reader, string columnName, DateTime nullValue = default)
        {
            return GetDateTimeFromDataReaderNullable(reader, columnName) ?? nullValue;
        }
        protected T GetValueFromDataReader<T>(DbDataReader reader, string columnName, T nullValue = default) where T : struct
        {
            return GetValueFromDataReaderNullable<T>(reader, columnName) ?? nullValue;
        }
        protected async Task<T> GetValueFromDataReaderAsync<T>(DbDataReader reader, string columnName, T nullValue = default, CancellationToken cancellationToken = default) where T : struct
        {
            return await GetValueFromDataReaderNullableAsync<T>(reader, columnName, cancellationToken).ConfigureAwait(false) ?? nullValue;
        }

        protected DateTime? GetDateTimeFromDataReaderNullable(DbDataReader reader, string columnName)
        {
            if (reader == null)
            {
                return null;
            }

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
        protected T? GetValueFromDataReaderNullable<T>(DbDataReader reader, string columnName) where T : struct
        {
            if (reader == null)
            {
                return null;
            }

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
        protected async Task<T?> GetValueFromDataReaderNullableAsync<T>(DbDataReader reader, string columnName, CancellationToken cancellationToken = default) where T : struct
        {
            return !await reader.IsDBNullAsync(columnName, cancellationToken).ConfigureAwait(false)
                ? (T?) await reader.GetFieldValueAsync<T>(columnName, cancellationToken).ConfigureAwait(false)
                : null;
        }

        protected async Task<DbDataReader> ExecuteProcedureCommandAsync(DbCommand command, CancellationToken token = default)
        {
            if (command == null)
            {
                throw new NullReferenceException("Command cannot be null!");
            }

            // use the connection here
            command.Connection = this.DbConnection;
            command.CommandType = CommandType.StoredProcedure;
            command.Transaction = this.DbTransaction; // null if not transaction

            if (command.Connection.State != ConnectionState.Open)
            {
                await command.Connection.OpenAsync(token).ConfigureAwait(false);
            }

            var commandBehavior = this.DbTransaction != null
                ? CommandBehavior.Default
                : CommandBehavior.CloseConnection;
            
            return await command.ExecuteReaderAsync(
                commandBehavior,
                token).ConfigureAwait(false);
        }

        protected DbDataReader ExecuteProcedureCommand(DbCommand command)
        {
            if (command == null)
            {
                throw new NullReferenceException("Command cannot be null!");
            }

            // use the connection here
            command.Connection = this.DbConnection;
            command.CommandType = CommandType.StoredProcedure;

            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
            }
            
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        protected async Task<DbDataReader> ExecuteSelectCommandAsync(DbCommand command, CancellationToken token = default)
        {
            if (command == null)
            {
                throw new NullReferenceException("Command cannot be null!");
            }

            // use the connection here
            command.Connection = new SqlConnection(CONN_STRING); // nevytvaret novou instanci
            command.CommandType = CommandType.Text;

            if (command.Connection.State != ConnectionState.Open)
            {
                await command.Connection.OpenAsync(token).ConfigureAwait(false);
            }


            return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, token).ConfigureAwait(false);
        }

        protected DbDataReader ExecuteSelectCommand(DbCommand command)
        {
            if (command == null)
            {
                throw new NullReferenceException("Command cannot be null!");
            }

            // use the connection here
            command.Connection = this.DbConnection;
            command.CommandType = CommandType.Text;

            OpenConnection();
            
            return command.ExecuteReader();
        }

        /// <summary>
        /// Executes the insert command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>ID of inserted row</returns>
        protected int ExecuteInsertCommand(DbCommand command)
        {
            if (command == null)
            {
                throw new NullReferenceException("Command cannot be null!");
            }

            // use the connection here
            command.Connection = this.DbConnection;
            command.CommandType = CommandType.Text;
            command.Transaction = this.DbTransaction;

            OpenConnection();

            return (int)(command.ExecuteScalar() ?? -1);
        }

        /// <summary>
        /// Executes the adjust command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>Count of affected rows</returns>
        protected int ExecuteAdjustCommand(DbCommand command)
        {
            if (command == null)
            {
                throw new NullReferenceException("Command cannot be null!");
            }

            // use the connection here
            command.Connection = this.DbConnection;
            command.CommandType = CommandType.Text;
            command.Transaction = this.DbTransaction;

            OpenConnection();
            
            return command.ExecuteNonQuery();
        }

        protected async Task<int> ExecuteAdjustCommandAsync(DbCommand command, CancellationToken token = default)
        {
            if (command == null)
            {
                throw new NullReferenceException("Command cannot be null!");
            }

            // use the connection here
            command.Connection = this.DbConnection;
            command.CommandType = CommandType.Text;
            command.Transaction = this.DbTransaction;

            await OpenConnectionAsync(token).ConfigureAwait(false);
            
            return await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }
    }
}
