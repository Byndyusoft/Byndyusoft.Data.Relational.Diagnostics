using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Data.Diagnostics.Payloads;

namespace Microsoft.Data.Diagnostics
{
    public class DbDiagnosticListener : DiagnosticListener
    {
        private const string Prefix = "Microsoft.Data.Common";

        public const string SqlBeforeExecuteCommand = Prefix + nameof(WriteCommandBefore);
        public const string SqlAfterExecuteCommand = Prefix + nameof(WriteCommandAfter);
        public const string SqlErrorExecuteCommand = Prefix + nameof(WriteCommandError);

        public const string SqlBeforeOpenConnection = Prefix + nameof(WriteConnectionOpenBefore);
        public const string SqlAfterOpenConnection = Prefix + nameof(WriteConnectionOpenAfter);
        public const string SqlErrorOpenConnection = Prefix + nameof(WriteConnectionOpenError);

        public const string SqlBeforeCloseConnection = Prefix + nameof(WriteConnectionCloseBefore);
        public const string SqlAfterCloseConnection = Prefix + nameof(WriteConnectionCloseAfter);
        public const string SqlErrorCloseConnection = Prefix + nameof(WriteConnectionCloseError);

        public const string SqlBeforeCommitTransaction = Prefix + nameof(WriteTransactionCommitBefore);
        public const string SqlAfterCommitTransaction = Prefix + nameof(WriteTransactionCommitAfter);
        public const string SqlErrorCommitTransaction = Prefix + nameof(WriteTransactionCommitError);

        public const string SqlBeforeRollbackTransaction = Prefix + nameof(WriteTransactionRollbackBefore);
        public const string SqlAfterRollbackTransaction = Prefix + nameof(WriteTransactionRollbackAfter);
        public const string SqlErrorRollbackTransaction = Prefix + nameof(WriteTransactionRollbackError);

        public static readonly DbDiagnosticListener Instance = new DbDiagnosticListener();

        public DbDiagnosticListener() : base(nameof(DbDiagnosticListener))
        {
        }

        internal Guid WriteCommandBefore(DiagnosedDbCommand sqlCommand,
            DiagnosedDbTransaction? transaction, [CallerMemberName] string operation = "")
        {
            if (!IsEnabled(SqlBeforeExecuteCommand)) return Guid.Empty;

            var operationId = Guid.NewGuid();

            Write(
                SqlBeforeExecuteCommand,
                new CommandPayload
                {
                    OperationId = operationId,
                    Operation = operation,
                    ConnectionId = sqlCommand.Connection.ClientConnectionId,
                    Command = sqlCommand,
                    TransactionId = transaction?.TransactionId,
                    Timestamp = Stopwatch.GetTimestamp()
                });

            return operationId;

        }

        internal void WriteCommandAfter(Guid operationId, DiagnosedDbCommand sqlCommand,
            DiagnosedDbTransaction? transaction, [CallerMemberName] string operation = "")
        {
            if (IsEnabled(SqlAfterExecuteCommand))
                Write(
                    SqlAfterExecuteCommand,
                    new CommandPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        ConnectionId = sqlCommand.Connection.ClientConnectionId,
                        Command = sqlCommand,
                        TransactionId = transaction?.TransactionId,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal void WriteCommandError(Guid operationId, DiagnosedDbCommand sqlCommand,
            DiagnosedDbTransaction? transaction, Exception ex, [CallerMemberName] string operation = "")
        {
            if (IsEnabled(SqlErrorExecuteCommand))
                Write(
                    SqlErrorExecuteCommand,
                    new CommandPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        ConnectionId = sqlCommand.Connection.ClientConnectionId,
                        Command = sqlCommand,
                        TransactionId = transaction?.TransactionId,
                        Exception = ex,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal Guid WriteConnectionOpenBefore(DiagnosedDbConnection sqlConnection,
            [CallerMemberName] string operation = "")
        {
            if (!IsEnabled(SqlBeforeOpenConnection)) return Guid.Empty;
            
            var operationId = Guid.NewGuid();

            Write(
                SqlBeforeOpenConnection,
                new ConnectionPayload
                {
                    OperationId = operationId,
                    Operation = operation,
                    Connection = sqlConnection,
                    ClientVersion = sqlConnection.GetType().Assembly.GetName().Version?.ToString(),
                    Timestamp = Stopwatch.GetTimestamp()
                });

            return operationId;

        }

        internal void WriteConnectionOpenAfter(Guid operationId,
            DiagnosedDbConnection sqlConnection, [CallerMemberName] string operation = "")
        {
            if (IsEnabled(SqlAfterOpenConnection))
                Write(
                    SqlAfterOpenConnection,
                    new ConnectionPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        ConnectionId = sqlConnection.ClientConnectionId,
                        Connection = sqlConnection,
                        ClientVersion = sqlConnection.GetType().Assembly.GetName().Version?.ToString(),
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal void WriteConnectionOpenError(Guid operationId,
            DiagnosedDbConnection sqlConnection, Exception ex, [CallerMemberName] string operation = "")
        {
            if (IsEnabled(SqlErrorOpenConnection))
                Write(
                    SqlErrorOpenConnection,
                    new ConnectionPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        ConnectionId = sqlConnection.ClientConnectionId,
                        Connection = sqlConnection,
                        ClientVersion = sqlConnection.GetType().Assembly.GetName().Version?.ToString(),
                        Exception = ex,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal Guid WriteConnectionCloseBefore(DiagnosedDbConnection sqlConnection,
            [CallerMemberName] string operation = "")
        {
            if (!IsEnabled(SqlBeforeCloseConnection)) return Guid.Empty;
            
            var operationId = Guid.NewGuid();

            Write(
                SqlBeforeCloseConnection,
                new ConnectionPayload
                {
                    OperationId = operationId,
                    Operation = operation,
                    ConnectionId = sqlConnection.ClientConnectionId,
                    Connection = sqlConnection,
                    Timestamp = Stopwatch.GetTimestamp()
                });

            return operationId;

        }

        internal void WriteConnectionCloseAfter(Guid operationId,
            Guid clientConnectionId, DbConnection sqlConnection, [CallerMemberName] string operation = "")
        {
            if (IsEnabled(SqlAfterCloseConnection))
                Write(
                    SqlAfterCloseConnection,
                    new ConnectionPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        ConnectionId = clientConnectionId,
                        Connection = sqlConnection,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal void WriteConnectionCloseError(Guid operationId,
            Guid clientConnectionId, DiagnosedDbConnection sqlConnection, Exception ex,
            [CallerMemberName] string operation = "")
        {
            if (IsEnabled(SqlErrorCloseConnection))
                Write(
                    SqlErrorCloseConnection,
                    new ConnectionPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        ConnectionId = clientConnectionId,
                        Connection = sqlConnection,
                        Exception = ex,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal Guid WriteTransactionCommitBefore(IsolationLevel isolationLevel,
            DbConnection connection, DiagnosedDbTransaction transaction, [CallerMemberName] string operation = "")
        {
            if (!IsEnabled(SqlBeforeCommitTransaction)) return Guid.Empty;
            
            var operationId = Guid.NewGuid();

            Write(
                SqlBeforeCommitTransaction,
                new TransactionPayload
                {
                    OperationId = operationId,
                    Operation = operation,
                    IsolationLevel = isolationLevel,
                    Connection = connection,
                    TransactionId = transaction.TransactionId,
                    Timestamp = Stopwatch.GetTimestamp()
                });

            return operationId;

        }

        internal void WriteTransactionCommitAfter(Guid operationId,
            IsolationLevel isolationLevel, DbConnection connection, DiagnosedDbTransaction transaction,
            [CallerMemberName] string operation = "")
        {
            if (IsEnabled(SqlAfterCommitTransaction))
                Write(
                    SqlAfterCommitTransaction,
                    new TransactionPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        IsolationLevel = isolationLevel,
                        Connection = connection,
                        TransactionId = transaction.TransactionId,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal void WriteTransactionCommitError(Guid operationId,
            IsolationLevel isolationLevel, DbConnection connection, DiagnosedDbTransaction transaction, Exception ex,
            [CallerMemberName] string operation = "")
        {
            if (IsEnabled(SqlErrorCommitTransaction))
                Write(
                    SqlErrorCommitTransaction,
                    new TransactionPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        IsolationLevel = isolationLevel,
                        Connection = connection,
                        TransactionId = transaction.TransactionId,
                        Exception = ex,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal Guid WriteTransactionRollbackBefore(
            IsolationLevel isolationLevel, DbConnection connection, DiagnosedDbTransaction transaction, [CallerMemberName] string operation = "")
        {
            if (!IsEnabled(SqlBeforeRollbackTransaction)) return Guid.Empty;
            
            var operationId = Guid.NewGuid();

            Write(
                SqlBeforeRollbackTransaction,
                new TransactionPayload
                {
                    OperationId = operationId,
                    Operation = operation,
                    IsolationLevel = isolationLevel,
                    Connection = connection,
                    TransactionId = transaction.TransactionId,
                    Timestamp = Stopwatch.GetTimestamp()
                });

            return operationId;

        }

        internal void WriteTransactionRollbackAfter(Guid operationId,
            IsolationLevel isolationLevel, DbConnection connection, DiagnosedDbTransaction transaction, [CallerMemberName] string operation = "")
        {
            if (IsEnabled(SqlAfterRollbackTransaction))
                Write(
                    SqlAfterRollbackTransaction,
                    new TransactionPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        IsolationLevel = isolationLevel,
                        Connection = connection,
                        TransactionId = transaction.TransactionId,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }

        internal void WriteTransactionRollbackError(Guid operationId,
            IsolationLevel isolationLevel, DbConnection connection, DiagnosedDbTransaction transaction, Exception ex, [CallerMemberName] string operation = "")
        {
            if (IsEnabled(SqlErrorRollbackTransaction))
                Write(
                    SqlErrorRollbackTransaction,
                    new TransactionPayload
                    {
                        OperationId = operationId,
                        Operation = operation,
                        IsolationLevel = isolationLevel,
                        Connection = connection,
                        TransactionId = transaction.TransactionId,
                        Exception = ex,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
        }
    }
}