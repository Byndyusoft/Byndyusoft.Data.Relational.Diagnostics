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
        private static readonly DbDiagnosticListener DiagnosticListenerListener = new DbDiagnosticListener();

        public DiagnosedDbCommand(DbCommand inner)
        {
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public override string CommandText
        {
            get => Inner.CommandText;
            set => Inner.CommandText = value;
        }

        public override int CommandTimeout
        {
            get => Inner.CommandTimeout;
            set => Inner.CommandTimeout = value;
        }

        public override CommandType CommandType
        {
            get => Inner.CommandType;
            set => Inner.CommandType = value;
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get => Inner.UpdatedRowSource;
            set => Inner.UpdatedRowSource = value;
        }

        protected override DbConnection DbConnection
        {
            get => Inner.Connection.Unwrap();
            set => Inner.Connection = value.Unwrap();
        }

        protected override DbParameterCollection DbParameterCollection => Inner.Parameters;

        protected override DbTransaction? DbTransaction
        {
            get => Inner.Transaction?.Unwrap();
            set => Inner.Transaction = value?.Unwrap();
        }

        public override bool DesignTimeVisible
        {
            get => Inner.DesignTimeVisible;
            set => Inner.DesignTimeVisible = value;
        }

        public override ISite Site
        {
            get => Inner.Site;
            set => Inner.Site = value;
        }

        internal DbCommand Inner { get; }

        public override void Cancel()
        {
            Inner.Cancel();
        }

        public override void Prepare()
        {
            Inner.Prepare();
        }

        protected override DbParameter CreateDbParameter()
        {
            return Inner.CreateParameter();
        }

        public override string ToString()
        {
            return Inner.ToString();
        }

#if NETCOREAPP
        public override object InitializeLifetimeService()
#else
        public override object? InitializeLifetimeService()
#endif
        {
            return Inner.InitializeLifetimeService();
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

        protected override object GetService(Type service)
        {
            return Inner.Site.GetService(service);
        }

        public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            var operationId = DiagnosticListenerListener.OnCommandExecuting(this, Transaction);
            try
            {
                var result = await Inner.ExecuteNonQueryAsync(cancellationToken)
                    .ConfigureAwait(false);
                DiagnosticListenerListener.OnCommandExecuted(operationId, this, Transaction);
                return result;
            }
            catch (Exception ex)
            {
                DiagnosticListenerListener.OnCommandError(operationId, this, Transaction, ex);
                throw;
            }
        }

        public override async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            var operationId = DiagnosticListenerListener.OnCommandExecuting(this, Transaction);
            try
            {
                var result = await Inner.ExecuteScalarAsync(cancellationToken)
                    .ConfigureAwait(false);
                DiagnosticListenerListener.OnCommandExecuted(operationId, this, Transaction);
                return result;
            }
            catch (Exception ex)
            {
                DiagnosticListenerListener.OnCommandError(operationId, this, Transaction, ex);
                throw;
            }
        }

        public override int ExecuteNonQuery()
        {
            var operationId = DiagnosticListenerListener.OnCommandExecuting(this, Transaction);
            try
            {
                var result = Inner.ExecuteNonQuery();
                DiagnosticListenerListener.OnCommandExecuted(operationId, this, Transaction);
                return result;
            }
            catch (Exception ex)
            {
                DiagnosticListenerListener.OnCommandError(operationId, this, Transaction, ex);
                throw;
            }
        }

        public override object ExecuteScalar()
        {
            var operationId = DiagnosticListenerListener.OnCommandExecuting(this, Transaction);
            try
            {
                var result = Inner.ExecuteScalar();
                DiagnosticListenerListener.OnCommandExecuted(operationId, this, Transaction);
                return result;
            }
            catch (Exception ex)
            {
                DiagnosticListenerListener.OnCommandError(operationId, this, Transaction, ex);
                throw;
            }
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            var operationId = DiagnosticListenerListener.OnCommandExecuting(this, Transaction);
            try
            {
                var result = Inner.ExecuteReader(behavior);
                DiagnosticListenerListener.OnCommandExecuted(operationId, this, Transaction);
                return result;
            }
            catch (Exception ex)
            {
                DiagnosticListenerListener.OnCommandError(operationId, this, Transaction, ex);
                throw;
            }
        }

        protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior,
            CancellationToken cancellationToken)
        {
            var operationId = DiagnosticListenerListener.OnCommandExecuting(this, Transaction);
            try
            {
                var result = await Inner.ExecuteReaderAsync(behavior, cancellationToken)
                    .ConfigureAwait(false);
                DiagnosticListenerListener.OnCommandExecuted(operationId, this, Transaction);
                return result;
            }
            catch (Exception ex)
            {
                DiagnosticListenerListener.OnCommandError(operationId, this, Transaction, ex);
                throw;
            }
        }

#if ADO_NET_ASYNC

        public override ValueTask DisposeAsync()
        {
            return Inner.DisposeAsync();
        }

        public override Task PrepareAsync(CancellationToken cancellationToken = default)
        {
            return Inner.PrepareAsync(cancellationToken);
        }

#endif 


    }
}