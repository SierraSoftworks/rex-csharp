using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Randy.API.Controllers
{
    public abstract class IdeaController<T> : ControllerBase
        where T : class, Views.IModelView<Models.Idea>, Views.IModelSource<Models.Idea>, new()
    {
        public IdeaController(Stores.IdeaStore store) => Store = store;

        protected Stores.IdeaStore Store { get; }

        [HttpGet]
        [Route("api/v{version:apiVersion}/idea/{id:Guid}")]
        public virtual async Task<ActionResult<T>> Get(Guid id) => (await this.Store.GetIdeaAsync(id))?.ToView<T>()?.ToActionResult() ?? new NotFoundResult();


        [HttpGet]
        [Route("api/v{version:apiVersion}/idea/random")]
        public virtual async Task<ActionResult<T>> GetRandom() => (await this.Store.GetRandomIdeaAsync())?.ToView<T>()?.ToActionResult() ?? new NotFoundResult();


        [HttpPut]
        [Route("api/v{version:apiVersion}/idea/{id:Guid}")]
        public virtual async Task<T> Put(Guid id, Models.Idea idea)
        {
            idea.Id = id;
            idea = await this.Store.StoreIdeaAsync(idea);
            return idea.ToView<T>();
        }

        [HttpGet]
        [Route("api/v{version:apiVersion}/ideas")]
        public virtual async Task<IEnumerable<T>> List() => (await this.Store.GetIdeasAsync()).Select(x => x.ToView<T>());


        [HttpPost]
        [Route("api/v{version:apiVersion}/ideas")]
        public virtual async Task<T> Add(T idea) => (await this.Store.StoreIdeaAsync(idea.ToModel())).ToView<T>();
    }

    [ApiVersion("1.0")]
    [ApiController]
    public class IdeaV1Controller : IdeaController<Views.IdeaV1>
    {
        public IdeaV1Controller(Stores.IdeaStore store) : base(store) { }
    }

    [ApiVersion("2.0")]
    [ApiController]
    public class IdeaV2Controller : IdeaController<Views.IdeaV2>
    {
        public IdeaV2Controller(Stores.IdeaStore store) : base(store) { }
    }
}
