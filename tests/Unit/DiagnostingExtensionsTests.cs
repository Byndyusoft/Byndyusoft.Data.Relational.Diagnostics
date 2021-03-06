﻿using System;
using System.Data.Common;
using Microsoft.Data.Diagnostics;
using Moq;
using Xunit;

namespace Byndyusoft.Data.Relational.Diagnostics.Tests.Unit
{
    public class DiagnostingExtensionsTests
    {
        [Fact]
        public void AddDiagnosting_NullConnection_ThrowsException()
        {
            // arrange
            DbConnection connection = null!;

            // act
            var exception = Assert.Throws<ArgumentNullException>(() => connection.AddDiagnosting());

            // assert
            Assert.Equal("connection", exception.ParamName);
        }

        [Fact]
        public void AddDiagnosting_Connection()
        {
            // arrange
            var connection = Mock.Of<DbConnection>();

            // act
            var wrapped = connection.AddDiagnosting();

            // assert
            var diagnosedConnection = Assert.IsType<DiagnosedDbConnection>(wrapped);
            Assert.Equal(connection, diagnosedConnection.Inner);
        }

        [Fact]
        public void AddDiagnosting_NullTransaction_ThrowsException()
        {
            // arrange
            DbTransaction transaction = null!;

            // act
            var exception = Assert.Throws<ArgumentNullException>(() => transaction.AddDiagnosting());

            // assert
            Assert.Equal("transaction", exception.ParamName);
        }

        [Fact]
        public void AddDiagnosting_Transaction()
        {
            // arrange
            var transaction = Mock.Of<DbTransaction>();

            // act
            var wrapped = transaction.AddDiagnosting();

            // assert
            var diagnosedTransaction = Assert.IsType<DiagnosedDbTransaction>(wrapped);
            Assert.Equal(transaction, diagnosedTransaction.Inner);
        }

        [Fact]
        public void AddDiagnosting_NullCommand_ThrowsException()
        {
            // arrange
            DbCommand command = null!;

            // act
            var exception = Assert.Throws<ArgumentNullException>(() => command.AddDiagnosting());

            // assert
            Assert.Equal("command", exception.ParamName);
        }

        [Fact]
        public void AddDiagnosting_Command()
        {
            // arrange
            var transaction = Mock.Of<DbCommand>();

            // act
            var wrapped = transaction.AddDiagnosting();

            // assert
            var diagnosedCommand = Assert.IsType<DiagnosedDbCommand>(wrapped);
            Assert.Equal(transaction, diagnosedCommand.Inner);
        }
    }
}