using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Rex.Stores
{
    public class MemoryIdeaStore : IIdeaStore
    {
        private ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim();

        private Dictionary<Guid, Dictionary<Guid, Models.Idea>> _state = new Dictionary<Guid, Dictionary<Guid, Models.Idea>>();

        public async Task<Models.Idea> GetIdeaAsync(Guid collection, Guid id)
        {
            try
            {
                this.lockSlim.EnterReadLock();
                return this._state.GetValueOrDefault(collection)?.GetValueOrDefault(id);
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }

        public async Task<Models.Idea> StoreIdeaAsync(Models.Idea idea)
        {
            try
            {
                this.lockSlim.EnterWriteLock();
                this._state[idea.CollectionId] = this._state.GetValueOrDefault(idea.CollectionId) ?? new Dictionary<Guid, Models.Idea>();
                this._state[idea.CollectionId][idea.Id] = idea;
                return idea;
            }
            finally
            {
                this.lockSlim.ExitWriteLock();
            }
        }
        public async Task<bool> RemoveIdeaAsync(Guid collection, Guid id)
        {
            try
            {
                this.lockSlim.EnterReadLock();

                return this._state.GetValueOrDefault(collection)?.Remove(id) ?? false;
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }

        public async IAsyncEnumerable<Models.Idea> GetIdeasAsync(Guid collection)
        {
            try
            {
                this.lockSlim.EnterReadLock();
                foreach (var idea in this._state.GetValueOrDefault(collection)?.Values?.ToArray() ?? Array.Empty<Models.Idea>())
                {
                    yield return idea;
                }
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }

        public async IAsyncEnumerable<Models.Idea> GetIdeasAsync(Guid collection, Func<Models.Idea, bool> predicate)
        {
            try
            {
                this.lockSlim.EnterReadLock();
                foreach (var idea in this._state.GetValueOrDefault(collection)?.Values?.Where(predicate)?.ToArray() ?? Array.Empty<Models.Idea>())
                {
                    yield return idea;
                }
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }

        public async Task<Models.Idea> GetRandomIdeaAsync(Guid collection)
        {
            try
            {
                this.lockSlim.EnterReadLock();
                return this._state.GetValueOrDefault(collection)?.Values?.RandomOrDefault(null);
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }

        public async Task<Models.Idea> GetRandomIdeaAsync(Guid collection, Func<Models.Idea, bool> predicate)
        {
            try
            {
                this.lockSlim.EnterReadLock();
                return this._state.GetValueOrDefault(collection)?.Values?.Where(predicate)?.RandomOrDefault(null);
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }
    }
}