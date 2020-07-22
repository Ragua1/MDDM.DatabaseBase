using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MDDM.DatabaseBase.Interfaces
{
    public interface IDbBase
    {
        void OpenConnection();
        void CloseConnection();

        void BeginTransaction(IsolationLevel? isolationLevel = null, string transactionName = default);
        void CommitTransaction();
        void RollbackTransaction();
    }
}
