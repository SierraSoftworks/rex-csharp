using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rex.Models;
using System.Linq;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SierraLib.API.Views;
using System.Globalization;
using Rex.Exceptions;
using Rex.Extensions;

namespace Rex.Stores
{
    public class TableStorageIdeaStore : IIdeaStore
    {
        public TableStorageIdeaStore(IConfiguration config, ILogger<TableStorageIdeaStore> logger, IRepresenter<Idea, IdeaEntity> representer)
        {
            var connectionString = config.GetConnectionString("StorageAccount");
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            this.table = tableClient.GetTableReference("ideas");
            this.logger = logger;
            this.representer = representer;
            this.random = new Random();
        }

        private readonly CloudTable table;
        private readonly ILogger<TableStorageIdeaStore> logger;
        private readonly IRepresenter<Idea, IdeaEntity> representer;
        private readonly Random random;

        public async Task<Idea?> GetIdeaAsync(Guid collection, Guid id)
        {
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);

            var op = TableOperation.Retrieve<IdeaEntity>(collection.ToString("N", CultureInfo.InvariantCulture), id.ToString("N", CultureInfo.InvariantCulture));
            var result = await table.ExecuteAsync(op).ConfigureAwait(false);

            var idea = result?.Result as IdeaEntity;

            return this.representer.ToModelOrDefault(idea);
        }

        public async IAsyncEnumerable<Idea> GetIdeasAsync(Guid collection)
        {
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);

            var query = new TableQuery<IdeaEntity>().Where(
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.Equal,
                    collection.ToString("N", CultureInfo.InvariantCulture)));

            var count = 0;
            TableContinuationToken? continuationToken = null;
            do
            {
                var result = await table.ExecuteQuerySegmentedAsync(query, continuationToken).ConfigureAwait(false);
                continuationToken = result.ContinuationToken;
                count += result.Results.Count;
                foreach (var idea in result.Results.Select(i => this.representer.ToModel(i)))
                    yield return idea;
            } while (continuationToken != null);

            this.logger.LogDebug("Fetched {Count} ideas for collection {Collection}", count, collection);
        }

        public IAsyncEnumerable<Idea> GetIdeasAsync(Guid collection, Func<Idea, bool> predicate)
        {
            return GetIdeasAsync(collection).Where(predicate);
        }

        public async Task<Idea?> GetRandomIdeaAsync(Guid collection)
        {
            return await GetIdeasAsync(collection).RandomOrDefaultWith(null, random).ConfigureAwait(false);
        }

        public async Task<Idea?> GetRandomIdeaAsync(Guid collection, Func<Idea, bool> predicate)
        {
            return await GetIdeasAsync(collection).Where(predicate).RandomOrDefaultWith(null, random).ConfigureAwait(false);
        }

        public async Task<bool> RemoveIdeaAsync(Guid collection, Guid id)
        {
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);

            var op = TableOperation.Retrieve<IdeaEntity>(collection.ToString("N", CultureInfo.InvariantCulture), id.ToString("N", CultureInfo.InvariantCulture));
            var result = await table.ExecuteAsync(op).ConfigureAwait(false);

            if (!(result?.Result is IdeaEntity idea))
            {
                return false;
            }

            op = TableOperation.Delete(idea);
            await table.ExecuteAsync(op).ConfigureAwait(false);

            return true;
        }

        public async Task<Idea> StoreIdeaAsync(Idea idea)
        {
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);

            var op = TableOperation.InsertOrReplace(this.representer.ToView(idea));
            var result = await table.ExecuteAsync(op).ConfigureAwait(false);

            var ideaResult = result?.Result as IdeaEntity;

            return this.representer.ToModelOrDefault(ideaResult) ?? throw new Exception("Failed to store idea.");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "This is required to represent the model.")]
        public class IdeaEntity : TableEntity, IView<Idea>
        {
            public IdeaEntity()
            {

            }

            public string? Name { get; set; }

            public string? Description { get; set; }

            public bool Completed { get; set; }

            public string? Tags { get; set; }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "This is required to represent the view.")]
            public class Representer : IRepresenter<Idea, IdeaEntity>
            {
                public Idea ToModel(IdeaEntity view)
                {
                    if (view is null)
                    {
                        throw new ArgumentNullException(nameof(view));
                    }

                    return new Idea
                    {
                        CollectionId = Guid.ParseExact(view.PartitionKey, "N"),
                        Id = Guid.ParseExact(view.RowKey, "N"),
                        Name = view.Name ?? throw new RequiredFieldException(nameof(Idea), nameof(Idea.Name)),
                        Description = view.Description ?? throw new RequiredFieldException(nameof(Idea), nameof(Idea.Description)),
                        Completed = view.Completed,
                        Tags = view.Tags?.Split(",")?.Select(t => t.Trim())?.Where(t => !string.IsNullOrEmpty(t))?.ToHashSet() ?? new HashSet<string>()
                    };
                }

                public IdeaEntity ToView(Idea model)
                {
                    if (model is null)
                    {
                        throw new ArgumentNullException(nameof(model));
                    }

                    return new IdeaEntity
                    {
                        PartitionKey = (model.CollectionId == Guid.Empty ? Guid.NewGuid() : model.CollectionId).ToString("N", CultureInfo.InvariantCulture),
                        RowKey = (model.Id == Guid.Empty ? Guid.NewGuid() : model.Id).ToString("N", CultureInfo.InvariantCulture),
                        Name = model.Name,
                        Description = model.Description,
                        Completed = model.Completed,
                        Tags = string.Join(',', model.Tags),
                    };
                }
            }
        }
    }
}