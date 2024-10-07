using Azure.Data.Tables;
using Azure;
using System;

namespace ProfileBot.Utilities
{
    public class UserIDMappingEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string UserID { get; set; }
        public string Token { get; set; }
    }
}
