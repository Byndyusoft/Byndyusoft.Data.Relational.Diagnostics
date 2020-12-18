namespace Microsoft.Data.Diagnostics
{
    public partial class DbDiagnosticListener
    {
        public static class EventNames
        {
            // ReSharper disable InconsistentNaming
            private const string Prefix = "System.Data.Common.";

            public const string CommandExecuting = Prefix + nameof(CommandExecuting);
            public const string CommandExecuted = Prefix + nameof(CommandExecuted);
            public const string CommandExecutingError = Prefix + nameof(CommandExecutingError);

            public const string ConnectionOpening = Prefix + nameof(ConnectionOpening);
            public const string ConnectionOpened = Prefix + nameof(ConnectionOpened);
            public const string ConnectionOpeningError = Prefix + nameof(ConnectionOpeningError);

            public const string ConnectionClosing = Prefix + nameof(ConnectionClosing);
            public const string ConnectionClosed = Prefix + nameof(ConnectionClosed);
            public const string ConnectionClosingError = Prefix + nameof(ConnectionClosingError);

            public const string TransactionCommitting = Prefix + nameof(TransactionCommitting);
            public const string TransactionCommitted = Prefix + nameof(TransactionCommitted);
            public const string TransactionCommittingError = Prefix + nameof(TransactionCommittingError);

            public const string TransactionRollingBack = Prefix + nameof(TransactionRollingBack);
            public const string TransactionRolledBack = Prefix + nameof(TransactionRolledBack);

            public const string TransactionRollingBackError = Prefix + nameof(TransactionRollingBackError);
            // ReSharper enable InconsistentNaming
        }
    }
}