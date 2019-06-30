using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rex.Stores
{
    public interface ICollectionStore
    {
        Task<Models.Collection> GetCollection(Guid collectionId, Guid userId);

        IAsyncEnumerable<Models.Collection> GetCollection(Guid collectionId);

        Task<Models.Collection> StoreCollectionAsync(Models.Collection collection);

        Task<bool> RemoveCollectionAsync(Guid collectionId, Guid userId);
    }
}