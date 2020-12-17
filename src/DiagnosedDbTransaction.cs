using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Diagnostics
{
    internal class DiagnosedDbTransaction : DbTransaction
    {
        private static readonly DbDiagnosticSource DiagnosticSourceListener = new DbDiagnosticSource();

        public DiagnosedDbTransaction(DbTransaction transaction)
        {
            Inner = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        public DbTransaction Inner { get; }

        public override IsolationLevel IsolationLevel => Inner.IsolationLevel;

        protected override DbConnection DbConnection => Inner.Connection.GetUnderlying();

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
            var operationId = DiagnosticSourceListener.WriteTransactionCommitBefore(IsolationLevel, Connection, this);
            try
            {
                Inner.Commit();
                DiagnosticSourceListener.WriteTransactionCommitAfter(operationId, IsolationLevel, Connection, this);
            }
            catch (Exception ex)
            {
                DiagnosticSourceListener.WriteTransactionCommitError(operationId, IsolationLevel, Connection, this, ex);
                throw;
            }
        }

        public override void Rollback()
        {
            var operationId = DiagnosticSourceListener.WriteTransactionRollbackBefore(IsolationLevel, Connection, this);
            try
            {
                Inner.Rollback();
                DiagnosticSourceListener.WriteTransactionRollbackAfter(operationId, IsolationLevel, Connection, this);
            }
            catch (Exception ex)
            {
                DiagnosticSourceListener.WriteTransactionRollbackError(operationId, IsolationLevel, Connection, this,
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
            var operationId = DiagnosticSourceListener.WriteTransactionCommitBefore(isolationLevel, connection, this);
            try
            {
                await Inner.CommitAsync(cancellationToken).ConfigureAwait(false);
                DiagnosticSourceListener.WriteTransactionCommitAfter(operationId, isolationLevel, connection, this);
            }
            catch (Exception ex)
            {
                DiagnosticSourceListener.WriteTransactionCommitError(operationId, isolationLevel, connection, this, ex);
                throw;
            }
        }

        public override ValueTask DisposeAsync()
        {
            return Inner.DisposeAsync();
        }

        public override async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            var operationId = DiagnosticSourceListener.WriteTransactionRollbackBefore(IsolationLevel, Connection, this);
            try
            {
                await Inner.RollbackAsync(cancellationToken).ConfigureAwait(false);
                DiagnosticSourceListener.WriteTransactionRollbackAfter(operationId, IsolationLevel, Connection, this);
            }
            catch (Exception ex)
            {
                DiagnosticSourceListener.WriteTransactionRollbackError(operationId, IsolationLevel, Connection, this,
                    ex);
                throw;
            }
        }

#endif
    }
}