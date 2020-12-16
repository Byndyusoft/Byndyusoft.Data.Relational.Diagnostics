﻿using System;
using System.Data.Common;

namespace Microsoft.Data.Diagnostics
{
    public static class DiagnostingExtensions
    {
        public static DbTransaction AddDiagnosting(this DbTransaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            return transaction is DiagnosedDbTransaction ? transaction : new DiagnosedDbTransaction(transaction);
        }

        public static DbConnection AddDiagnosting(this DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            return connection is DiagnosedDbConnection ? connection : new DiagnosedDbConnection(connection);
        }

        public static DbCommand AddDiagnosting(this DbCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            return command is DiagnosedDbCommand ? command : new DiagnosedDbCommand(command);
        }
    }
}