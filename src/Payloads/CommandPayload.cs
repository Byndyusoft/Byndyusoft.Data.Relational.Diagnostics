using System;
using System.Data.Common;

namespace Microsoft.Data.Diagnostics.Payloads
{
    public class CommandPayload
    {
        public Guid OperationId { get; set; }

        public string Operation { get; set; } = default!;

        public Guid ConnectionId { get; set; }

        public DbCommand Command { get; set; } = default!;

        public long? TransactionId { get; set; }

        public long Timestamp { get; set; }

        public Exception? Exception { get; set; }
    }
}