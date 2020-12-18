using System;
using System.Data;
using System.Data.Common;

#if ADO_NET_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Microsoft.Data.Diagnostics
{
    internal class DiagnosedDbTransaction : DbTransaction
    {
        private static readonly DbDiagnosticListener DiagnosticListenerListener = new DbDiagnosticListener();

        public DiagnosedDbTransaction(DbTransaction transaction)
        {
            Inner = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        public DbTransaction Inner { get; }

        public override IsolationLevel IsolationLevel => Inner.IsolationLevel;

        protected override DbConnection DbConnection => Inner.Connection.Unwrap();

#if NETCOREAPP
        public override object InitializeLifetimeService()
#else
        public override object? InitializeLifetimeService()
#endif
        {
            return Inner.InitializeLifetimeService();
        }

        public override void Commit()
        {
            var connection = Connection;
            var isolationLevel = IsolationLevel;
            var operationId = DiagnosticListenerListener.OnTransactionCommitting(isolationLevel, connection, this);
            try
            {
                Inner.Commit();
                DiagnosticListenerListener.OnTransactionCommitted(operationId, isolationLevel, connection, this);
            }
            catch (Exception ex)
            {
                DiagnosticListenerListener.OnTransactionCommittingError(operationId, isolationLevel, connection, this,
                    ex);
                throw;
            }
        }

        public override void Rollback()
        {
            var connection = Connection;
            var isolationLevel = IsolationLevel;
            var operationId = DiagnosticListenerListener.OnTransactionRollingBack(isolationLevel, connection, this);
            try
            {
                Inner.Rollback();
                DiagnosticListenerListener.OnTransactionRolledBack(operationId, isolationLevel, connection, this);
            }
            catch (Exception ex)
            {
                DiagnosticListenerListener.OnTransactionRollingBackError(operationId, isolationLevel, connection, this,
                    ex);
                throw;
            }
        }

        public override string? ToString()
        {
            return Inner.ToString();
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || Inner.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Inner.GetHashCode();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Inner.Dispose();
        }

#if ADO_NET_ASYNC
        public override async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            var connection = Connection;
            var isolationLevel = IsolationLevel;
            var operationId = DiagnosticListenerListener.OnTransactionCommitting(isolationLevel, connection, this);
            try
            {
                await Inner.CommitAsync(cancellationToken).ConfigureAwait(false);
                DiagnosticListenerListener.OnTransactionCommitted(operationId, isolationLevel, connection, this);
            }
            catch (Exception ex)
            {
                DiagnosticListenerListener.OnTransactionCommittingError(operationId, isolationLevel, connection, this, ex);
                throw;
            }
        }

        public override ValueTask DisposeAsync()
        {
            return Inner.DisposeAsync();
        }

        public override async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            var operationId = DiagnosticListenerListener.OnTransactionRollingBack(IsolationLevel, Connection, this);
            try
            {
                await Inner.RollbackAsync(cancellationToken).ConfigureAwait(false);
                DiagnosticListenerListener.OnTransactionRolledBack(operationId, IsolationLevel, Connection, this);
            }
            catch (Exception ex)
            {
                DiagnosticListenerListener.OnTransactionRollingBackError(operationId, IsolationLevel, Connection, this,
                    ex);
                throw;
            }
        }

#endif
    }
}