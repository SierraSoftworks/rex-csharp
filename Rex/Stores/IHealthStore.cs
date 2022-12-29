namespace Rex.Stores;

public interface IHealthStore
{
    Task<Models.Health> GetHealthStateAsync();
}