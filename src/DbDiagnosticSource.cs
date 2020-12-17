using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Data.Diagnostics.Payloads;

namespace Microsoft.Data.Diagnostics
{
    public partial class DbDiagnosticSource : DiagnosticListener
    {
        public const string DefaultPrefix = "System.Data.Common";

        private static Dictionary<string, string> _eventPrefixes = new Dictionary<string, string>
        {
            ["SqlConnection"] = "Microsoft.Data.SqlClient",
            ["SqliteConnection"] = "Microsoft.Data.Sqlite",
            ["NpgsqlConnection"] = "System.Data.Npgsql",
            ["MySqlConnection"] = "System.Data.MySql",
            ["FbConnection"] = "System.Data.FirebirdClient",
            ["SQLiteConnection"] = "System.Data.SQLite"
        };

        public DbDiagnosticSource() : base(nameof(DbDiagnosticSource))
        {
        }

        public static IDisposable AddEventsPrefix(Type connectionType, string prefix)
        {
            var revert = new EventsPrefixChangingRevert();
            _eventPrefixes[connectionType.Name] = prefix;
            return revert;
        }

        public static IDisposable ClearEventsPrefixes()
        {
            var revert = new EventsPrefixChangingRevert();
            _eventPrefixes.Clear();
            return revert;
        }

        public static string GetEventName(Type connectionType, string eventType)
        {
            if (connectionType == null) throw new ArgumentNullException(nameof(connectionType));

            var connectionTypeName = connectionType.Name;
            if (_eventPrefixes.TryGetValue(connectionTypeName, out var prefix) == false) prefix = DefaultPrefix;

            return $"{prefix}.{eventType}";
        }

        private static string GetEventName(DbConnection connection, string eventType)
        {
            var underlyingConnection = connection.GetUnderlying();
            return GetEventName(underlyingConnection.GetType(), eventType);
        }

        internal Guid WriteCommandBefore(DbCommand command,
            DbTransaction? transaction, [CallerMemberName] string operation = "")
        {
            var eventName = GetEventName(command.Connection, EventNames.WriteCommandBefore);

            if (!IsEnabled(eventName)) return Guid.Empty;

            var operationId = Guid.NewGuid();
            Write(
                eventName,
                new CommandPayload
                {
                    OperationId = operationId,
                    Operation = operation,
                    ConnectionId = command.Connection.GetId(),
                    Command = command,
                    TransactionId = transaction?.GetHashCode(),
                    Timestamp = Stopwatch.GetTimestamp()
                });

            return operationId;
        }

        internal void WriteCommandAfter(Guid operationId, DbCommand command,
            DbTransaction? transaction, [CallerMemberName] string operation = "")
        {
            var eventName = GetEventName(command.Connection, EventNames.WriteCommandAfter);

            if (IsEnabled(eventName))
                Write(
                    eventName,
                    new CommandPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        ConnectionId = command.Connection.GetId(),
                        Command = command,
                        TransactionId = transaction?.GetHashCode(),
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal void WriteCommandError(Guid operationId, DbCommand command,
            DbTransaction? transaction, Exception ex, [CallerMemberName] string operation = "")
        {
            var eventName = GetEventName(command.Connection, EventNames.WriteCommandError);

            if (IsEnabled(eventName))
                Write(
                    eventName,
                    new CommandPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        ConnectionId = command.Connection.GetId(),
                        Command = command,
                        TransactionId = transaction?.GetHashCode(),
                        Exception = ex,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal Guid WriteConnectionOpenBefore(DbConnection connection,
            [CallerMemberName] string operation = "")
        {
            var eventName = GetEventName(connection, EventNames.WriteConnectionOpenBefore);

            if (!IsEnabled(eventName)) return Guid.Empty;

            var operationId = Guid.NewGuid();
            Write(
                eventName,
                new ConnectionPayload
                {
                    OperationId = operationId,
                    Operation = operation,
                    Connection = connection,
                    ConnectionId = connection.GetId(),
                    ClientVersion = connection.GetType().Assembly.GetName().Version?.ToString(),
                    Timestamp = Stopwatch.GetTimestamp()
                });

            return operationId;
        }

        internal void WriteConnectionOpenAfter(Guid operationId,
            DbConnection connection, [CallerMemberName] string operation = "")
        {
            var eventName = GetEventName(connection, EventNames.WriteConnectionOpenAfter);

            if (IsEnabled(eventName))
                Write(
                    eventName,
                    new ConnectionPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        ConnectionId = connection.GetId(),
                        Connection = connection,
                        ClientVersion = connection.GetType().Assembly.GetName().Version?.ToString(),
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal void WriteConnectionOpenError(Guid operationId,
            DbConnection connection, Exception ex, [CallerMemberName] string operation = "")
        {
            var eventName = GetEventName(connection, EventNames.WriteConnectionOpenError);

            if (IsEnabled(eventName))
                Write(
                    eventName,
                    new ConnectionPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        ConnectionId = connection.GetId(),
                        Connection = connection,
                        ClientVersion = connection.GetType().Assembly.GetName().Version?.ToString(),
                        Exception = ex,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal Guid WriteConnectionCloseBefore(DbConnection connection,
            [CallerMemberName] string operation = "")
        {
            var eventName = GetEventName(connection, EventNames.WriteConnectionCloseBefore);

            if (!IsEnabled(eventName)) return Guid.Empty;

            var operationId = Guid.NewGuid();

            Write(
                eventName,
                new ConnectionPayload
                {
                    OperationId = operationId,
                    Operation = operation,
                    ConnectionId = connection.GetId(),
                    Connection = connection,
                    Timestamp = Stopwatch.GetTimestamp()
                });

            return operationId;
        }

        internal void WriteConnectionCloseAfter(Guid operationId, DbConnection connection,
            [CallerMemberName] string operation = "")
        {
            var eventName = GetEventName(connection, EventNames.WriteConnectionCloseAfter);

            if (IsEnabled(eventName))
                Write(
                    eventName,
                    new ConnectionPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        ConnectionId = connection.GetId(),
                        Connection = connection,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal void WriteConnectionCloseError(Guid operationId, DbConnection connection, Exception ex,
            [CallerMemberName] string operation = "")
        {
            var eventName = GetEventName(connection, EventNames.WriteConnectionCloseError);

            if (IsEnabled(eventName))
                Write(
                    eventName,
                    new ConnectionPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        ConnectionId = connection.GetId(),
                        Connection = connection,
                        Exception = ex,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal Guid WriteTransactionCommitBefore(IsolationLevel isolationLevel,
            DbConnection connection, DbTransaction transaction, [CallerMemberName] string operation = "")
        {
            var eventName = GetEventName(connection, EventNames.WriteTransactionCommitBefore);

            if (!IsEnabled(eventName)) return Guid.Empty;

            var operationId = Guid.NewGuid();

            Write(
                eventName,
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

        internal void WriteTransactionCommitAfter(Guid operationId,
            IsolationLevel isolationLevel, DbConnection connection, DbTransaction transaction,
            [CallerMemberName] string operation = "")
        {
            var eventName = GetEventName(connection, EventNames.WriteTransactionCommitAfter);

            if (IsEnabled(eventName))
                Write(
                    eventName,
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

        internal void WriteTransactionCommitError(Guid operationId,
            IsolationLevel isolationLevel, DbConnection connection, DbTransaction transaction, Exception ex,
            [CallerMemberName] string operation = "")
        {
            var eventName = GetEventName(connection, EventNames.WriteTransactionCommitError);

            if (IsEnabled(eventName))
                Write(
                    eventName,
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

        internal Guid WriteTransactionRollbackBefore(
            IsolationLevel isolationLevel, DbConnection connection, DbTransaction transaction,
            [CallerMemberName] string operation = "")
        {
            var eventName = GetEventName(connection, EventNames.WriteTransactionRollbackBefore);

            if (!IsEnabled(eventName)) return Guid.Empty;

            var operationId = Guid.NewGuid();
            Write(
                eventName,
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

        internal void WriteTransactionRollbackAfter(Guid operationId,
            IsolationLevel isolationLevel, DbConnection connection, DbTransaction transaction,
            [CallerMemberName] string operation = "")
        {
            var eventName = GetEventName(connection, EventNames.WriteTransactionRollbackAfter);

            if (IsEnabled(eventName))
                Write(
                    eventName,
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

        internal void WriteTransactionRollbackError(Guid operationId,
            IsolationLevel isolationLevel, DbConnection connection, DbTransaction transaction, Exception ex,
            [CallerMemberName] string operation = "")
        {
            var eventName = GetEventName(connection, EventNames.WriteTransactionRollbackError);

            if (IsEnabled(eventName))
                Write(
                    eventName,
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

        private class EventsPrefixChangingRevert : IDisposable
        {
            private readonly Dictionary<string, string> _previous;

            public EventsPrefixChangingRevert()
            {
                _previous = new Dictionary<string, string>(_eventPrefixes);
            }

            public void Dispose()
            {
                _eventPrefixes = _previous;
            }
        }
    }
}