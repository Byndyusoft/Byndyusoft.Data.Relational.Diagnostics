using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Diagnostics;
using Microsoft.Data.Diagnostics.Payloads;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Byndyusoft.Data.Relational.Diagnostics.Tests.Functional
{
    public class DbSessionAccessorFunctionalTests : IAsyncLifetime
    {
        private readonly string _connectionString = "Data Source=queries.db";

        public async Task InitializeAsync()
        {
            File.Delete("queries.db");

            await using var connection = new SqliteConnection(_connectionString);

            await connection.ExecuteAsync("CREATE TABLE test (id INT, name TEXT)");

            await connection.CloseAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Test()
        {
            // arrange
            var observer = new Observer();
            using var subscription = DiagnosticListener.AllListeners.Subscribe(observer);
            var isolationLevel = IsolationLevel.ReadCommitted;

            // act
            await using var connection = new SQLiteConnection(_connectionString).AddDiagnosting();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync(isolationLevel);
            await connection.QueryAsync("SELECT id, name from test", transaction: transaction);
            await transaction.CommitAsync();
            await connection.CloseAsync();

            // assert
            Assert.Equal(8, observer.Events.Count);

            {
                var first = observer.Events[0];
                Assert.Equal("System.Data.SQLite.WriteConnectionOpenBefore", first.Key);
                var before = Assert.IsType<ConnectionPayload>(first.Value);
                Assert.Equal(connection, before.Connection);

                var second = observer.Events[1];
                Assert.Equal("System.Data.SQLite.WriteConnectionOpenAfter", second.Key);
                var after = Assert.IsType<ConnectionPayload>(second.Value);
                Assert.Equal(connection, after.Connection);
                Assert.Equal(before.OperationId, after.OperationId);
                Assert.True(before.Timestamp < after.Timestamp);
            }

            {
                var third = observer.Events[2];
                Assert.Equal("System.Data.SQLite.WriteCommandBefore", third.Key);
                var before = Assert.IsType<CommandPayload>(third.Value);
                Assert.Equal(connection.GetId(), before.ConnectionId);
                Assert.Equal(transaction.GetId(), before.TransactionId);

                var fourth = observer.Events[3];
                Assert.Equal("System.Data.SQLite.WriteCommandAfter", fourth.Key);
                var after = Assert.IsType<CommandPayload>(fourth.Value);
                Assert.Equal(connection.GetId(), before.ConnectionId);
                Assert.Equal(transaction.GetId(), before.TransactionId);
                Assert.Equal(before.Command, after.Command);
                Assert.Equal(before.OperationId, after.OperationId);
                Assert.True(before.Timestamp < after.Timestamp);
            }

            {
                var fifth = observer.Events[4];
                Assert.Equal("System.Data.SQLite.WriteTransactionCommitBefore", fifth.Key);
                var before = Assert.IsType<TransactionPayload>(fifth.Value);
                Assert.Equal(connection, before.Connection);
                Assert.Equal(transaction.GetId(), before.TransactionId);
                Assert.Equal(isolationLevel, before.IsolationLevel);

                var sixth = observer.Events[5];
                Assert.Equal("System.Data.SQLite.WriteTransactionCommitAfter", sixth.Key);
                var after = Assert.IsType<TransactionPayload>(sixth.Value);
                Assert.Equal(connection, after.Connection);
                Assert.Equal(transaction.GetId(), after.TransactionId);
                Assert.Equal(isolationLevel, after.IsolationLevel);
                Assert.Equal(before.OperationId, after.OperationId);
                Assert.True(before.Timestamp < after.Timestamp);
            }

            {
                var seventh = observer.Events[6];
                Assert.Equal("System.Data.SQLite.WriteConnectionCloseBefore", seventh.Key);
                var before = Assert.IsType<ConnectionPayload>(seventh.Value);
                Assert.Equal(connection, before.Connection);

                var eighth = observer.Events[7];
                Assert.Equal("System.Data.SQLite.WriteConnectionCloseAfter", eighth.Key);
                var after = Assert.IsType<ConnectionPayload>(eighth.Value);
                Assert.Equal(connection, after.Connection);
                Assert.Equal(before.OperationId, after.OperationId);
                Assert.True(before.Timestamp < after.Timestamp);
            }
        }

        private sealed class Observer :
            IObserver<DiagnosticListener>,
            IObserver<KeyValuePair<string, object>>
        {
            private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

            public readonly List<KeyValuePair<string, object>> Events = new List<KeyValuePair<string, object>>();

            void IObserver<DiagnosticListener>.OnNext(DiagnosticListener diagnosticListener)
            {
                if (diagnosticListener.Name == nameof(DbDiagnosticSource))
                    _subscriptions.Add(diagnosticListener.Subscribe(this));
            }

            void IObserver<DiagnosticListener>.OnError(Exception error)
            {
            }

            void IObserver<DiagnosticListener>.OnCompleted()
            {
                _subscriptions.ForEach(x => x.Dispose());
                _subscriptions.Clear();
            }

            void IObserver<KeyValuePair<string, object>>.OnCompleted()
            {
            }

            void IObserver<KeyValuePair<string, object>>.OnError(Exception error)
            {
            }

            void IObserver<KeyValuePair<string, object>>.OnNext(KeyValuePair<string, object> value)
            {
                Events.Add(value);
            }
        }
    }
}