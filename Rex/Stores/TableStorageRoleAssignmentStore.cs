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

namespace Rex.Stores
{
    public class TableStorageRoleAssignmentStore : IRoleAssignmentStore
    {
        public TableStorageRoleAssignmentStore(IConfiguration config, ILogger<TableStorageIdeaStore> logger, IRepresenter<RoleAssignment, RoleAssignmentEntity> representer)
        {
            var connectionString = config.GetConnectionString("StorageAccount");
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            this.table = tableClient.GetTableReference("roleassignments");
            this.logger = logger;
            this.representer = representer;
            this.random = new Random();
        }

        private readonly CloudTable table;
        private readonly ILogger<TableStorageIdeaStore> logger;
        private readonly IRepresenter<RoleAssignment, RoleAssignmentEntity> representer;
        private readonly Random random;

        public async Task<RoleAssignment> GetRoleAssignment(Guid collectionId, Guid userId)
        {
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);

            var op = TableOperation.Retrieve<RoleAssignmentEntity>(collectionId.ToString("N", CultureInfo.InvariantCulture), userId.ToString("N", CultureInfo.InvariantCulture));
            var result = await table.ExecuteAsync(op).ConfigureAwait(false);

            var assignment = result?.Result as RoleAssignmentEntity;

            return this.representer.ToModelOrDefault(assignment);
        }

        public async IAsyncEnumerable<RoleAssignment> GetRoleAssignments(Guid collectionId)
        {
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);

            var query = new TableQuery<RoleAssignmentEntity>().Where(
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.Equal,
                    collectionId.ToString("N", CultureInfo.InvariantCulture)));

            var count = 0;
            TableContinuationToken continuationToken = null;
            do
            {
                var result = await table.ExecuteQuerySegmentedAsync(query, continuationToken).ConfigureAwait(false);
                continuationToken = result.ContinuationToken;
                count += result.Results.Count;
                foreach (var idea in result.Results.Select(i => this.representer.ToModel(i)))
                    yield return idea;
            } while (continuationToken != null);

            this.logger.LogDebug("Fetched {Count} ideas for user {UserId}", count, collectionId);
        }

        public async Task<bool> RemoveRoleAssignmentAsync(Guid collectionId, Guid userId)
        {
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);

            var op = TableOperation.Retrieve<RoleAssignmentEntity>(collectionId.ToString("N", CultureInfo.InvariantCulture), userId.ToString("N", CultureInfo.InvariantCulture));
            var result = await table.ExecuteAsync(op).ConfigureAwait(false);

            var assignment = result?.Result as RoleAssignmentEntity;
            if (assignment == null)
            {
                return false;
            }

            op = TableOperation.Delete(assignment);
            result = await table.ExecuteAsync(op).ConfigureAwait(false);

            return true;
        }

        public async Task<RoleAssignment> StoreRoleAssignmentAsync(RoleAssignment assignment)
        {
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);

            var op = TableOperation.InsertOrReplace(this.representer.ToView(assignment));
            var result = await table.ExecuteAsync(op).ConfigureAwait(false);

            var assignmentResult = result?.Result as RoleAssignmentEntity;

            return this.representer.ToModelOrDefault(assignmentResult);
        }

        public class RoleAssignmentEntity : TableEntity, IView<RoleAssignment>
        {
            public RoleAssignmentEntity()
            {

            }

            public string Role { get; set; }

            public class Representer : IRepresenter<RoleAssignment, RoleAssignmentEntity>
            {
                public RoleAssignment ToModel(RoleAssignmentEntity view)
                {
                    if (view is null)
                    {
                        throw new ArgumentNullException(nameof(view));
                    }

                    return new RoleAssignment
                    {
                        CollectionId = Guid.ParseExact(view.PartitionKey, "N"),
                        PrincipalId = Guid.ParseExact(view.RowKey, "N"),
                        Role = view.Role
                    };
                }

                public RoleAssignmentEntity ToView(RoleAssignment model)
                {
                    if (model is null)
                    {
                        throw new ArgumentNullException(nameof(model));
                    }

                    return new RoleAssignmentEntity
                    {
                        PartitionKey = (model.CollectionId == Guid.Empty ? Guid.NewGuid() : model.CollectionId).ToString("N", CultureInfo.InvariantCulture),
                        RowKey = (model.PrincipalId == Guid.Empty ? Guid.NewGuid() : model.PrincipalId).ToString("N", CultureInfo.InvariantCulture),
                        Role = model.Role,
                };
                }
            }
        }
    }
}