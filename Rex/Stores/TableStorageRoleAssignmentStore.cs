using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rex.Models;
using System.Linq;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Rex.Stores
{
    public class TableStorageRoleAssignmentStore : IRoleAssignmentStore
    {
        public TableStorageRoleAssignmentStore(IConfiguration config, ILogger<TableStorageIdeaStore> logger)
        {
            var connectionString = config.GetConnectionString("StorageAccount");
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            this.table = tableClient.GetTableReference("roleassignments");
            this.logger = logger;
            this.random = new Random();
        }

        private readonly CloudTable table;
        private readonly ILogger<TableStorageIdeaStore> logger;
        private readonly Random random;

        public async Task<RoleAssignment> GetRoleAssignment(Guid collectionId, Guid userId)
        {
            await table.CreateIfNotExistsAsync();

            var op = TableOperation.Retrieve<RoleAssignmentEntity>(collectionId.ToString("N"), userId.ToString("N"));
            var result = await table.ExecuteAsync(op);

            var assignment = result?.Result as RoleAssignmentEntity;

            return assignment?.Model;
        }

        public async IAsyncEnumerable<RoleAssignment> GetRoleAssignments(Guid collectionId)
        {
            await table.CreateIfNotExistsAsync();

            var query = new TableQuery<RoleAssignmentEntity>().Where(
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.Equal,
                    collectionId.ToString("N")));

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

            this.logger.LogDebug("Fetched {Count} ideas for user {UserId}", count, collectionId);
        }

        public async Task<bool> RemoveRoleAssignmentAsync(Guid collectionId, Guid userId)
        {
            await table.CreateIfNotExistsAsync();

            var op = TableOperation.Retrieve<RoleAssignmentEntity>(collectionId.ToString("N"), userId.ToString("N"));
            var result = await table.ExecuteAsync(op);

            var assignment = result?.Result as RoleAssignmentEntity;
            if (assignment == null)
            {
                return false;
            }

            op = TableOperation.Delete(assignment);
            result = await table.ExecuteAsync(op);

            return true;
        }

        public async Task<RoleAssignment> StoreRoleAssignmentAsync(RoleAssignment assignment)
        {
            await table.CreateIfNotExistsAsync();

            var op = TableOperation.InsertOrReplace(new RoleAssignmentEntity(assignment));
            var result = await table.ExecuteAsync(op);

            var assignmentResult = result?.Result as RoleAssignmentEntity;

            return assignmentResult?.Model;
        }

        private class RoleAssignmentEntity : TableEntity
        {
            public RoleAssignmentEntity()
            {

            }

            public RoleAssignmentEntity(Models.RoleAssignment assignment)
            {
                this.PartitionKey = (assignment.CollectionId == Guid.Empty ? Guid.NewGuid() : assignment.CollectionId).ToString("N");
                this.RowKey = (assignment.PrincipalId == Guid.Empty ? Guid.NewGuid() : assignment.PrincipalId).ToString("N");
                this.Role = assignment.Role;
            }

            public string Role { get; set; }

            public Models.RoleAssignment Model => new Models.RoleAssignment()
            {
                CollectionId = Guid.ParseExact(this.PartitionKey, "N"),
                PrincipalId = Guid.ParseExact(this.RowKey, "N"),
                Role = this.Role
            };
        }
    }
}