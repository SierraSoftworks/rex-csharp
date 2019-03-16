using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Randy.Stores
{
    public class IdeaStore
    {
        private ConcurrentDictionary<Guid, Models.Idea> _state = new ConcurrentDictionary<Guid, Models.Idea>();

        public async Task<Models.Idea> GetIdeaAsync(Guid id) => this._state[id];

        public async Task<Models.Idea> StoreIdeaAsync(Models.Idea idea) => this._state.AddOrUpdate(idea.Id, idea, (_id, _old) => idea);

        public async Task<IEnumerable<Models.Idea>> GetIdeasAsync() => this._state.Values;

        public async Task<IEnumerable<Models.Idea>> GetIdeasAsync(Func<Models.Idea, bool> predicate)
            => this._state.Values.Where(predicate);

        public async Task<Models.Idea> GetRandomIdeaAsync() => this._state.Values.RandomOrDefault(null);

        public async Task<Models.Idea> GetRandomIdeaAsync(Func<Models.Idea, bool> predicate) => this._state.Values.Where(predicate).Random();
    }
}