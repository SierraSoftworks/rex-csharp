namespace Rex.Stores;

public interface IIdeaStore
{
    Task<Models.Idea?> GetIdeaAsync(Guid collection, Guid id);

    Task<Models.Idea?> GetRandomIdeaAsync(Guid collection);

    Task<Models.Idea?> GetRandomIdeaAsync(Guid collection, Func<Models.Idea, bool> predicate);

    Task<Models.Idea> StoreIdeaAsync(Models.Idea idea);

    Task<bool> RemoveIdeaAsync(Guid collection, Guid id);

    IAsyncEnumerable<Models.Idea> GetIdeasAsync(Guid collection);

    IAsyncEnumerable<Models.Idea> GetIdeasAsync(Guid collection, Func<Models.Idea, bool> predicate);
}