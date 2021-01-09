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
    public class TableStorageUserStore : IUserStore
    {
        public TableStorageUserStore(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("StorageAccount");
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            this.table = tableClient.GetTableReference("users");
        }

        private readonly CloudTable table;

        private readonly IRepresenter<User, UserEntity> representer = new UserEntity.Representer();

        public async Task<User?> GetUserAsync(string emailHash)
        {
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);

            var op = TableOperation.Retrieve<UserEntity>(emailHash, emailHash);
            var result = await table.ExecuteAsync(op).ConfigureAwait(false);

            var assignment = result?.Result as UserEntity;

            return this.representer.ToModelOrDefault(assignment);
        }

        public async Task<User> StoreUserAsync(User user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            await table.CreateIfNotExistsAsync().ConfigureAwait(false);

            var op = TableOperation.InsertOrReplace(this.representer.ToView(user));
            var result = await table.ExecuteAsync(op).ConfigureAwait(false);

            var assignmentResult = result?.Result as UserEntity;

            return this.representer.ToModelOrDefault(assignmentResult) ?? throw new Exception("Failed to store user.");
        }

        private class UserEntity : TableEntity, IView<User>
        {
            public UserEntity()
            {
            }

            public string? FirstName { get; set; }

            public string? EmailHash { get; set; }

            public class Representer : IRepresenter<User, UserEntity>
            {
                public User ToModel(UserEntity view)
                {
                    return new User
                    {
                        PrincipalId = Guid.ParseExact(view.PartitionKey, "N"),
                        EmailHash = view.EmailHash ?? throw new RequiredFieldException(nameof(User), nameof(User.EmailHash)),
                        FirstName = view.FirstName ?? throw new RequiredFieldException(nameof(User), nameof(User.FirstName))
                    };
                }

                public UserEntity ToView(User model)
                {
                    return new UserEntity
                    {
                        PartitionKey = (model.PrincipalId == Guid.Empty ? Guid.NewGuid() : model.PrincipalId).ToString("N", CultureInfo.InvariantCulture),
                        RowKey = (model.PrincipalId == Guid.Empty ? Guid.NewGuid() : model.PrincipalId).ToString("N", CultureInfo.InvariantCulture),
                        EmailHash = model.EmailHash,
                        FirstName = model.FirstName,
                    };
                }
            }
        }
    }
}