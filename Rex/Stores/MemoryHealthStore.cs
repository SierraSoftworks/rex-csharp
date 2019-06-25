using System;
using System.Threading.Tasks;

namespace Rex.Stores
{
    public class MemoryHealthStore : IHealthStore
    {
        private Models.Health _state = new Models.Health
        {
            Ok = true,
            StartedAt = DateTime.UtcNow,
        };

        public Task<Models.Health> GetHealthStateAsync() => Task.FromResult(this._state);
    }
}