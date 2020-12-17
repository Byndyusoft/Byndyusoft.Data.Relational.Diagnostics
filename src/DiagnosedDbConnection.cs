using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using IsolationLevel = System.Data.IsolationLevel;

namespace Microsoft.Data.Diagnostics
{
    public class DiagnosedDbConnection : DbConnection
    {
        private static readonly DbDiagnosticSource DiagnosticSourceListener = new DbDiagnosticSource();

        public DiagnosedDbConnection(DbConnection inner)
        {
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public override int ConnectionTimeout => Inner.ConnectionTimeout;

        internal DbConnection Inner { get; }

        public override ISite Site
        {
            get => Inner.Site;
            set => Inner.Site = value;
        }

        public override string ConnectionString
        {
            get => Inner.ConnectionString;
            set => Inner.ConnectionString = value;
        }

        public override string Database => Inner.Database;

        public override ConnectionState State => Inner.State;

        public override string? DataSource => Inner.DataSource;

        public override string? ServerVersion => Inner.ServerVersion;

        public override void Close()
        {
            var operationId = DiagnosticSourceListener.WriteConnectionCloseBefore(this);
            try
            {
                Inner.Close();
                DiagnosticSourceListener.WriteConnectionCloseAfter(operationId, this);
            }
            catch (Exception ex)
            {
                DiagnosticSourceListener.WriteConnectionCloseError(operationId, this, ex);
                throw;
            }
        }

        public override async Task CloseAsync()
        {
            var operationId = DiagnosticSourceListener.WriteConnectionCloseBefore(this);
            try
            {
                await Inner.CloseAsync().ConfigureAwait(false);
                DiagnosticSourceListener.WriteConnectionCloseAfter(operationId, this);
            }
            catch (Exception ex)
            {
                DiagnosticSourceListener.WriteConnectionCloseError(operationId, this, ex);
                throw;
            }
        }

        public override void Open()
        {
            var operationId = DiagnosticSourceListener.WriteConnectionOpenBefore(this);
            try
            {
                Inner.Open();
                DiagnosticSourceListener.WriteConnectionOpenAfter(operationId, this);
            }
            catch (Exception ex)
            {
                DiagnosticSourceListener.WriteConnectionOpenError(operationId, this, ex);
                throw;
            }
        }

        public override async Task OpenAsync(CancellationToken cancellationToken)
        {
            var operationId = DiagnosticSourceListener.WriteConnectionOpenBefore(this);
            try
            {
                await Inner.OpenAsync(cancellationToken).ConfigureAwait(false);
                DiagnosticSourceListener.WriteConnectionOpenAfter(operationId, this);
            }
            catch (Exception ex)
            {
                DiagnosticSourceListener.WriteConnectionOpenError(operationId, this, ex);
                throw;
            }
        }

        public override void EnlistTransaction(Transaction transaction)
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

        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            return Inner.GetSchema(collectionName, restrictionValues);
        }

        public override object? InitializeLifetimeService()
        {
            return Inner.InitializeLifetimeService();
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

        public override event StateChangeEventHandler StateChange
        {
            add => Inner.StateChange += value;
            remove => Inner.StateChange -= value;
        }

        protected override DbCommand CreateDbCommand()
        {
            return Inner.CreateCommand().AddDiagnosting();
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return Inner.BeginTransaction(isolationLevel).AddDiagnosting();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Inner.Dispose();
        }

        protected override async ValueTask<DbTransaction> BeginDbTransactionAsync(IsolationLevel isolationLevel,
            CancellationToken cancellationToken)
        {
            var transaction = await Inner.BeginTransactionAsync(isolationLevel, cancellationToken)
                .ConfigureAwait(false);
            return transaction.AddDiagnosting();
        }
    }
}