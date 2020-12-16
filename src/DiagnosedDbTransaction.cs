using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Diagnostics
{
    internal class DiagnosedDbTransaction : DbTransaction
    {
        private static long _id;
        private static readonly DbDiagnosticListener DiagnosticListener = DbDiagnosticListener.Instance;

        private readonly DbTransaction _inner;

        public DiagnosedDbTransaction(DbTransaction transaction)
        {
            _inner = transaction ?? throw new ArgumentNullException(nameof(transaction));
            TransactionId = Interlocked.Increment(ref _id);
        }

        public long TransactionId { get; }

        public override IsolationLevel IsolationLevel => _inner.IsolationLevel;

        protected override DbConnection DbConnection => _inner.Connection.AddDiagnosting();

        public override object? InitializeLifetimeService()
        {
            return _inner.InitializeLifetimeService();
        }

        public override void Commit()
        {
            var operationId = DiagnosticListener.WriteTransactionCommitBefore(IsolationLevel, Connection, this);
            Exception? e = null;
            try
            {
                _inner.Commit();
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                    DiagnosticListener.WriteTransactionCommitError(operationId, IsolationLevel, Connection, this,
                        e);
                else
                    DiagnosticListener.WriteTransactionCommitAfter(operationId, IsolationLevel, Connection, this);
            }
        }

        public override void Rollback()
        {
            var operationId = DiagnosticListener.WriteTransactionRollbackBefore(IsolationLevel, Connection, this);
            Exception? e = null;
            try
            {
                _inner.Rollback();
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                    DiagnosticListener.WriteTransactionRollbackError(operationId, IsolationLevel, Connection,
                        this, e);
                else
                    DiagnosticListener.WriteTransactionRollbackAfter(operationId, IsolationLevel, Connection,
                        this);
            }
        }

        public override string? ToString()
        {
            return _inner.ToString();
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || _inner.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _inner.GetHashCode();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _inner.Dispose();
        }

        public override async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            var operationId = DiagnosticListener.WriteTransactionCommitBefore(IsolationLevel, Connection, this);
            Exception? e = null;
            try
            {
                await _inner.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                    DiagnosticListener.WriteTransactionCommitError(operationId, IsolationLevel, Connection, this, e);
                else
                    DiagnosticListener.WriteTransactionCommitAfter(operationId, IsolationLevel, Connection, this);
            }
        }

        public override ValueTask DisposeAsync()
        {
            return _inner.DisposeAsync();
        }

        public override async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            var operationId = DiagnosticListener.WriteTransactionRollbackBefore(IsolationLevel, Connection, this);
            Exception? e = null;
            try
            {
                await _inner.RollbackAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                    DiagnosticListener.WriteTransactionRollbackError(operationId, IsolationLevel, Connection, this, e);
                else
                    DiagnosticListener.WriteTransactionRollbackAfter(operationId, IsolationLevel, Connection, this);
            }
        }
    }
}