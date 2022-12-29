namespace Rex.Stores;

[SuppressMessage("Await.Warning", "CS1998", Justification = "This in-memory implementation doesn't await anything.")]
public sealed class MemoryUserStore : IUserStore, IDisposable
{
    private SemaphoreSlim lockSlim = new SemaphoreSlim(1);

    private Dictionary<string, Models.User> _state = new Dictionary<string, Models.User>();

    public async Task<User?> GetUserAsync(string emailHash)
    {
        try
        {
            await this.lockSlim.WaitAsync().ConfigureAwait(false);
            return this._state.GetValueOrDefault(emailHash);
        }
        finally
        {
           this.lockSlim.Release(); 
        }
    }

    public async Task<User> StoreUserAsync(User user)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));

        try
        {
            await this.lockSlim.WaitAsync().ConfigureAwait(false);
            this._state[user.EmailHash] = user;
            return user;
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