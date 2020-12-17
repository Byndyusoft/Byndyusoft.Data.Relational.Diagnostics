using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Data.Diagnostics.Payloads;

namespace Microsoft.Data.Diagnostics
{
    public abstract class DbDiagnosticObserver :
        IObserver<DiagnosticListener>,
        IObserver<KeyValuePair<string, object?>>,
        IDisposable
    {
        private readonly Dictionary<string, Action<DbDiagnosticObserver, object>> _methods =
            new Dictionary<string, Action<DbDiagnosticObserver, object>>
            {
                {
                    DbDiagnosticListener.EventNames.CommandExecuting,
                    (observer, value) => observer.OnCommandExecuting((CommandPayload) value)
                },
                {
                    DbDiagnosticListener.EventNames.CommandExecuted,
                    (observer, value) => observer.OnCommandExecuted((CommandPayload) value)
                },
                {
                    DbDiagnosticListener.EventNames.CommandExecutingError,
                    (observer, value) => observer.OnCommandExecutingError((CommandPayload) value)
                },
                {
                    DbDiagnosticListener.EventNames.ConnectionOpening,
                    (observer, value) => observer.OnConnectionOpening((ConnectionPayload) value)
                },
                {
                    DbDiagnosticListener.EventNames.ConnectionOpened,
                    (observer, value) => observer.OnConnectionOpened((ConnectionPayload) value)
                },
                {
                    DbDiagnosticListener.EventNames.ConnectionOpeningError,
                    (observer, value) => observer.OnConnectionOpeningError((ConnectionPayload) value)
                },
                {
                    DbDiagnosticListener.EventNames.ConnectionClosing,
                    (observer, value) => observer.OnConnectionClosing((ConnectionPayload) value)
                },
                {
                    DbDiagnosticListener.EventNames.ConnectionClosed,
                    (observer, value) => observer.OnConnectionClosed((ConnectionPayload) value)
                },
                {
                    DbDiagnosticListener.EventNames.ConnectionClosingError,
                    (observer, value) => observer.OnConnectionClosingError((ConnectionPayload) value)
                },
                {
                    DbDiagnosticListener.EventNames.TransactionCommitting,
                    (observer, value) => observer.OnTransactionCommitting((TransactionPayload) value)
                },
                {
                    DbDiagnosticListener.EventNames.TransactionCommitted,
                    (observer, value) => observer.OnTransactionCommitted((TransactionPayload) value)
                },
                {
                    DbDiagnosticListener.EventNames.TransactionCommittingError,
                    (observer, value) => observer.OnTransactionCommittingError((TransactionPayload) value)
                },
                {
                    DbDiagnosticListener.EventNames.TransactionRollingBack,
                    (observer, value) => observer.OnTransactionRollingBack((TransactionPayload) value)
                },
                {
                    DbDiagnosticListener.EventNames.TransactionRolledBack,
                    (observer, value) => observer.OnTransactionRolledBack((TransactionPayload) value)
                },
                {
                    DbDiagnosticListener.EventNames.TransactionRollingBackError,
                    (observer, value) => observer.OnTransactionRollingBackError((TransactionPayload) value)
                }
            };

        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private bool _disposed;
        private IDisposable? _executing;

        public void Dispose()
        {
            if (_disposed)
                return;

            Stop();
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        void IObserver<DiagnosticListener>.OnNext(DiagnosticListener diagnosticListener)
        {
            if (diagnosticListener.Name == nameof(DbDiagnosticListener))
                _subscriptions.Add(diagnosticListener.Subscribe(this));
        }

        void IObserver<DiagnosticListener>.OnCompleted()
        {
            _subscriptions.ForEach(x => x.Dispose());
            _subscriptions.Clear();
        }

        void IObserver<DiagnosticListener>.OnError(Exception error)
        {
        }

        void IObserver<KeyValuePair<string, object?>>.OnCompleted()
        {
        }

        void IObserver<KeyValuePair<string, object?>>.OnError(Exception error)
        {
        }

        void IObserver<KeyValuePair<string, object?>>.OnNext(KeyValuePair<string, object?> value)
        {
            if (OnNext(value.Key, value.Value))
                return;

            if (_methods.TryGetValue(value.Key, out var method) == false)
                return;

            method(this, value.Value!);
        }

        public void Start()
        {
            ThrowIfDisposed();

            if (_executing != null)
                throw new InvalidOperationException("DbDiagnosticObserver is already started");

            OnStart();
            _executing = DiagnosticListener.AllListeners.Subscribe(this);
        }

        public void Stop()
        {
            ThrowIfDisposed();

            if (_executing != null)
            {
                OnStop();
                _executing.Dispose();
                _executing = null;
            }
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnStop()
        {
        }

        protected virtual bool OnNext(string eventName, object? payload)
        {
            return string.IsNullOrWhiteSpace(eventName) == false && payload != null;
        }

        protected virtual void OnCommandExecuting(CommandPayload payload)
        {
        }

        protected virtual void OnCommandExecuted(CommandPayload payload)
        {
        }

        protected virtual void OnCommandExecutingError(CommandPayload payload)
        {
        }

        protected virtual void OnConnectionOpening(ConnectionPayload payload)
        {
        }

        protected virtual void OnConnectionOpened(ConnectionPayload payload)
        {
        }

        protected virtual void OnConnectionOpeningError(ConnectionPayload payload)
        {
        }

        protected virtual void OnConnectionClosing(ConnectionPayload payload)
        {
        }

        protected virtual void OnConnectionClosed(ConnectionPayload payload)
        {
        }

        protected virtual void OnConnectionClosingError(ConnectionPayload payload)
        {
        }

        protected virtual void OnTransactionCommitting(TransactionPayload payload)
        {
        }

        protected virtual void OnTransactionCommitted(TransactionPayload payload)
        {
        }

        protected virtual void OnTransactionCommittingError(TransactionPayload payload)
        {
        }

        protected virtual void OnTransactionRollingBack(TransactionPayload payload)
        {
        }

        protected virtual void OnTransactionRolledBack(TransactionPayload payload)
        {
        }

        protected virtual void OnTransactionRollingBackError(TransactionPayload payload)
        {
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DbDiagnosticObserver));
        }
    }
}