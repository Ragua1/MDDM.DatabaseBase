using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace MDDM.DatabaseBase.Interfaces
{
    public interface IDbBase
    {
        void OpenConnection();
        Task OpenConnectionAsync(CancellationToken token = default);
        void CloseConnection();
        Task CloseConnectionAsync();
        void BeginTransaction(IsolationLevel? isolationLevel = null);
        Task BeginTransactionAsync(IsolationLevel? isolationLevel = null, CancellationToken token = default);
        void CommitTransaction();
        Task CommitTransactionAsync(CancellationToken token = default);
        void RollbackTransaction();
        Task RollbackTransactionAsync(CancellationToken token = default);
    }
}