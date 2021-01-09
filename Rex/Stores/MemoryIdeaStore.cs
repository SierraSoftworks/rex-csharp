using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Rex.Extensions;

namespace Rex.Stores
{
    [SuppressMessage("Await.Warning", "CS1997", Justification = "This in-memory implementation doesn't await anything.")]
    public sealed class MemoryIdeaStore : IIdeaStore, IDisposable
    {
        private readonly SemaphoreSlim lockSlim = new SemaphoreSlim(1);

        private Dictionary<Guid, Dictionary<Guid, Models.Idea>> _state = new Dictionary<Guid, Dictionary<Guid, Models.Idea>>();

        public async Task<Models.Idea?> GetIdeaAsync(Guid collection, Guid id)
        {
            try
            {
                await this.lockSlim.WaitAsync().ConfigureAwait(false);
                return this._state.GetValueOrDefault(collection)?.GetValueOrDefault(id);
            }
            finally
            {
                this.lockSlim.Release();
            }
        }

        public async Task<Models.Idea> StoreIdeaAsync(Models.Idea idea)
        {
            if (idea is null)
            {
                throw new ArgumentNullException(nameof(idea));
            }

            try
            {
                await this.lockSlim.WaitAsync().ConfigureAwait(false);
                this._state[idea.CollectionId] = this._state.GetValueOrDefault(idea.CollectionId) ?? new Dictionary<Guid, Models.Idea>();
                this._state[idea.CollectionId][idea.Id] = idea;
                return idea;
            }
            finally
            {
                this.lockSlim.Release();
            }
        }
        public async Task<bool> RemoveIdeaAsync(Guid collection, Guid id)
        {
            try
            {
                await this.lockSlim.WaitAsync().ConfigureAwait(false);

                return this._state.GetValueOrDefault(collection)?.Remove(id) ?? false;
            }
            finally
            {
                this.lockSlim.Release();
            }
        }

        public async IAsyncEnumerable<Models.Idea> GetIdeasAsync(Guid collection)
        {
            try
            {
                await this.lockSlim.WaitAsync().ConfigureAwait(false);
                foreach (var idea in this._state.GetValueOrDefault(collection)?.Values?.ToArray() ?? Array.Empty<Models.Idea>())
                    yield return idea;
            }
            finally
            {
                this.lockSlim.Release();
            }
        }

        public async IAsyncEnumerable<Models.Idea> GetIdeasAsync(Guid collection, Func<Models.Idea, bool> predicate)
        {
            try
            {
                await this.lockSlim.WaitAsync().ConfigureAwait(false);
                foreach (var idea in this._state.GetValueOrDefault(collection)?.Values?.Where(predicate)?.ToArray() ?? Array.Empty<Models.Idea>())
                    yield return idea;
            }
            finally
            {
                this.lockSlim.Release();
            }
        }

        public async Task<Models.Idea?> GetRandomIdeaAsync(Guid collection)
        {
            try
            {
                await this.lockSlim.WaitAsync().ConfigureAwait(false);
                return this._state.GetValueOrDefault(collection)?.Values?.RandomOrDefault(null);
            }
            finally
            {
                this.lockSlim.Release();
            }
        }

        public async Task<Models.Idea?> GetRandomIdeaAsync(Guid collection, Func<Models.Idea, bool> predicate)
        {
            try
            {
                await this.lockSlim.WaitAsync().ConfigureAwait(false);
                return this._state.GetValueOrDefault(collection)?.Values?.Where(predicate)?.RandomOrDefault(null);
            }
            finally
            {
                this.lockSlim.Release();
            }
        }

        public async Task ClearAsync()
        {
            try
            {
                await this.lockSlim.WaitAsync().ConfigureAwait(false);
                this._state.Clear();
            }
            finally
            {
                this.lockSlim.Release();
            }
        }

        public void Dispose()
        {
            this.lockSlim.Dispose();
        }
    }
}