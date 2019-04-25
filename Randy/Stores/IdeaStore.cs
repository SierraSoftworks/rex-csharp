using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Randy.Stores
{
    public class IdeaStore
    {
        private ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim();

        private Dictionary<Guid, Models.Idea> _state = new Dictionary<Guid, Models.Idea>();

        public async Task<Models.Idea> GetIdeaAsync(Guid id)
        {
            try
            {
                this.lockSlim.EnterReadLock();
                return this._state[id];
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
                this._state[idea.Id] = idea;
                return idea;
            }
            finally
            {
                this.lockSlim.ExitWriteLock();
            }
        }
        public async Task<bool> RemoveIdeaAsync(Guid id)
        {
            try
            {
                this.lockSlim.EnterReadLock();

                return this._state.Remove(id);
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }

        public async Task<IEnumerable<Models.Idea>> GetIdeasAsync()
        {
            try
            {
                this.lockSlim.EnterReadLock();
                return this._state.Values.ToArray();
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }

        public async Task<IEnumerable<Models.Idea>> GetIdeasAsync(Func<Models.Idea, bool> predicate)
        {
            try
            {
                this.lockSlim.EnterReadLock();
                return this._state.Values.Where(predicate).ToArray();
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }

        public async Task<Models.Idea> GetRandomIdeaAsync()
        {
            try
            {
                this.lockSlim.EnterReadLock();
                return this._state.Values.RandomOrDefault(null);
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }

        public async Task<Models.Idea> GetRandomIdeaAsync(Func<Models.Idea, bool> predicate)
        {
            try
            {
                this.lockSlim.EnterReadLock();
                return this._state.Values.Where(predicate).RandomOrDefault(null);
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }
    }
}