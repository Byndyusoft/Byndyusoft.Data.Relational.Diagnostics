using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using IsolationLevel = System.Data.IsolationLevel;

namespace Microsoft.Data.Diagnostics
{
    internal class DiagnosedDbConnection : DbConnection
    {
        private static readonly DbDiagnosticListener DiagnosticListenerListener = new DbDiagnosticListener();

        public DiagnosedDbConnection(DbConnection inner)
        {
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public override int ConnectionTimeout => Inner.ConnectionTimeout;

        internal DbConnection Inner { get; }

        public override string? ConnectionString
        {
            get => Inner.ConnectionString;
            set => Inner.ConnectionString = value;
        }

        public override string Database => Inner.Database;

        public override ConnectionState State => Inner.State;

        public override string DataSource => Inner.DataSource!;

        public override string ServerVersion => Inner.ServerVersion!;

        public override void Close()
        {
            var operationId = DiagnosticListenerListener.OnConnectionClosing(this);
            try
            {
                Inner.Close();
                DiagnosticListenerListener.OnConnectionClosed(operationId, this);
            }
            catch (Exception ex)
            {
                DiagnosticListenerListener.OmConnectionClosingError(operationId, this, ex);
                throw;
            }
        }

        public override void Open()
        {
            var operationId = DiagnosticListenerListener.OnConnectionOpening(this);
            try
            {
                Inner.Open();
                DiagnosticListenerListener.OnConnectionOpened(operationId, this);
            }
            catch (Exception ex)
            {
                DiagnosticListenerListener.OnConnectionOpeningError(operationId, this, ex);
                throw;
            }
        }

        public override async Task OpenAsync(CancellationToken cancellationToken)
        {
            var operationId = DiagnosticListenerListener.OnConnectionOpening(this);
            try
            {
                await Inner.OpenAsync(cancellationToken).ConfigureAwait(false);
                DiagnosticListenerListener.OnConnectionOpened(operationId, this);
            }
            catch (Exception ex)
            {
                DiagnosticListenerListener.OnConnectionOpeningError(operationId, this, ex);
                throw;
            }
        }

        public override void EnlistTransaction(Transaction? transaction)
        {
            Inner.EnlistTransaction(transaction);
        }

        public override DataTable GetSchema()
        {
            return Inner.GetSchema();
        }

        public override DataTable GetSchema(string collectionName)
        {
            return Inner.GetSchema(collectionName);
        }

        public override DataTable GetSchema(string collectionName, string?[] restrictionValues)
        {
            return Inner.GetSchema(collectionName, restrictionValues);
        }

        public override string ToString()
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

        public override void ChangeDatabase(string databaseName)
        {
            Inner.ChangeDatabase(databaseName);
        }

        public override event StateChangeEventHandler? StateChange
        {
            add => Inner.StateChange += value;
            remove => Inner.StateChange -= value;
        }

        protected override DbCommand CreateDbCommand()
        {
            return Inner.CreateCommand()?.AddDiagnosting()!;
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return Inner.BeginTransaction(isolationLevel)?.AddDiagnosting()!;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Inner.Dispose();
        }

#if ADO_NET_ASYNC

        public override Task ChangeDatabaseAsync(string databaseName, CancellationToken cancellationToken = default)
        {
            return Inner.ChangeDatabaseAsync(databaseName, cancellationToken);
        }

        public override ValueTask DisposeAsync()
        {
            return Inner.DisposeAsync();
        }

        protected override async ValueTask<DbTransaction> BeginDbTransactionAsync(IsolationLevel isolationLevel,
            CancellationToken cancellationToken)
        {
            var transaction = await Inner.BeginTransactionAsync(isolationLevel, cancellationToken)
                .ConfigureAwait(false);
            return transaction?.AddDiagnosting()!;
        }
        
        public override async Task CloseAsync()
        {
            var operationId = DiagnosticListenerListener.OnConnectionClosing(this);
            try
            {
                await Inner.CloseAsync().ConfigureAwait(false);
                DiagnosticListenerListener.OnConnectionClosed(operationId, this);
            }
            catch (Exception ex)
            {
                DiagnosticListenerListener.OmConnectionClosingError(operationId, this, ex);
                throw;
            }
        }

#endif

#if NET5_0

        public override Task<DataTable> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            return Inner.GetSchemaAsync(cancellationToken);
        }

        public override Task<DataTable> GetSchemaAsync(string collectionName, CancellationToken cancellationToken = default)
        {
            return Inner.GetSchemaAsync(collectionName, cancellationToken);
        }

        public override Task<DataTable> GetSchemaAsync(string collectionName, string?[] restrictionValues, CancellationToken cancellationToken = default)
        {
            return Inner.GetSchemaAsync(collectionName, restrictionValues, cancellationToken);
        }

#endif

    }
}