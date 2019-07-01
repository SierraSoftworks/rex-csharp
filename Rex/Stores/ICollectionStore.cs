using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rex.Stores
{
    public interface ICollectionStore
    {
        Task<Models.Collection> GetCollection(Guid userId, Guid collectionId);

        IAsyncEnumerable<Models.Collection> GetCollection(Guid userId);

        Task<Models.Collection> StoreCollectionAsync(Models.Collection collection);

        Task<bool> RemoveCollectionAsync(Guid userId, Guid collectionId);
    }
}