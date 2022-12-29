namespace Rex.Stores;

[SuppressMessage("Await.Warning", "CS1998", Justification = "This in-memory implementation doesn't await anything.")]
public sealed class MemoryCollectionStore : ICollectionStore, IDisposable
{
    private SemaphoreSlim lockSlim = new SemaphoreSlim(1);

    private Dictionary<Guid, Dictionary<Guid, Models.Collection>> _state = new Dictionary<Guid, Dictionary<Guid, Models.Collection>>();

    public async Task<Collection?> GetCollectionAsync(Guid userId, Guid collectionId)
    {
        try
        {
            await this.lockSlim.WaitAsync().ConfigureAwait(false);
            return this._state.GetValueOrDefault(userId)?.GetValueOrDefault(collectionId);
        }
        finally
        {
            this.lockSlim.Release();
        }
    }

    public async IAsyncEnumerable<Collection> GetCollectionsAsync(Guid userId)
    {
        try
        {
            await this.lockSlim.WaitAsync().ConfigureAwait(false);
            foreach (var assignment in this._state.GetValueOrDefault(userId)?.Values.ToArray() ?? Array.Empty<Collection>())
                yield return assignment;
        }
        finally
        {
            this.lockSlim.Release();
        }
    }

    public async Task<bool> RemoveCollectionAsync(Guid userId, Guid collectionId)
    {
        try
        {
            await this.lockSlim.WaitAsync().ConfigureAwait(false);

            return this._state.GetValueOrDefault(userId)?.Remove(collectionId) ?? false;
        }
        finally
        {
            this.lockSlim.Release();
        }
    }

    public async Task<Collection> StoreCollectionAsync(Collection collection)
    {
        if (collection is null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        try
        {
            await this.lockSlim.WaitAsync().ConfigureAwait(false);
            this._state[collection.PrincipalId] = this._state.GetValueOrDefault(collection.PrincipalId) ?? new Dictionary<Guid, Models.Collection>();
            this._state[collection.PrincipalId][collection.CollectionId] = collection;
            return collection;
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