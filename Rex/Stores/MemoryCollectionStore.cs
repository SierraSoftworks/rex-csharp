using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rex.Models;

namespace Rex.Stores
{
    public class MemoryCollectionStore : ICollectionStore
    {
        private ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim();

        private Dictionary<Guid, Dictionary<Guid, Models.Collection>> _state = new Dictionary<Guid, Dictionary<Guid, Models.Collection>>();

        public async Task<Collection> GetCollection(Guid userId, Guid collectionId)
        {
            try
            {
                this.lockSlim.EnterReadLock();
                return this._state.GetValueOrDefault(userId)?.GetValueOrDefault(collectionId);
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }

        public async IAsyncEnumerable<Collection> GetCollection(Guid userId)
        {
            try
            {
                this.lockSlim.EnterReadLock();
                foreach (var assignment in this._state.GetValueOrDefault(userId)?.Values.ToArray() ?? Array.Empty<Collection>())
                    yield return assignment;
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }

        public async Task<bool> RemoveCollectionAsync(Guid userId, Guid collectionId)
        {
            try
            {
                this.lockSlim.EnterReadLock();

                return this._state.GetValueOrDefault(userId)?.Remove(collectionId) ?? false;
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
                this._state[collection.PrincipalId] = this._state.GetValueOrDefault(collection.PrincipalId) ?? new Dictionary<Guid, Models.Collection>();
                this._state[collection.PrincipalId][collection.CollectionId] = collection;
                return collection;
            }
            finally
            {
                this.lockSlim.ExitWriteLock();
            }
        }
    }
}