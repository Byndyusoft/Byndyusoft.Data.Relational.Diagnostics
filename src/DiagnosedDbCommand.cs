using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Diagnostics
{
    internal class DiagnosedDbCommand : DbCommand
    {
        private static readonly DbDiagnosticSource DiagnosticSourceListener = new DbDiagnosticSource();

        private readonly DbCommand _inner;

        public DiagnosedDbCommand(DbCommand inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public override string CommandText
        {
            get => _inner.CommandText;
            set => _inner.CommandText = value;
        }

        public override int CommandTimeout
        {
            get => _inner.CommandTimeout;
            set => _inner.CommandTimeout = value;
        }

        public override CommandType CommandType
        {
            get => _inner.CommandType;
            set => _inner.CommandType = value;
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get => _inner.UpdatedRowSource;
            set => _inner.UpdatedRowSource = value;
        }

        protected override DbConnection DbConnection
        {
            get => _inner.Connection.GetUnderlying();
            set => _inner.Connection = value.GetUnderlying();
        }

        protected override DbParameterCollection DbParameterCollection => _inner.Parameters;

        protected override DbTransaction? DbTransaction
        {
            get => _inner.Transaction.GetUnderlying();
            set => _inner.Transaction = value?.GetUnderlying();
        }

        public override bool DesignTimeVisible
        {
            get => _inner.DesignTimeVisible;
            set => _inner.DesignTimeVisible = value;
        }

        public override ISite Site
        {
            get => _inner.Site;
            set => _inner.Site = value;
        }

        public override void Cancel()
        {
            _inner.Cancel();
        }

        public override void Prepare()
        {
            _inner.Prepare();
        }

        protected override DbParameter CreateDbParameter()
        {
            return _inner.CreateParameter();
        }

        public override string ToString()
        {
            return _inner.ToString();
        }

#if NETCOREAPP
        public override object InitializeLifetimeService()
#else
        public override object? InitializeLifetimeService()
#endif
        {
            return _inner.InitializeLifetimeService();
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

        protected override object GetService(Type service)
        {
            return _inner.Site.GetService(service);
        }

        public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            var operationId = DiagnosticSourceListener.WriteCommandBefore(this, Transaction);
            try
            {
                var result = await _inner.ExecuteNonQueryAsync(cancellationToken)
                    .ConfigureAwait(false);
                DiagnosticSourceListener.WriteCommandAfter(operationId, this, Transaction);
                return result;
            }
            catch (Exception ex)
            {
                DiagnosticSourceListener.WriteCommandError(operationId, this, Transaction, ex);
                throw;
            }
        }

        public override async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            var operationId = DiagnosticSourceListener.WriteCommandBefore(this, Transaction);
            try
            {
                var result = await _inner.ExecuteScalarAsync(cancellationToken)
                    .ConfigureAwait(false);
                DiagnosticSourceListener.WriteCommandAfter(operationId, this, Transaction);
                return result;
            }
            catch (Exception ex)
            {
                DiagnosticSourceListener.WriteCommandError(operationId, this, Transaction, ex);
                throw;
            }
        }

        public override int ExecuteNonQuery()
        {
            var operationId = DiagnosticSourceListener.WriteCommandBefore(this, Transaction);
            try
            {
                var result = _inner.ExecuteNonQuery();
                DiagnosticSourceListener.WriteCommandAfter(operationId, this, Transaction);
                return result;
            }
            catch (Exception ex)
            {
                DiagnosticSourceListener.WriteCommandError(operationId, this, Transaction, ex);
                throw;
            }
        }

        public override object ExecuteScalar()
        {
            var operationId = DiagnosticSourceListener.WriteCommandBefore(this, Transaction);
            try
            {
                var result = _inner.ExecuteScalar();
                DiagnosticSourceListener.WriteCommandAfter(operationId, this, Transaction);
                return result;
            }
            catch (Exception ex)
            {
                DiagnosticSourceListener.WriteCommandError(operationId, this, Transaction, ex);
                throw;
            }
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            var operationId = DiagnosticSourceListener.WriteCommandBefore(this, Transaction);
            try
            {
                var result = _inner.ExecuteReader(behavior);
                DiagnosticSourceListener.WriteCommandAfter(operationId, this, Transaction);
                return result;
            }
            catch (Exception ex)
            {
                DiagnosticSourceListener.WriteCommandError(operationId, this, Transaction, ex);
                throw;
            }
        }

        protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior,
            CancellationToken cancellationToken)
        {
            var operationId = DiagnosticSourceListener.WriteCommandBefore(this, Transaction);
            try
            {
                var result = await _inner.ExecuteReaderAsync(behavior, cancellationToken)
                    .ConfigureAwait(false);
                DiagnosticSourceListener.WriteCommandAfter(operationId, this, Transaction);
                return result;
            }
            catch (Exception ex)
            {
                DiagnosticSourceListener.WriteCommandError(operationId, this, Transaction, ex);
                throw;
            }
        }

#if ADO_NET_ASYNC

        public override ValueTask DisposeAsync()
        {
            return _inner.DisposeAsync();
        }

        public override Task PrepareAsync(CancellationToken cancellationToken = default)
        {
            return _inner.PrepareAsync(cancellationToken);
        }

#endif 


    }
}