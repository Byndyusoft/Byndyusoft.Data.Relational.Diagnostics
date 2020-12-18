using System;
using System.Data.Common;

namespace Microsoft.Data.Diagnostics
{
    public static class DiagnostingExtensions
    {
        /// <summary>
        ///     Adds <see cref="System.Diagnostics.DiagnosticSource" /> tracing to the
        ///     <param name="transaction">.</param>
        /// </summary>
        /// <param name="transaction">Instance of <see cref="DbTransaction" />.</param>
        /// <exception cref="ArgumentNullException">If
        ///     <param name="transaction"> is null.</param>
        /// </exception>
        /// <returns>Instance of <see cref="DbTransaction" /> with enabled tracing.</returns>
        public static DbTransaction AddDiagnosting(this DbTransaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            return transaction is DiagnosedDbTransaction ? transaction : new DiagnosedDbTransaction(transaction);
        }

        /// <summary>
        ///     Adds <see cref="System.Diagnostics.DiagnosticSource" /> tracing to the
        ///     <param name="connection">.</param>
        /// </summary>
        /// <param name="connection">Instance of <see cref="DbConnection" />.</param>
        /// <exception cref="ArgumentNullException">If
        ///     <param name="connection"> is null.</param>
        /// </exception>
        /// <returns>Instance of <see cref="DbConnection" /> with enabled tracing.</returns>
        public static DbConnection AddDiagnosting(this DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            return connection is DiagnosedDbConnection ? connection : new DiagnosedDbConnection(connection);
        }

        /// <summary>
        ///     Adds <see cref="System.Diagnostics.DiagnosticSource" /> tracing to the
        ///     <param name="command">.</param>
        /// </summary>
        /// <param name="command">Instance of <see cref="DbCommand" />.</param>
        /// <exception cref="ArgumentNullException">If
        ///     <param name="command"> is null.</param>
        /// </exception>
        /// <returns>Instance of <see cref="DbCommand" /> with enabled tracing.</returns>
        public static DbCommand AddDiagnosting(this DbCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            return command is DiagnosedDbCommand ? command : new DiagnosedDbCommand(command);
        }

        internal static DbTransaction Unwrap(this DbTransaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            return transaction is DiagnosedDbTransaction diagnosed ? diagnosed.Inner : transaction;
        }

        internal static DbConnection Unwrap(this DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            return connection is DiagnosedDbConnection diagnosed ? diagnosed.Inner : connection;
        }

        internal static Guid GetGuid(this DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            connection = connection.Unwrap();
            byte[] guidData = new byte[16];
            var typeHashCode = connection.GetType().GetHashCode();
            var connectionHashCode = connection.GetHashCode();
            var connectionStringHashCode = connection.ConnectionString?.GetHashCode() ?? 0;
            Array.Copy(BitConverter.GetBytes(typeHashCode), 0, guidData, 0, sizeof(int));
            Array.Copy(BitConverter.GetBytes(connectionHashCode), 0, guidData, sizeof(int), sizeof(int));
            Array.Copy(BitConverter.GetBytes(connectionStringHashCode), 0, guidData, sizeof(int) * 2, sizeof(int));
            return new Guid(guidData);
        }

        internal static long GetId(this DbTransaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            return transaction.Unwrap().GetHashCode();
        }
    }
}