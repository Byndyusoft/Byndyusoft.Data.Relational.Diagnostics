using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Data.Diagnostics.Payloads;

namespace Microsoft.Data.Diagnostics
{
    public partial class DbDiagnosticListener : DiagnosticListener
    {
        public DbDiagnosticListener() : base(nameof(DbDiagnosticListener))
        {
        }

        internal Guid OnCommandExecuting(DbCommand command,
            DbTransaction? transaction, [CallerMemberName] string operation = "")
        {
            if (!IsEnabled(EventNames.CommandExecuting)) return Guid.Empty;

            var operationId = Guid.NewGuid();
            Write(
                EventNames.CommandExecuting,
                new CommandPayload
                {
                    OperationId = operationId,
                    Operation = operation,
                    ConnectionId = command.Connection.GetGuid(),
                    Command = command,
                    TransactionId = transaction?.GetId(),
                    Timestamp = Stopwatch.GetTimestamp()
                });

            return operationId;
        }

        internal void OnCommandExecuted(Guid operationId, DbCommand command,
            DbTransaction? transaction, [CallerMemberName] string operation = "")
        {
            if (IsEnabled(EventNames.CommandExecuted))
                Write(
                    EventNames.CommandExecuted,
                    new CommandPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        ConnectionId = command.Connection.GetGuid(),
                        Command = command,
                        TransactionId = transaction?.GetId(),
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal void OnCommandError(Guid operationId, DbCommand command,
            DbTransaction? transaction, Exception ex, [CallerMemberName] string operation = "")
        {
            if (IsEnabled(EventNames.CommandExecutingError))
                Write(
                    EventNames.CommandExecutingError,
                    new CommandPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        ConnectionId = command.Connection.GetGuid(),
                        Command = command,
                        TransactionId = transaction?.GetId(),
                        Exception = ex,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal Guid OnConnectionOpening(DbConnection connection,
            [CallerMemberName] string operation = "")
        {
            if (!IsEnabled(EventNames.ConnectionOpening)) return Guid.Empty;

            var operationId = Guid.NewGuid();
            Write(
                EventNames.ConnectionOpening,
                new ConnectionPayload
                {
                    OperationId = operationId,
                    Operation = operation,
                    Connection = connection,
                    ConnectionId = connection.GetGuid(),
                    ClientVersion = connection.GetType().Assembly.GetName().Version?.ToString(),
                    Timestamp = Stopwatch.GetTimestamp()
                });

            return operationId;
        }

        internal void OnConnectionOpened(Guid operationId,
            DbConnection connection, [CallerMemberName] string operation = "")
        {
            if (IsEnabled(EventNames.ConnectionOpened))
                Write(
                    EventNames.ConnectionOpened,
                    new ConnectionPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        ConnectionId = connection.GetGuid(),
                        Connection = connection,
                        ClientVersion = connection.GetType().Assembly.GetName().Version?.ToString(),
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal void OnConnectionOpeningError(Guid operationId,
            DbConnection connection, Exception ex, [CallerMemberName] string operation = "")
        {
            if (IsEnabled(EventNames.ConnectionOpeningError))
                Write(
                    EventNames.ConnectionOpeningError,
                    new ConnectionPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        ConnectionId = connection.GetGuid(),
                        Connection = connection,
                        ClientVersion = connection.GetType().Assembly.GetName().Version?.ToString(),
                        Exception = ex,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal Guid OnConnectionClosing(DbConnection connection,
            [CallerMemberName] string operation = "")
        {
            if (!IsEnabled(EventNames.ConnectionClosing)) return Guid.Empty;

            var operationId = Guid.NewGuid();

            Write(
                EventNames.ConnectionClosing,
                new ConnectionPayload
                {
                    OperationId = operationId,
                    Operation = operation,
                    ConnectionId = connection.GetGuid(),
                    Connection = connection,
                    Timestamp = Stopwatch.GetTimestamp()
                });

            return operationId;
        }

        internal void OnConnectionClosed(Guid operationId, DbConnection connection,
            [CallerMemberName] string operation = "")
        {
            if (IsEnabled(EventNames.ConnectionClosed))
                Write(
                    EventNames.ConnectionClosed,
                    new ConnectionPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        ConnectionId = connection.GetGuid(),
                        Connection = connection,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal void OmConnectionClosingError(Guid operationId, DbConnection connection, Exception ex,
            [CallerMemberName] string operation = "")
        {
            if (IsEnabled(EventNames.ConnectionClosingError))
                Write(
                    EventNames.ConnectionClosingError,
                    new ConnectionPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        ConnectionId = connection.GetGuid(),
                        Connection = connection,
                        Exception = ex,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal Guid OnTransactionCommitting(IsolationLevel isolationLevel,
            DbConnection connection, DbTransaction transaction, [CallerMemberName] string operation = "")
        {
            if (!IsEnabled(EventNames.TransactionCommitting)) return Guid.Empty;

            var operationId = Guid.NewGuid();

            Write(
                EventNames.TransactionCommitting,
                new TransactionPayload
                {
                    OperationId = operationId,
                    Operation = operation,
                    IsolationLevel = isolationLevel,
                    Connection = connection,
                    TransactionId = transaction.GetHashCode(),
                    Timestamp = Stopwatch.GetTimestamp()
                });

            return operationId;
        }

        internal void OnTransactionCommitted(Guid operationId,
            IsolationLevel isolationLevel, DbConnection connection, DbTransaction transaction,
            [CallerMemberName] string operation = "")
        {
            if (IsEnabled(EventNames.TransactionCommitted))
                Write(
                    EventNames.TransactionCommitted,
                    new TransactionPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        IsolationLevel = isolationLevel,
                        Connection = connection,
                        TransactionId = transaction.GetHashCode(),
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal void OnTransactionCommittingError(Guid operationId,
            IsolationLevel isolationLevel, DbConnection connection, DbTransaction transaction, Exception ex,
            [CallerMemberName] string operation = "")
        {
            if (IsEnabled(EventNames.TransactionCommittingError))
                Write(
                    EventNames.TransactionCommittingError,
                    new TransactionPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        IsolationLevel = isolationLevel,
                        Connection = connection,
                        TransactionId = transaction.GetHashCode(),
                        Exception = ex,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal Guid OnTransactionRollingBack(
            IsolationLevel isolationLevel, DbConnection connection, DbTransaction transaction,
            [CallerMemberName] string operation = "")
        {
            if (!IsEnabled(EventNames.TransactionRollingBack)) return Guid.Empty;

            var operationId = Guid.NewGuid();
            Write(
                EventNames.TransactionRollingBack,
                new TransactionPayload
                {
                    OperationId = operationId,
                    Operation = operation,
                    IsolationLevel = isolationLevel,
                    Connection = connection,
                    TransactionId = transaction.GetHashCode(),
                    Timestamp = Stopwatch.GetTimestamp()
                });

            return operationId;
        }

        internal void OnTransactionRolledBack(Guid operationId,
            IsolationLevel isolationLevel, DbConnection connection, DbTransaction transaction,
            [CallerMemberName] string operation = "")
        {
            if (IsEnabled(EventNames.TransactionRolledBack))
                Write(
                    EventNames.TransactionRolledBack,
                    new TransactionPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        IsolationLevel = isolationLevel,
                        Connection = connection,
                        TransactionId = transaction.GetHashCode(),
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal void OnTransactionRollingBackError(Guid operationId,
            IsolationLevel isolationLevel, DbConnection connection, DbTransaction transaction, Exception ex,
            [CallerMemberName] string operation = "")
        {
            if (IsEnabled(EventNames.TransactionRollingBackError))
                Write(
                    EventNames.TransactionRollingBackError,
                    new TransactionPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        IsolationLevel = isolationLevel,
                        Connection = connection,
                        TransactionId = transaction.GetHashCode(),
                        Exception = ex,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }
    }
}