using System.Data;
using MDDM.DatabaseBase.DataClasses;
using Oracle.ManagedDataAccess.Client;

namespace MDDM.DatabaseBase.Oracle.DataClasses
{
    public class OracleBase : DbBase
    {
        private OracleConnection OracleConnection
        {
            get => (OracleConnection)base.DbConnection;
            set => base.DbConnection = value;
        }
        private OracleTransaction OracleTransaction
        {
            get => (OracleTransaction)base.DbTransaction;
            set => base.DbTransaction = value;
        }

        protected OracleBase(string connectionString, IsolationLevel defaultIsolationLevel = IsolationLevel.Unspecified) : base(connectionString, defaultIsolationLevel)
        {
            OracleConnection = (OracleConnection)(base.DbConnection ??= new OracleConnection(CONN_STRING));
        }
    }
}