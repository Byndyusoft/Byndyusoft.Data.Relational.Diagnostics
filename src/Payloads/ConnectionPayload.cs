using System;
using System.Data.Common;

namespace Microsoft.Data.Diagnostics.Payloads
{
    public class ConnectionPayload
    {
        public Guid OperationId { get; set; }

        public string? Operation { get; set; }

        public Guid? ConnectionId { get; set; }

        public DbConnection Connection { get; set; } = default!;

        public string? ClientVersion { get; set; }

        public long Timestamp { get; set; }

        public Exception? Exception { get; set; }
    }
}