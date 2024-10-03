using Azure;
using Azure.Data.Tables;
using System;
using System.Threading.Tasks;

namespace ProfileBot
{
    public class AzureTableHelper
    {
        private readonly TableClient _tableClient;

        public AzureTableHelper(string storageConnectionString, string tableName)
        {
            var serviceClient = new TableServiceClient(storageConnectionString);
            _tableClient = serviceClient.GetTableClient(tableName);
            _tableClient.CreateIfNotExists();
        }

        public async Task<UserProfile> GetUserProfileAsync(string email)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<UserProfile>("UserProfilePartition", email);
                return response.Value;
            }
            catch (RequestFailedException)
            {
                return null; // If not found, return null
            }
        }

        public async Task SaveUserProfileAsync(UserProfile userProfile)
        {
            userProfile.RowKey = Guid.NewGuid().ToString();
            await _tableClient.UpsertEntityAsync(userProfile);
        }

        public async Task DeleteUserProfileAsync(string email)
        {
            await _tableClient.DeleteEntityAsync("UserProfilePartition", email);
        }
    }

}
