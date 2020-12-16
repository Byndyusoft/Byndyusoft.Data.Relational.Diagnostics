using System;
using System.Data;
using System.Data.Common;

namespace Microsoft.Data.Diagnostics.Payloads
{
    public class TransactionPayload
    {
        public Guid OperationId { get; set; }

        public string Operation { get; set; } = default!;

        public IsolationLevel IsolationLevel { get; set; }

        public DbConnection Connection { get; set; } = default!;

        public long TransactionId { get; set; } = default!;

        public long Timestamp { get; set; }

        public Exception? Exception { get; set; }
    }
}