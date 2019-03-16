using System;
using System.Threading.Tasks;

namespace Randy.API.Stores
{
    public class HealthStore
    {
        private Models.Health _state = new Models.Health
        {
            Ok = true,
            StartedAt = DateTime.UtcNow,
        };

        public Task<Models.Health> GetHealthStateAsync() => Task.FromResult(this._state);
    }
}