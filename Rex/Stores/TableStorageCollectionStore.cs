using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rex.Models;
using System.Linq;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using SierraLib.API.Views;
using Rex.Exceptions;

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
        private readonly IRepresenter<Collection, CollectionEntity> representer = new CollectionEntity.Representer();

        public async Task<Collection?> GetCollectionAsync(Guid userId, Guid collectionId)
        {
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);

            var op = TableOperation.Retrieve<CollectionEntity>(userId.ToString("N", CultureInfo.InvariantCulture), collectionId.ToString("N", CultureInfo.InvariantCulture));
            var result = await table.ExecuteAsync(op).ConfigureAwait(false);

            var assignment = result?.Result as CollectionEntity;

            return this.representer.ToModelOrDefault(assignment);
        }

        public async IAsyncEnumerable<Collection> GetCollectionsAsync(Guid userId)
        {
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);

            var query = new TableQuery<CollectionEntity>().Where(
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.Equal,
                    userId.ToString("N", CultureInfo.InvariantCulture)));

            var count = 0;
            TableContinuationToken? continuationToken = null;
            do
            {
                var result = await table.ExecuteQuerySegmentedAsync(query, continuationToken).ConfigureAwait(false);
                continuationToken = result.ContinuationToken;
                count += result.Results.Count;
                foreach (var idea in result.Results.Select(this.representer.ToModel))
                    yield return idea;
            } while (continuationToken != null);

            this.logger.LogDebug("Fetched {Count} ideas for collection {CollectionID}", count, userId);
        }

        public async Task<bool> RemoveCollectionAsync(Guid userId, Guid collectionId)
        {
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);

            var op = TableOperation.Retrieve<CollectionEntity>(userId.ToString("N", CultureInfo.InvariantCulture), collectionId.ToString("N", CultureInfo.InvariantCulture));
            var result = await table.ExecuteAsync(op).ConfigureAwait(false);

            if (!(result?.Result is CollectionEntity assignment))
            {
                return false;
            }

            op = TableOperation.Delete(assignment);
            await table.ExecuteAsync(op).ConfigureAwait(false);

            return true;
        }

        public async Task<Collection> StoreCollectionAsync(Collection collection)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            await table.CreateIfNotExistsAsync().ConfigureAwait(false);

            var op = TableOperation.InsertOrReplace(this.representer.ToView(collection));
            var result = await table.ExecuteAsync(op).ConfigureAwait(false);

            var assignmentResult = result?.Result as CollectionEntity;

            return this.representer.ToModelOrDefault(assignmentResult) ?? throw new Exception("Failed to store collection.");
        }

        private class CollectionEntity : TableEntity, IView<Collection>
        {
            public CollectionEntity()
            {

            }

            public string? Name { get; set; }

            public class Representer : IRepresenter<Collection, CollectionEntity>
            {
                public Collection ToModel(CollectionEntity view)
                {
                    return new Collection
                    {
                        PrincipalId = Guid.ParseExact(view.PartitionKey, "N"),
                        CollectionId = Guid.ParseExact(view.RowKey, "N"),
                        Name = view.Name ?? throw new RequiredFieldException(nameof(Collection), nameof(Collection.Name))
                    };
                }

                public CollectionEntity ToView(Collection model)
                {
                    return new CollectionEntity
                    {
                        PartitionKey = (model.PrincipalId == Guid.Empty ? Guid.NewGuid() : model.PrincipalId).ToString("N", CultureInfo.InvariantCulture),
                        RowKey = (model.CollectionId == Guid.Empty ? Guid.NewGuid() : model.CollectionId).ToString("N", CultureInfo.InvariantCulture),
                        Name = model.Name,
                    };
                }
            }
        }
    }
}