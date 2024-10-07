using Azure.Data.Tables;
using Azure;
using Azure.Data.Tables.Models;
using ProfileBot.Utilities;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace ProfileBot.Services
{
    public class AzureTableHelper
    {
        private TableServiceClient _tableServiceClient;
        private string tableName = "UserProfileTable";
        private TableClient _userIDMappingTable;
      
        public AzureTableHelper(string connectionString)
        {
            _tableServiceClient = new TableServiceClient(connectionString);
            _userIDMappingTable = _tableServiceClient.GetTableClient(tableName);
            _userIDMappingTable.CreateIfNotExists();
        }

        public async Task<bool> insertccbotstorageaccountAsync(string userId, string Token)
        {
            try
            {
                UserIDMappingEntity entity = new UserIDMappingEntity() { PartitionKey = "Proddd", RowKey = userId, Token = Token, UserID = userId };
                await _userIDMappingTable.UpsertEntityAsync(entity);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

       

        public string GetTokenIfExists(string userID)
        {
            try
            {
                Pageable<UserIDMappingEntity> queryResultsFilter = _userIDMappingTable.Query<UserIDMappingEntity>(filter: $"UserID eq '{userID}'");
                return queryResultsFilter?.FirstOrDefault()?.Token ?? null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<UserProfile> GetUserProfileAsync(string email)
        {
            try
            {
                var response = await _userIDMappingTable.GetEntityAsync<UserProfile>("UserProfilePartition", email);
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
            await _userIDMappingTable.UpsertEntityAsync(userProfile);
        }
    }
}
