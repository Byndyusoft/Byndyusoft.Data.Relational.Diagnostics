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

        public override object? InitializeLifetimeService()
        {
            return Inner.InitializeLifetimeService();
        }

        public override void Commit()
        {
            var operationId = DiagnosticSourceListener.WriteTransactionCommitBefore(IsolationLevel, Connection, this);
            Exception? e = null;
            try
            {
                Inner.Commit();
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                    DiagnosticSourceListener.WriteTransactionCommitError(operationId, IsolationLevel, Connection, this,
                        e);
                else
                    DiagnosticSourceListener.WriteTransactionCommitAfter(operationId, IsolationLevel, Connection, this);
            }
        }

        public override void Rollback()
        {
            var operationId = DiagnosticSourceListener.WriteTransactionRollbackBefore(IsolationLevel, Connection, this);
            Exception? e = null;
            try
            {
                Inner.Rollback();
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                    DiagnosticSourceListener.WriteTransactionRollbackError(operationId, IsolationLevel, Connection,
                        this, e);
                else
                    DiagnosticSourceListener.WriteTransactionRollbackAfter(operationId, IsolationLevel, Connection,
                        this);
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

        public override async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            var connection = Connection;
            var isolationLevel = IsolationLevel;
            var operationId = DiagnosticSourceListener.WriteTransactionCommitBefore(isolationLevel, connection, this);
            Exception? e = null;
            try
            {
                await Inner.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                    DiagnosticSourceListener.WriteTransactionCommitError(operationId, isolationLevel, connection, this, e);
                else
                    DiagnosticSourceListener.WriteTransactionCommitAfter(operationId, isolationLevel, connection, this);
            }
        }

        public override ValueTask DisposeAsync()
        {
            return Inner.DisposeAsync();
        }

        public override async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            var operationId = DiagnosticSourceListener.WriteTransactionRollbackBefore(IsolationLevel, Connection, this);
            Exception? e = null;
            try
            {
                await Inner.RollbackAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                    DiagnosticSourceListener.WriteTransactionRollbackError(operationId, IsolationLevel, Connection, this, e);
                else
                    DiagnosticSourceListener.WriteTransactionRollbackAfter(operationId, IsolationLevel, Connection, this);
            }
        }
    }
}