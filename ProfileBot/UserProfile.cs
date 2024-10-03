using Azure;
using Azure.Data.Tables;
using System;

namespace ProfileBot
{
    public class UserProfile : ITableEntity
    {
        public string PartitionKey { get; set; } = "UserProfilePartition"; // Default partition key
        public string RowKey { get; set; } // Use email or unique ID as RowKey
        public string Name { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public string ProfileImage { get; set; }

        // ITableEntity requires these properties
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
