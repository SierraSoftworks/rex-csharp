using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rex.Models;

namespace Rex.Stores
{
    public class MemoryCollectionStore : ICollectionStore
    {
        private ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim();

        private Dictionary<Guid, Dictionary<Guid, Models.Collection>> _state = new Dictionary<Guid, Dictionary<Guid, Models.Collection>>();

        public async Task<Collection> GetCollection(Guid collectionId, Guid userId)
        {
            try
            {
                this.lockSlim.EnterReadLock();
                return this._state.GetValueOrDefault(collectionId)?.GetValueOrDefault(userId);
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }

        public async IAsyncEnumerable<Collection> GetCollection(Guid collectionId)
        {
            try
            {
                this.lockSlim.EnterReadLock();
                foreach (var assignment in this._state.GetValueOrDefault(collectionId)?.Values)
                    yield return assignment;
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }

        public async Task<bool> RemoveCollectionAsync(Guid collectionId, Guid userId)
        {
            try
            {
                this.lockSlim.EnterReadLock();

                return this._state.GetValueOrDefault(collectionId)?.Remove(userId) ?? false;
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }

        public async Task<Collection> StoreCollectionAsync(Collection collection)
        {
            try
            {
                this.lockSlim.EnterWriteLock();
                this._state[collection.CollectionId] = this._state.GetValueOrDefault(collection.CollectionId) ?? new Dictionary<Guid, Models.Collection>();
                this._state[collection.CollectionId][collection.PrincipalId] = collection;
                return collection;
            }
            finally
            {
                this.lockSlim.ExitWriteLock();
            }
        }
    }
}