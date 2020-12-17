namespace Microsoft.Data.Diagnostics
{
    public partial class DbDiagnosticSource
    {
        public static class EventNames
        {
            public const string WriteCommandBefore = nameof(DbDiagnosticSource.WriteCommandBefore);
            public const string WriteCommandAfter = nameof(DbDiagnosticSource.WriteCommandAfter);
            public const string WriteCommandError = nameof(DbDiagnosticSource.WriteCommandError);

            public const string WriteConnectionOpenBefore = nameof(DbDiagnosticSource.WriteConnectionOpenBefore);
            public const string WriteConnectionOpenAfter = nameof(DbDiagnosticSource.WriteConnectionOpenAfter);
            public const string WriteConnectionOpenError = nameof(DbDiagnosticSource.WriteConnectionOpenError);

            public const string WriteConnectionCloseBefore = nameof(DbDiagnosticSource.WriteConnectionCloseBefore);
            public const string WriteConnectionCloseAfter = nameof(DbDiagnosticSource.WriteConnectionCloseAfter);
            public const string WriteConnectionCloseError = nameof(DbDiagnosticSource.WriteConnectionCloseError);

            public const string WriteTransactionCommitBefore = nameof(DbDiagnosticSource.WriteTransactionCommitBefore);
            public const string WriteTransactionCommitAfter = nameof(DbDiagnosticSource.WriteTransactionCommitAfter);
            public const string WriteTransactionCommitError = nameof(DbDiagnosticSource.WriteTransactionCommitError);

            public const string WriteTransactionRollbackBefore =
                nameof(DbDiagnosticSource.WriteTransactionRollbackBefore);

            public const string WriteTransactionRollbackAfter =
                nameof(DbDiagnosticSource.WriteTransactionRollbackAfter);

            public const string WriteTransactionRollbackError =
                nameof(DbDiagnosticSource.WriteTransactionRollbackError);
        }
    }
}