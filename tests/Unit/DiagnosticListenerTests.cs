// ReSharper disable ExplicitCallerInfoArgument

using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Diagnostics;
using Microsoft.Data.Diagnostics.Payloads;
using Moq;
using Moq.Protected;
using Xunit;

namespace Byndyusoft.Data.Relational.Diagnostics.Tests.Unit
{
    public class DiagnosticListenerTests
    {
        [Fact]
        public void OnConnectionOpening_IsNotEnabled()
        {
            // arrange
            var connection = Mock.Of<DbConnection>();
            var source = new DbDiagnosticListener();

            // act
            var operationId = source.OnConnectionOpening(connection, "operation");

            // assert
            Assert.Equal(Guid.Empty, operationId);
        }

        [Fact]
        public void OnConnectionOpening()
        {
            // arrange
            var connection = Mock.Of<DbConnection>();
            var source = new DbDiagnosticListener();

            var operationId = Guid.Empty;

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () => { operationId = source.OnConnectionOpening(connection, "operation"); });

            // assert
            Assert.Equal("System.Data.Common.ConnectionOpening", eventName);
            var connectionPayload = Assert.IsType<ConnectionPayload>(payload);
            Assert.Equal(connection, connectionPayload.Connection);
            Assert.Equal(connection.GetGuid(), connectionPayload.ConnectionId);
            Assert.Equal(operationId, connectionPayload.OperationId);
            Assert.Equal("operation", connectionPayload.Operation);
            Assert.Null(connectionPayload.Exception);
        }

        [Fact]
        public void OnConnectionOpened()
        {
            // arrange
            var operationId = Guid.NewGuid();
            var connection = Mock.Of<DbConnection>();
            var source = new DbDiagnosticListener();

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () => source.OnConnectionOpened(operationId, connection, "operation"));

            // assert
            Assert.Equal("System.Data.Common.ConnectionOpened", eventName);
            var connectionPayload = Assert.IsType<ConnectionPayload>(payload);
            Assert.Equal(connection, connectionPayload.Connection);
            Assert.Equal(connection.GetGuid(), connectionPayload.ConnectionId);
            Assert.Equal(operationId, connectionPayload.OperationId);
            Assert.Equal("operation", connectionPayload.Operation);
            Assert.Null(connectionPayload.Exception);
        }

        [Fact]
        public void OnConnectionOpeningError()
        {
            // arrange
            var operationId = Guid.NewGuid();
            var exception = Mock.Of<Exception>();
            var connection = Mock.Of<DbConnection>();
            var source = new DbDiagnosticListener();

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () => source.OnConnectionOpeningError(operationId, connection, exception, "operation"));

            // assert
            Assert.Equal("System.Data.Common.ConnectionOpeningError", eventName);
            var connectionPayload = Assert.IsType<ConnectionPayload>(payload);
            Assert.Equal(connection, connectionPayload.Connection);
            Assert.Equal(connection.GetGuid(), connectionPayload.ConnectionId);
            Assert.Equal(operationId, connectionPayload.OperationId);
            Assert.Equal("operation", connectionPayload.Operation);
            Assert.Equal(exception, connectionPayload.Exception);
        }

        [Fact]
        public void OnConnectionClosing_IsNotEnabled()
        {
            // arrange
            var connection = Mock.Of<DbConnection>();
            var source = new DbDiagnosticListener();

            // act
            var operationId = source.OnConnectionClosing(connection, "operation");

            // assert
            Assert.Equal(Guid.Empty, operationId);
        }

        [Fact]
        public void OnConnectionClosing()
        {
            // arrange
            var connection = Mock.Of<DbConnection>();
            var source = new DbDiagnosticListener();
            var operationId = Guid.Empty;

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () => { operationId = source.OnConnectionClosing(connection, "operation"); });

            // assert
            Assert.Equal("System.Data.Common.ConnectionClosing", eventName);
            var connectionPayload = Assert.IsType<ConnectionPayload>(payload);
            Assert.Equal(connection, connectionPayload.Connection);
            Assert.Equal(connection.GetGuid(), connectionPayload.ConnectionId);
            Assert.Equal(operationId, connectionPayload.OperationId);
            Assert.Equal("operation", connectionPayload.Operation);
            Assert.Null(connectionPayload.Exception);
        }

        [Fact]
        public void OnConnectionClosed()
        {
            // arrange
            var operationId = Guid.NewGuid();
            var connection = Mock.Of<DbConnection>();
            var source = new DbDiagnosticListener();

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () => source.OnConnectionClosed(operationId, connection, "operation"));

            // assert
            Assert.Equal("System.Data.Common.ConnectionClosed", eventName);
            var connectionPayload = Assert.IsType<ConnectionPayload>(payload);
            Assert.Equal(connection, connectionPayload.Connection);
            Assert.Equal(connection.GetGuid(), connectionPayload.ConnectionId);
            Assert.Equal(operationId, connectionPayload.OperationId);
            Assert.Equal("operation", connectionPayload.Operation);
            Assert.Null(connectionPayload.Exception);
        }

        [Fact]
        public void OmConnectionClosingError()
        {
            // arrange
            var operationId = Guid.NewGuid();
            var exception = Mock.Of<Exception>();
            var connection = Mock.Of<DbConnection>();
            var source = new DbDiagnosticListener();

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () => { source.OmConnectionClosingError(operationId, connection, exception, "operation"); });

            // assert
            Assert.Equal("System.Data.Common.ConnectionClosingError", eventName);
            var connectionPayload = Assert.IsType<ConnectionPayload>(payload);
            Assert.Equal(connection, connectionPayload.Connection);
            Assert.Equal(connection.GetGuid(), connectionPayload.ConnectionId);
            Assert.Equal(operationId, connectionPayload.OperationId);
            Assert.Equal("operation", connectionPayload.Operation);
            Assert.Equal(exception, connectionPayload.Exception);
        }

        [Fact]
        public void WriteOnCommandExecuting_IsNotEnabled()
        {
            // arrange
            var command = Mock.Of<DbCommand>();
            Mock.Get(command).Protected().SetupGet<DbConnection>("DbConnection").Returns(Mock.Of<DbConnection>());
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticListener();

            // act
            var operationId = source.OnCommandExecuting(command, transaction, "operation");

            // assert
            Assert.Equal(Guid.Empty, operationId);
        }

        [Fact]
        public void WriteOnCommandExecuting()
        {
            // arrange
            var connection = Mock.Of<DbConnection>();
            var command = Mock.Of<DbCommand>();
            Mock.Get(command).Protected().SetupGet<DbConnection>("DbConnection").Returns(connection);
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticListener();
            var operationId = Guid.Empty;

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () => { operationId = source.OnCommandExecuting(command, transaction, "operation"); });

            // assert
            Assert.Equal("System.Data.Common.CommandExecuting", eventName);
            var commandPayload = Assert.IsType<CommandPayload>(payload);
            Assert.Equal(command, commandPayload.Command);
            Assert.Equal(connection.GetGuid(), commandPayload.ConnectionId);
            Assert.Equal(operationId, commandPayload.OperationId);
            Assert.Equal("operation", commandPayload.Operation);
            Assert.Equal(transaction.GetId(), commandPayload.TransactionId);
            Assert.Null(commandPayload.Exception);
        }

        [Fact]
        public void OnCommandExecuted()
        {
            // arrange
            var operationId = Guid.NewGuid();
            var connection = Mock.Of<DbConnection>();
            var command = Mock.Of<DbCommand>();
            Mock.Get(command).Protected().SetupGet<DbConnection>("DbConnection").Returns(connection);
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticListener();

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () => source.OnCommandExecuted(operationId, command, transaction, "operation"));

            // assert
            Assert.Equal("System.Data.Common.CommandExecuted", eventName);
            var commandPayload = Assert.IsType<CommandPayload>(payload);
            Assert.Equal(command, commandPayload.Command);
            Assert.Equal(connection.GetGuid(), commandPayload.ConnectionId);
            Assert.Equal(operationId, commandPayload.OperationId);
            Assert.Equal("operation", commandPayload.Operation);
            Assert.Equal(transaction.GetId(), commandPayload.TransactionId);
            Assert.Null(commandPayload.Exception);
        }

        [Fact]
        public void OnCommandError()
        {
            // arrange
            var operationId = Guid.NewGuid();
            var exception = Mock.Of<Exception>();
            var connection = Mock.Of<DbConnection>();
            var command = Mock.Of<DbCommand>();
            Mock.Get(command).Protected().SetupGet<DbConnection>("DbConnection").Returns(connection);
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticListener();

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () => { source.OnCommandError(operationId, command, transaction, exception, "operation"); });

            // assert
            Assert.Equal("System.Data.Common.CommandExecutingError", eventName);
            var commandPayload = Assert.IsType<CommandPayload>(payload);
            Assert.Equal(command, commandPayload.Command);
            Assert.Equal(connection.GetGuid(), commandPayload.ConnectionId);
            Assert.Equal(operationId, commandPayload.OperationId);
            Assert.Equal("operation", commandPayload.Operation);
            Assert.Equal(transaction.GetId(), commandPayload.TransactionId);
            Assert.Equal(exception, commandPayload.Exception);
        }

        [Fact]
        public void OnTransactionCommitting_IsNotEnabled()
        {
            // arrange
            var isolationLevel = IsolationLevel.Chaos;
            var connection = Mock.Of<DbConnection>();
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticListener();

            // act
            var operationId = source.OnTransactionCommitting(isolationLevel, connection, transaction, "operation");

            // assert
            Assert.Equal(Guid.Empty, operationId);
        }

        [Fact]
        public void OnTransactionCommitting()
        {
            // arrange
            var isolationLevel = IsolationLevel.Chaos;
            var connection = Mock.Of<DbConnection>();
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticListener();
            var operationId = Guid.Empty;

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () =>
                {
                    operationId =
                        source.OnTransactionCommitting(isolationLevel, connection, transaction, "operation");
                });

            // assert
            Assert.Equal("System.Data.Common.TransactionCommitting", eventName);
            var transactionPayload = Assert.IsType<TransactionPayload>(payload);
            Assert.Equal(transaction.GetId(), transactionPayload.TransactionId);
            Assert.Equal(connection, transactionPayload.Connection);
            Assert.Equal(isolationLevel, transactionPayload.IsolationLevel);
            Assert.Equal(operationId, transactionPayload.OperationId);
            Assert.Equal("operation", transactionPayload.Operation);
            Assert.Null(transactionPayload.Exception);
        }

        [Fact]
        public void OnTransactionCommitted()
        {
            // arrange
            var isolationLevel = IsolationLevel.Chaos;
            var connection = Mock.Of<DbConnection>();
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticListener();
            var operationId = Guid.NewGuid();

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () =>
                {
                    source.OnTransactionCommitted(operationId, isolationLevel, connection, transaction,
                        "operation");
                });

            // assert
            Assert.Equal("System.Data.Common.TransactionCommitted", eventName);
            var transactionPayload = Assert.IsType<TransactionPayload>(payload);
            Assert.Equal(transaction.GetId(), transactionPayload.TransactionId);
            Assert.Equal(connection, transactionPayload.Connection);
            Assert.Equal(isolationLevel, transactionPayload.IsolationLevel);
            Assert.Equal(operationId, transactionPayload.OperationId);
            Assert.Equal("operation", transactionPayload.Operation);
            Assert.Null(transactionPayload.Exception);
        }

        [Fact]
        public void OnTransactionCommittingError()
        {
            // arrange
            var exception = Mock.Of<Exception>();
            var isolationLevel = IsolationLevel.Chaos;
            var connection = Mock.Of<DbConnection>();
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticListener();
            var operationId = Guid.NewGuid();

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () =>
                {
                    source.OnTransactionCommittingError(operationId, isolationLevel, connection, transaction, exception,
                        "operation");
                });

            // assert
            Assert.Equal("System.Data.Common.TransactionCommittingError", eventName);
            var transactionPayload = Assert.IsType<TransactionPayload>(payload);
            Assert.Equal(transaction.GetId(), transactionPayload.TransactionId);
            Assert.Equal(connection, transactionPayload.Connection);
            Assert.Equal(isolationLevel, transactionPayload.IsolationLevel);
            Assert.Equal(operationId, transactionPayload.OperationId);
            Assert.Equal("operation", transactionPayload.Operation);
            Assert.Equal(exception, transactionPayload.Exception);
        }

        [Fact]
        public void OnTransactionRollingBack_IsNotEnabled()
        {
            // arrange
            var isolationLevel = IsolationLevel.Chaos;
            var connection = Mock.Of<DbConnection>();
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticListener();

            // act
            var operationId =
                source.OnTransactionRollingBack(isolationLevel, connection, transaction, "operation");

            // assert
            Assert.Equal(Guid.Empty, operationId);
        }

        [Fact]
        public void OnTransactionRollingBack()
        {
            // arrange
            var isolationLevel = IsolationLevel.Chaos;
            var connection = Mock.Of<DbConnection>();
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticListener();
            var operationId = Guid.Empty;

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () =>
                {
                    operationId =
                        source.OnTransactionRollingBack(isolationLevel, connection, transaction, "operation");
                });

            // assert
            Assert.Equal("System.Data.Common.TransactionRollingBack", eventName);
            var transactionPayload = Assert.IsType<TransactionPayload>(payload);
            Assert.Equal(transaction.GetId(), transactionPayload.TransactionId);
            Assert.Equal(connection, transactionPayload.Connection);
            Assert.Equal(isolationLevel, transactionPayload.IsolationLevel);
            Assert.Equal(operationId, transactionPayload.OperationId);
            Assert.Equal("operation", transactionPayload.Operation);
            Assert.Null(transactionPayload.Exception);
        }

        [Fact]
        public void OnTransactionRolledBack()
        {
            // arrange
            var isolationLevel = IsolationLevel.Chaos;
            var connection = Mock.Of<DbConnection>();
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticListener();
            var operationId = Guid.NewGuid();

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () =>
                {
                    source.OnTransactionRolledBack(operationId, isolationLevel, connection, transaction,
                        "operation");
                });

            // assert
            Assert.Equal("System.Data.Common.TransactionRolledBack", eventName);
            var transactionPayload = Assert.IsType<TransactionPayload>(payload);
            Assert.Equal(transaction.GetId(), transactionPayload.TransactionId);
            Assert.Equal(connection, transactionPayload.Connection);
            Assert.Equal(isolationLevel, transactionPayload.IsolationLevel);
            Assert.Equal(operationId, transactionPayload.OperationId);
            Assert.Equal("operation", transactionPayload.Operation);
            Assert.Null(transactionPayload.Exception);
        }

        [Fact]
        public void OnTransactionRollingBackError()
        {
            // arrange
            var exception = Mock.Of<Exception>();
            var isolationLevel = IsolationLevel.Chaos;
            var connection = Mock.Of<DbConnection>();
            var transaction = Mock.Of<DbTransaction>();
            var source = new DbDiagnosticListener();
            var operationId = Guid.NewGuid();

            // act
            var (eventName, payload) = DbDiagnosticSession.Execute(source,
                () =>
                {
                    source.OnTransactionRollingBackError(operationId, isolationLevel, connection, transaction,
                        exception, "operation");
                });

            // assert
            Assert.Equal("System.Data.Common.TransactionRollingBackError", eventName);
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