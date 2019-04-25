using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Randy.Controllers
{
    [Authorize]
    public abstract class IdeaController<T> : ControllerBase
        where T : class, Views.IModelView<Models.Idea>, Views.IModelSource<Models.Idea>, new()
    {
        public IdeaController(Stores.IdeaStore store) => Store = store;

        protected Stores.IdeaStore Store { get; }

        [HttpGet]
        [Route("api/v{version:apiVersion}/idea/{id:Guid}", Name = "GetIdea")]
        [Authorize(Roles = "Administrator,User")]
        public virtual async Task<ActionResult<T>> Get(Guid id) => (await this.Store.GetIdeaAsync(id))?.ToView<T>()?.ToActionResult() ?? new NotFoundResult();


        [HttpGet]
        [Route("api/v{version:apiVersion}/idea/random")]
        [Authorize(Roles = "Administrator,User")]
        public virtual async Task<ActionResult<T>> GetRandom() => (await this.Store.GetRandomIdeaAsync())?.ToView<T>()?.ToActionResult() ?? new NotFoundResult();


        [HttpPut]
        [Route("api/v{version:apiVersion}/idea/{id:Guid}")]
        [Authorize(Roles = "Administrator,User")]
        public virtual async Task<ActionResult<T>> Put(Guid id, [FromBody] Models.Idea idea)
        {
            if (idea == null)
            {
                return this.BadRequest();
            }

            idea.Id = id;
            idea = await this.Store.StoreIdeaAsync(idea);
            return idea.ToView<T>();
        }

        [HttpGet]
        [Route("api/v{version:apiVersion}/ideas")]
        [Authorize(Roles = "Administrator,User")]
        public virtual async Task<IEnumerable<T>> List() => (await this.Store.GetIdeasAsync()).Select(x => x.ToView<T>());


        [HttpPost]
        [Route("api/v{version:apiVersion}/ideas")]
        [Authorize(Roles = "Administrator,User")]
        public virtual async Task<ActionResult<T>> Add(T idea)
        {
            var addedIdea = await this.Store.StoreIdeaAsync(idea.ToModel());

            return this.CreatedAtRoute("GetIdea", new { version = this.RouteData.Values["version"], id = addedIdea.Id.ToString("N") }, addedIdea.ToView<T>());
        }

        [HttpDelete]
        [Route("api/v{version:apiVersion}/idea/{id:Guid}")]
        [Authorize(Roles = "Administrator")]
        public virtual async Task<ActionResult<T>> Delete(Guid id)
        {
            var idea = await this.Store.GetIdeaAsync(id);
            if (idea == null)
            {
                return this.NotFound();
            }

            await this.Store.RemoveIdeaAsync(id);

            return idea.ToView<T>();
        }
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
