using System.Data;
using System.Data.SqlClient;

namespace MDDM.DatabaseBase.DataClasses
{
    public class SqlBase : DbBase
    {
        private SqlConnection SqlConnection
        {
            get => (SqlConnection) base.DbConnection;
            set => base.DbConnection = value;
        }
        private SqlTransaction SqlTransaction
        {
            get => (SqlTransaction) base.DbTransaction;
            set => base.DbTransaction = value;
        }

        protected SqlBase(string connectionString, IsolationLevel defaultIsolationLevel = IsolationLevel.Unspecified) : base(connectionString, defaultIsolationLevel)
        {
            SqlConnection = (SqlConnection)(base.DbConnection ??= new SqlConnection(CONN_STRING));
        }
    }
}