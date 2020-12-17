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

        private readonly DbConnection _inner;

        public DiagnosedDbConnection(DbConnection inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public override int ConnectionTimeout => _inner.ConnectionTimeout;

        internal DbConnection Inner => _inner;

        public override ISite Site
        {
            get => _inner.Site;
            set => _inner.Site = value;
        }

        public override string ConnectionString
        {
            get => _inner.ConnectionString;
            set => _inner.ConnectionString = value;
        }

        public override string Database => _inner.Database;

        public override ConnectionState State => _inner.State;

        public override string? DataSource => _inner.DataSource;

        public override string? ServerVersion => _inner.ServerVersion;

        public override void Close()
        {
            var operationId = DiagnosticSourceListener.WriteConnectionCloseBefore(this);
            Exception? e = null;
            try
            {
                _inner.Close();
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                    DiagnosticSourceListener.WriteConnectionCloseError(operationId, this, e);
                else
                    DiagnosticSourceListener.WriteConnectionCloseAfter(operationId, this);
            }
        }

        public override void Open()
        {
            var operationId = DiagnosticSourceListener.WriteConnectionOpenBefore(this);
            Exception? e = null;
            try
            {
                _inner.Open();
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                    DiagnosticSourceListener.WriteConnectionOpenError(operationId, this, e);
                else
                    DiagnosticSourceListener.WriteConnectionOpenAfter(operationId, this);
            }
        }

        public override async Task OpenAsync(CancellationToken cancellationToken)
        {
            var operationId = DiagnosticSourceListener.WriteConnectionOpenBefore(this);
            Exception? e = null;
            try
            {
                await _inner.OpenAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                    DiagnosticSourceListener.WriteConnectionOpenError(operationId, this, e);
                else
                    DiagnosticSourceListener.WriteConnectionOpenAfter(operationId, this);
            }
        }


        public override void EnlistTransaction(Transaction transaction)
        {
            _inner.EnlistTransaction(transaction);
        }

        public override DataTable GetSchema()
        {
            return _inner.GetSchema();
        }

        public override DataTable GetSchema(string collectionName)
        {
            return _inner.GetSchema(collectionName);
        }

        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            return _inner.GetSchema(collectionName, restrictionValues);
        }

        public override object? InitializeLifetimeService()
        {
            return _inner.InitializeLifetimeService();
        }

        public override string ToString()
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

        public override void ChangeDatabase(string databaseName)
        {
            _inner.ChangeDatabase(databaseName);
        }

        public override event StateChangeEventHandler StateChange
        {
            add => _inner.StateChange += value;
            remove => _inner.StateChange -= value;
        }

        protected override DbCommand CreateDbCommand()
        {
            return _inner.CreateCommand().AddDiagnosting();
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return _inner.BeginTransaction(isolationLevel).AddDiagnosting();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _inner.Dispose();
        }

        protected override async ValueTask<DbTransaction> BeginDbTransactionAsync(IsolationLevel isolationLevel,
            CancellationToken cancellationToken)
        {
            var transaction = await _inner.BeginTransactionAsync(isolationLevel, cancellationToken)
                .ConfigureAwait(false);
            return transaction.AddDiagnosting();
        }

        public override async Task CloseAsync()
        {
            var operationId = DiagnosticSourceListener.WriteConnectionCloseBefore(this);
            Exception? e = null;
            try
            {
                await _inner.CloseAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                    DiagnosticSourceListener.WriteConnectionCloseError(operationId, this, e);
                else
                    DiagnosticSourceListener.WriteConnectionCloseAfter(operationId, this);
            }
        }
    }
}