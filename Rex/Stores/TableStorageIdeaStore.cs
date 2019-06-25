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
    public class TableStorageIdeaStore : IIdeaStore
    {
        public TableStorageIdeaStore(IConfiguration config, ILogger<TableStorageIdeaStore> logger)
        {
            var connectionString = config.GetConnectionString("StorageAccount");
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            this.table = tableClient.GetTableReference("ideas");
            this.logger = logger;
            this.random = new Random();
        }

        private readonly CloudTable table;
        private readonly ILogger<TableStorageIdeaStore> logger;
        private readonly Random random;

        public async Task<Idea> GetIdeaAsync(Guid collection, Guid id)
        {
            await table.CreateIfNotExistsAsync();

            var op = TableOperation.Retrieve<IdeaEntity>(collection.ToString("N"), id.ToString("N"));
            var result = await table.ExecuteAsync(op);

            var idea = result?.Result as IdeaEntity;

            return idea?.Model;
        }

        public async IAsyncEnumerable<Idea> GetIdeasAsync(Guid collection)
        {
            await table.CreateIfNotExistsAsync();

            var query = new TableQuery<IdeaEntity>().Where(
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.Equal,
                    collection.ToString("N")));

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

            this.logger.LogDebug("Fetched {Count} ideas for collection {Collection}", count, collection);
        }

        public IAsyncEnumerable<Idea> GetIdeasAsync(Guid collection, Func<Idea, bool> predicate)
        {
            return GetIdeasAsync(collection).Where(predicate);
        }

        public async Task<Idea> GetRandomIdeaAsync(Guid collection)
        {
            return await GetIdeasAsync(collection).RandomOrDefaultWith(null, this.random);
        }

        public async Task<Idea> GetRandomIdeaAsync(Guid collection, Func<Idea, bool> predicate)
        {
            return await GetIdeasAsync(collection).Where(predicate).RandomOrDefaultWith(null, this.random);
        }

        public async Task<bool> RemoveIdeaAsync(Guid collection, Guid id)
        {
            await table.CreateIfNotExistsAsync();

            var op = TableOperation.Retrieve<IdeaEntity>(collection.ToString("N"), id.ToString("N"));
            var result = await table.ExecuteAsync(op);

            var idea = result?.Result as IdeaEntity;
            if (idea == null)
            {
                return false;
            }

            op = TableOperation.Delete(idea);
            result = await table.ExecuteAsync(op);

            return true;
        }

        public async Task<Idea> StoreIdeaAsync(Idea idea)
        {
            await table.CreateIfNotExistsAsync();

            var op = TableOperation.InsertOrReplace(new IdeaEntity(idea));
            var result = await table.ExecuteAsync(op);

            var ideaResult = result?.Result as IdeaEntity;

            return ideaResult?.Model;
        }

        private class IdeaEntity : TableEntity
        {
            public IdeaEntity()
            {

            }

            public IdeaEntity(Models.Idea idea)
            {
                this.PartitionKey = (idea.CollectionId == Guid.Empty ? Guid.NewGuid() : idea.CollectionId).ToString("N");
                this.RowKey = (idea.Id == Guid.Empty ? Guid.NewGuid() : idea.Id).ToString("N");
                this.Name = idea.Name;
                this.Description = idea.Description;
                this.Completed = idea.Completed;
                this.Tags = string.Join(',', idea.Tags);
            }

            public string Name { get; set; }

            public string Description { get; set; }

            public bool Completed { get; set; }

            public string Tags { get; set; }

            public Models.Idea Model => new Idea()
            {
                CollectionId = Guid.ParseExact(this.PartitionKey, "N"),
                Id = Guid.ParseExact(this.RowKey, "N"),
                Name = this.Name,
                Description = this.Description,
                Completed = this.Completed,
                Tags = this.Tags.Split(",").Select(t => t.Trim()).ToHashSet()
            };
        }
    }
}