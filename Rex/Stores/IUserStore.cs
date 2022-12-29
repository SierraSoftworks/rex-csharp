namespace Rex.Stores;

public interface IUserStore
{
    Task<Models.User?> GetUserAsync(string emailHash);

    Task<Models.User> StoreUserAsync(Models.User user);
}