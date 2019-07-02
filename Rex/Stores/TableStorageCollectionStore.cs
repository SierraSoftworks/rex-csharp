using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rex.Models;
using Microsoft.WindowsAzure.Storage.Table;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Extensions.Logging;

namespace Rex.Stores
{
    public class TableStorageCollectionStore : ICollectionStore
    {
        public TableStorageCollectionStore(IConfiguration config, ILogger<TableStorageIdeaStore> logger)
        {
            var connectionString = config.GetConnectionString("StorageAccount");
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            this.table = tableClient.GetTableReference("collections");
            this.logger = logger;
            this.random = new Random();
        }

        private readonly CloudTable table;
        private readonly ILogger<TableStorageIdeaStore> logger;
        private readonly Random random;

        public async Task<Collection> GetCollectionAsync(Guid userId, Guid collectionId)
        {
            await table.CreateIfNotExistsAsync();

            var op = TableOperation.Retrieve<CollectionEntity>(userId.ToString("N"), collectionId.ToString("N"));
            var result = await table.ExecuteAsync(op);

            var assignment = result?.Result as CollectionEntity;

            return assignment?.Model;
        }

        public async IAsyncEnumerable<Collection> GetCollectionsAsync(Guid userId)
        {
            await table.CreateIfNotExistsAsync();

            var query = new TableQuery<CollectionEntity>().Where(
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.Equal,
                    userId.ToString("N")));

            var count = 0;
            TableContinuationToken continuationToken = null;
            do
            {
                var result = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                continuationToken = result.ContinuationToken;
                count += result.Results.Count;
                foreach (var idea in result.Results.Select(i => i.Model))
                    yield return idea;
            } while (continuationToken != null);

            this.logger.LogDebug("Fetched {Count} ideas for collection {CollectionID}", count, userId);
        }

        public async Task<bool> RemoveCollectionAsync(Guid userId, Guid collectionId)
        {
            await table.CreateIfNotExistsAsync();

            var op = TableOperation.Retrieve<CollectionEntity>(userId.ToString("N"), collectionId.ToString("N"));
            var result = await table.ExecuteAsync(op);

            var assignment = result?.Result as CollectionEntity;
            if (assignment == null)
            {
                return false;
            }

            op = TableOperation.Delete(assignment);
            result = await table.ExecuteAsync(op);

            return true;
        }

        public async Task<Collection> StoreCollectionAsync(Collection collection)
        {
            await table.CreateIfNotExistsAsync();

            var op = TableOperation.InsertOrReplace(new CollectionEntity(collection));
            var result = await table.ExecuteAsync(op);

            var assignmentResult = result?.Result as CollectionEntity;

            return assignmentResult?.Model;
        }

        private class CollectionEntity : TableEntity
        {
            public CollectionEntity()
            {

            }

            public CollectionEntity(Models.Collection collection)
            {
                this.PartitionKey = (collection.PrincipalId == Guid.Empty ? Guid.NewGuid() : collection.PrincipalId).ToString("N");
                this.RowKey = (collection.CollectionId == Guid.Empty ? Guid.NewGuid() : collection.CollectionId).ToString("N");
                this.Name = collection.Name;
            }

            public string Name { get; set; }

            public Models.Collection Model => new Models.Collection()
            {
                PrincipalId = Guid.ParseExact(this.PartitionKey, "N"),
                CollectionId = Guid.ParseExact(this.RowKey, "N"),
                Name = this.Name
            };
        }
    }
}