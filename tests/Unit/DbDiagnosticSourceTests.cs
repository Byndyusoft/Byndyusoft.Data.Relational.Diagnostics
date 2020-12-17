// ReSharper disable ExplicitCallerInfoArgument

using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.Data.Diagnostics;
using Microsoft.Data.Diagnostics.Payloads;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Moq;
using Moq.Protected;
using MySql.Data.MySqlClient;
using Npgsql;
using Xunit;

namespace Byndyusoft.Data.Relational.Diagnostics.Tests.Unit
{
    public class DbDiagnosticSourceTests
    {
        [Fact]
        public void GetEventName_NullConnectionType_ThrowsException()
        {
            // act
            var exception =
                Assert.Throws<ArgumentNullException>(() =>
                    DbDiagnosticSource.GetEventName(null!, "WriteCommandBefore"));

            // assert
            Assert.Equal("connectionType", exception.ParamName);
        }

        [Theory]
        [InlineData(typeof(SqlConnection), "Microsoft.Data.SqlClient.WriteCommandBefore")]
        [InlineData(typeof(SqliteConnection), "Microsoft.Data.Sqlite.WriteCommandBefore")]
        [InlineData(typeof(NpgsqlConnection), "System.Data.Npgsql.WriteCommandBefore")]
        [InlineData(typeof(MySqlConnection), "System.Data.MySql.WriteCommandBefore")]
        [InlineData(typeof(FbConnection), "System.Data.FirebirdClient.WriteCommandBefore")]
        [InlineData(typeof(SQLiteConnection), "System.Data.SQLite.WriteCommandBefore")]
        public void GetEventName_KnownConnections(Type connectionType, string expectedEventName)
        {
            // act
            var eventName = DbDiagnosticSource.GetEventName(connectionType, "WriteCommandBefore");

            // assert
            Assert.Equal(expectedEventName, eventName);
        }

        [Fact]
        public void GetEventName_UnknownConnection()
        {
            // arrange
            var connectionType = Mock.Of<DbConnection>().GetType();

            // act
            var eventName = DbDiagnosticSource.GetEventName(connectionType, "WriteCommandBefore");

            // assert
            Assert.Equal("System.Data.Common.WriteCommandBefore", eventName);
        }

        [Fact]
        public void AddEventsPrefix()
        {
            // arrange
            var connectionType = Mock.Of<DbConnection>().GetType();

            // act
            using var revert = DbDiagnosticSource.AddEventsPrefix(connectionType, "prefix");

            // assert
            var eventName = DbDiagnosticSource.GetEventName(connectionType, "WriteCommandBefore");
            Assert.Equal("prefix.WriteCommandBefore", eventName);
        }

        [Fact]
        public void AddEventsPrefix_UpdatesOne()
        {
            // arrange
            var connectionType = typeof(SqlConnection);
            using var revert = DbDiagnosticSource.AddEventsPrefix(connectionType, "prefix");

            // act
            DbDiagnosticSource.AddEventsPrefix(connectionType, "prefix2");

            // assert
            var eventName = DbDiagnosticSource.GetEventName(connectionType, "WriteCommandBefore");
            Assert.Equal("prefix2.WriteCommandBefore", eventName);
        }

        [Fact]
        public void ClearEventsPrefixes()
        {
            // arrange
            var connectionType = typeof(SqlConnection);
            using var revert = DbDiagnosticSource.AddEventsPrefix(connectionType, "prefix");

            // act
            DbDiagnosticSource.ClearEventsPrefixes();

            // assert
            var eventName = DbDiagnosticSource.GetEventName(connectionType, "WriteCommandBefore");
            Assert.Equal("System.Data.Common.WriteCommandBefore", eventName);
        }

        [Fact]
        public void WriteConnectionOpenBefore_IsNotEnabled()
        {
            // arrange
            var connection = Mock.Of<DbConnection>();
            var source = new DbDiagnosticSource();

            // act
            var operationId = source.WriteConnectionOpenBefore(connection, "operation");

            // assert
            Assert.Equal(Guid.Empty, operationId);
        }

        [Fact]
        public void WriteConnectionOpenBefore()
        {
            // arrange
            var connection = Mock.Of<DbConnection>();
            var source = new DbDiagnosticSource();

            var operationId = Guid.Empty;

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () => { operationId = source.WriteConnectionOpenBefore(connection, "operation"); });

            // assert
            Assert.Equal(DbDiagnosticSource.GetEventName(connection.GetType(), "WriteConnectionOpenBefore"), eventName);
            var connectionPayload = Assert.IsType<ConnectionPayload>(payload);
            Assert.Equal(connection, connectionPayload.Connection);
            Assert.Equal(connection.GetId(), connectionPayload.ConnectionId);
            Assert.Equal(operationId, connectionPayload.OperationId);
            Assert.Equal("operation", connectionPayload.Operation);
            Assert.Null(connectionPayload.Exception);
        }

        [Fact]
        public void WriteConnectionOpenAfter()
        {
            // arrange
            var operationId = Guid.NewGuid();
            var connection = Mock.Of<DbConnection>();
            var source = new DbDiagnosticSource();

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () => source.WriteConnectionOpenAfter(operationId, connection, "operation"));

            // assert
            Assert.Equal(DbDiagnosticSource.GetEventName(connection.GetType(), "WriteConnectionOpenAfter"), eventName);
            var connectionPayload = Assert.IsType<ConnectionPayload>(payload);
            Assert.Equal(connection, connectionPayload.Connection);
            Assert.Equal(connection.GetId(), connectionPayload.ConnectionId);
            Assert.Equal(operationId, connectionPayload.OperationId);
            Assert.Equal("operation", connectionPayload.Operation);
            Assert.Null(connectionPayload.Exception);
        }

        [Fact]
        public void WriteConnectionOpenError()
        {
            // arrange
            var operationId = Guid.NewGuid();
            var exception = Mock.Of<Exception>();
            var connection = Mock.Of<DbConnection>();
            var source = new DbDiagnosticSource();

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () => source.WriteConnectionOpenError(operationId, connection, exception, "operation"));

            // assert
            Assert.Equal(DbDiagnosticSource.GetEventName(connection.GetType(), "WriteConnectionOpenError"), eventName);
            var connectionPayload = Assert.IsType<ConnectionPayload>(payload);
            Assert.Equal(connection, connectionPayload.Connection);
            Assert.Equal(connection.GetId(), connectionPayload.ConnectionId);
            Assert.Equal(operationId, connectionPayload.OperationId);
            Assert.Equal("operation", connectionPayload.Operation);
            Assert.Equal(exception, connectionPayload.Exception);
        }

        [Fact]
        public void WriteConnectionCloseBefore_IsNotEnabled()
        {
            // arrange
            var connection = Mock.Of<DbConnection>();
            var source = new DbDiagnosticSource();

            // act
            var operationId = source.WriteConnectionCloseBefore(connection, "operation");

            // assert
            Assert.Equal(Guid.Empty, operationId);
        }

        [Fact]
        public void WriteConnectionCloseBefore()
        {
            // arrange
            var connection = Mock.Of<DbConnection>();
            var source = new DbDiagnosticSource();
            var operationId = Guid.Empty;

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () => { operationId = source.WriteConnectionCloseBefore(connection, "operation"); });

            // assert
            Assert.Equal(DbDiagnosticSource.GetEventName(connection.GetType(), "WriteConnectionCloseBefore"),
                eventName);
            var connectionPayload = Assert.IsType<ConnectionPayload>(payload);
            Assert.Equal(connection, connectionPayload.Connection);
            Assert.Equal(connection.GetId(), connectionPayload.ConnectionId);
            Assert.Equal(operationId, connectionPayload.OperationId);
            Assert.Equal("operation", connectionPayload.Operation);
            Assert.Null(connectionPayload.Exception);
        }

        [Fact]
        public void WriteConnectionCloseAfter()
        {
            // arrange
            var operationId = Guid.NewGuid();
            var connection = Mock.Of<DbConnection>();
            var source = new DbDiagnosticSource();

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () => source.WriteConnectionCloseAfter(operationId, connection, "operation"));

            // assert
            Assert.Equal(DbDiagnosticSource.GetEventName(connection.GetType(), "WriteConnectionCloseAfter"), eventName);
            var connectionPayload = Assert.IsType<ConnectionPayload>(payload);
            Assert.Equal(connection, connectionPayload.Connection);
            Assert.Equal(connection.GetId(), connectionPayload.ConnectionId);
            Assert.Equal(operationId, connectionPayload.OperationId);
            Assert.Equal("operation", connectionPayload.Operation);
            Assert.Null(connectionPayload.Exception);
        }

        [Fact]
        public void WriteConnectionCloseError()
        {
            // arrange
            var operationId = Guid.NewGuid();
            var exception = Mock.Of<Exception>();
            var connection = Mock.Of<DbConnection>();
            var source = new DbDiagnosticSource();

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () => { source.WriteConnectionCloseError(operationId, connection, exception, "operation"); });

            // assert
            Assert.Equal(DbDiagnosticSource.GetEventName(connection.GetType(), "WriteConnectionCloseError"), eventName);
            var connectionPayload = Assert.IsType<ConnectionPayload>(payload);
            Assert.Equal(connection, connectionPayload.Connection);
            Assert.Equal(connection.GetId(), connectionPayload.ConnectionId);
            Assert.Equal(operationId, connectionPayload.OperationId);
            Assert.Equal("operation", connectionPayload.Operation);
            Assert.Equal(exception, connectionPayload.Exception);
        }

        [Fact]
        public void WriteCommandBefore_IsNotEnabled()
        {
            // arrange
            var command = Mock.Of<DbCommand>();
            Mock.Get(command).Protected().SetupGet<DbConnection>("DbConnection").Returns(Mock.Of<DbConnection>());
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticSource();

            // act
            var operationId = source.WriteCommandBefore(command, transaction, "operation");

            // assert
            Assert.Equal(Guid.Empty, operationId);
        }

        [Fact]
        public void WriteCommandBefore()
        {
            // arrange
            var connection = Mock.Of<DbConnection>();
            var command = Mock.Of<DbCommand>();
            Mock.Get(command).Protected().SetupGet<DbConnection>("DbConnection").Returns(connection);
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticSource();
            var operationId = Guid.Empty;

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () => { operationId = source.WriteCommandBefore(command, transaction, "operation"); });

            // assert
            Assert.Equal(DbDiagnosticSource.GetEventName(connection.GetType(), "WriteCommandBefore"), eventName);
            var commandPayload = Assert.IsType<CommandPayload>(payload);
            Assert.Equal(command, commandPayload.Command);
            Assert.Equal(connection.GetId(), commandPayload.ConnectionId);
            Assert.Equal(operationId, commandPayload.OperationId);
            Assert.Equal("operation", commandPayload.Operation);
            Assert.Equal(transaction.GetId(), commandPayload.TransactionId);
            Assert.Null(commandPayload.Exception);
        }

        [Fact]
        public void WriteCommandAfter()
        {
            // arrange
            var operationId = Guid.NewGuid();
            var connection = Mock.Of<DbConnection>();
            var command = Mock.Of<DbCommand>();
            Mock.Get(command).Protected().SetupGet<DbConnection>("DbConnection").Returns(connection);
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticSource();

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () => source.WriteCommandAfter(operationId, command, transaction, "operation"));

            // assert
            Assert.Equal(DbDiagnosticSource.GetEventName(connection.GetType(), "WriteCommandAfter"), eventName);
            var commandPayload = Assert.IsType<CommandPayload>(payload);
            Assert.Equal(command, commandPayload.Command);
            Assert.Equal(connection.GetId(), commandPayload.ConnectionId);
            Assert.Equal(operationId, commandPayload.OperationId);
            Assert.Equal("operation", commandPayload.Operation);
            Assert.Equal(transaction.GetId(), commandPayload.TransactionId);
            Assert.Null(commandPayload.Exception);
        }

        [Fact]
        public void WriteCommandError()
        {
            // arrange
            var operationId = Guid.NewGuid();
            var exception = Mock.Of<Exception>();
            var connection = Mock.Of<DbConnection>();
            var command = Mock.Of<DbCommand>();
            Mock.Get(command).Protected().SetupGet<DbConnection>("DbConnection").Returns(connection);
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticSource();

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () => { source.WriteCommandError(operationId, command, transaction, exception, "operation"); });

            // assert
            Assert.Equal(DbDiagnosticSource.GetEventName(connection.GetType(), "WriteCommandError"), eventName);
            var commandPayload = Assert.IsType<CommandPayload>(payload);
            Assert.Equal(command, commandPayload.Command);
            Assert.Equal(connection.GetId(), commandPayload.ConnectionId);
            Assert.Equal(operationId, commandPayload.OperationId);
            Assert.Equal("operation", commandPayload.Operation);
            Assert.Equal(transaction.GetId(), commandPayload.TransactionId);
            Assert.Equal(exception, commandPayload.Exception);
        }

        [Fact]
        public void WriteTransactionCommitBefore_IsNotEnabled()
        {
            // arrange
            var isolationLevel = IsolationLevel.Chaos;
            var connection = Mock.Of<DbConnection>();
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticSource();

            // act
            var operationId = source.WriteTransactionCommitBefore(isolationLevel, connection, transaction, "operation");

            // assert
            Assert.Equal(Guid.Empty, operationId);
        }

        [Fact]
        public void WriteTransactionCommitBefore()
        {
            // arrange
            var isolationLevel = IsolationLevel.Chaos;
            var connection = Mock.Of<DbConnection>();
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticSource();
            var operationId = Guid.Empty;

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () =>
                {
                    operationId =
                        source.WriteTransactionCommitBefore(isolationLevel, connection, transaction, "operation");
                });

            // assert
            Assert.Equal(DbDiagnosticSource.GetEventName(connection.GetType(), "WriteTransactionCommitBefore"),
                eventName);
            var transactionPayload = Assert.IsType<TransactionPayload>(payload);
            Assert.Equal(transaction.GetId(), transactionPayload.TransactionId);
            Assert.Equal(connection, transactionPayload.Connection);
            Assert.Equal(isolationLevel, transactionPayload.IsolationLevel);
            Assert.Equal(operationId, transactionPayload.OperationId);
            Assert.Equal("operation", transactionPayload.Operation);
            Assert.Null(transactionPayload.Exception);
        }

        [Fact]
        public void WriteTransactionCommitAfter()
        {
            // arrange
            var isolationLevel = IsolationLevel.Chaos;
            var connection = Mock.Of<DbConnection>();
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticSource();
            var operationId = Guid.NewGuid();

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () =>
                {
                    source.WriteTransactionCommitAfter(operationId, isolationLevel, connection, transaction,
                        "operation");
                });

            // assert
            Assert.Equal(DbDiagnosticSource.GetEventName(connection.GetType(), "WriteTransactionCommitAfter"),
                eventName);
            var transactionPayload = Assert.IsType<TransactionPayload>(payload);
            Assert.Equal(transaction.GetId(), transactionPayload.TransactionId);
            Assert.Equal(connection, transactionPayload.Connection);
            Assert.Equal(isolationLevel, transactionPayload.IsolationLevel);
            Assert.Equal(operationId, transactionPayload.OperationId);
            Assert.Equal("operation", transactionPayload.Operation);
            Assert.Null(transactionPayload.Exception);
        }

        [Fact]
        public void WriteTransactionCommitError()
        {
            // arrange
            var exception = Mock.Of<Exception>();
            var isolationLevel = IsolationLevel.Chaos;
            var connection = Mock.Of<DbConnection>();
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticSource();
            var operationId = Guid.NewGuid();

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () =>
                {
                    source.WriteTransactionCommitError(operationId, isolationLevel, connection, transaction, exception,
                        "operation");
                });

            // assert
            Assert.Equal(DbDiagnosticSource.GetEventName(connection.GetType(), "WriteTransactionCommitError"),
                eventName);
            var transactionPayload = Assert.IsType<TransactionPayload>(payload);
            Assert.Equal(transaction.GetId(), transactionPayload.TransactionId);
            Assert.Equal(connection, transactionPayload.Connection);
            Assert.Equal(isolationLevel, transactionPayload.IsolationLevel);
            Assert.Equal(operationId, transactionPayload.OperationId);
            Assert.Equal("operation", transactionPayload.Operation);
            Assert.Equal(exception, transactionPayload.Exception);
        }

        [Fact]
        public void WriteTransactionRollbackBefore_IsNotEnabled()
        {
            // arrange
            var isolationLevel = IsolationLevel.Chaos;
            var connection = Mock.Of<DbConnection>();
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticSource();

            // act
            var operationId =
                source.WriteTransactionRollbackBefore(isolationLevel, connection, transaction, "operation");

            // assert
            Assert.Equal(Guid.Empty, operationId);
        }

        [Fact]
        public void WriteTransactionRollbackBefore()
        {
            // arrange
            var isolationLevel = IsolationLevel.Chaos;
            var connection = Mock.Of<DbConnection>();
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticSource();
            var operationId = Guid.Empty;

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () =>
                {
                    operationId =
                        source.WriteTransactionRollbackBefore(isolationLevel, connection, transaction, "operation");
                });

            // assert
            Assert.Equal(DbDiagnosticSource.GetEventName(connection.GetType(), "WriteTransactionRollbackBefore"),
                eventName);
            var transactionPayload = Assert.IsType<TransactionPayload>(payload);
            Assert.Equal(transaction.GetId(), transactionPayload.TransactionId);
            Assert.Equal(connection, transactionPayload.Connection);
            Assert.Equal(isolationLevel, transactionPayload.IsolationLevel);
            Assert.Equal(operationId, transactionPayload.OperationId);
            Assert.Equal("operation", transactionPayload.Operation);
            Assert.Null(transactionPayload.Exception);
        }

        [Fact]
        public void WriteTransactionRollbackAfter()
        {
            // arrange
            var isolationLevel = IsolationLevel.Chaos;
            var connection = Mock.Of<DbConnection>();
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticSource();
            var operationId = Guid.NewGuid();

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () =>
                {
                    source.WriteTransactionRollbackAfter(operationId, isolationLevel, connection, transaction,
                        "operation");
                });

            // assert
            Assert.Equal(DbDiagnosticSource.GetEventName(connection.GetType(), "WriteTransactionRollbackAfter"),
                eventName);
            var transactionPayload = Assert.IsType<TransactionPayload>(payload);
            Assert.Equal(transaction.GetId(), transactionPayload.TransactionId);
            Assert.Equal(connection, transactionPayload.Connection);
            Assert.Equal(isolationLevel, transactionPayload.IsolationLevel);
            Assert.Equal(operationId, transactionPayload.OperationId);
            Assert.Equal("operation", transactionPayload.Operation);
            Assert.Null(transactionPayload.Exception);
        }

        [Fact]
        public void WriteTransactionRollbackError()
        {
            // arrange
            var exception = Mock.Of<Exception>();
            var isolationLevel = IsolationLevel.Chaos;
            var connection = Mock.Of<DbConnection>();
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticSource();
            var operationId = Guid.NewGuid();

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () =>
                {
                    source.WriteTransactionRollbackError(operationId, isolationLevel, connection, transaction,
                        exception, "operation");
                });

            // assert
            Assert.Equal(DbDiagnosticSource.GetEventName(connection.GetType(), "WriteTransactionRollbackError"),
                eventName);
            var transactionPayload = Assert.IsType<TransactionPayload>(payload);
            Assert.Equal(transaction.GetId(), transactionPayload.TransactionId);
            Assert.Equal(connection, transactionPayload.Connection);
            Assert.Equal(isolationLevel, transactionPayload.IsolationLevel);
            Assert.Equal(operationId, transactionPayload.OperationId);
            Assert.Equal("operation", transactionPayload.Operation);
            Assert.Equal(exception, transactionPayload.Exception);
        }
    }
}