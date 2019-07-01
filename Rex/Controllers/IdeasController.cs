using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rex.Models;

namespace Rex.Controllers
{
    public abstract class IdeaController<T> : ControllerBase
        where T : class, IView<Idea>
    {
        private readonly ILogger<IdeaController<T>> logger;

        public IdeaController(Stores.IIdeaStore store, IRepresenter<Idea, T> representer, ILogger<IdeaController<T>> logger)
        {
            Store = store;
            Representer = representer;
            this.logger = logger;
        }

        protected Stores.IIdeaStore Store { get; }

        protected IRepresenter<Idea, T> Representer { get; }

        [HttpGet]
        [Route("api/[area]/idea/{id:Guid}", Name = "GetIdea.[area]")]
        [Route("api/[area]/collection/{collection:Guid}/idea/{id:Guid}", Name = "GetIdeaByCollection.[area]")]
        [Authorize(Scopes.IdeasRead, Roles = "Administrator,User")]
        public virtual async Task<ActionResult<T>> Get(Guid id, Guid? collection)
        {
            var model = await this.Store.GetIdeaAsync(collection ?? this.User.GetOid(), id);

            if (model == null)
                return NotFound();

            return Representer.ToView(model);
        }


        [HttpGet]
        [Route("api/[area]/idea/random", Name = "GetRandomIdea.[area]")]
        [Route("api/[area]/collection/{collection:Guid}/idea/random", Name = "GetRandomIdeaByCollection.[area]")]
        [Authorize(Scopes.IdeasRead, Roles = "Administrator,User")]
        public virtual async Task<ActionResult<T>> GetRandom(Guid? collection)
        {
            var model = await this.Store.GetRandomIdeaAsync(collection ?? this.User.GetOid());

            if (model == null)
                return NotFound();

            return Representer.ToView(model);
        }


        [HttpPut]
        [Route("api/[area]/idea/{id:Guid}", Name = "UpdateIdea.[area]")]
        [Route("api/[area]/collection/{collection:Guid}/idea/{id:Guid}", Name = "UpdateIdeaByCollection.[area]")]
        [Authorize(Scopes.IdeasWrite, Roles = "Administrator,User")]
        public virtual async Task<ActionResult<T>> Put(Guid id, [FromBody] T idea, Guid? collection)
        {
            if (idea == null)
            {
                return this.BadRequest();
            }

            var model = Representer.ToModel(idea);
            model.Id = id;
            model.CollectionId = collection ?? model.CollectionId;

            if (model.CollectionId == Guid.Empty)
            {
                model.CollectionId = this.User.GetOid();
            }

            model = await this.Store.StoreIdeaAsync(model);

            return Representer.ToView(model);
        }

        [HttpGet]
        [Route("api/[area]/ideas", Name = "GetIdeas.[area]")]
        [Route("api/[area]/collection/{collection:Guid}/ideas", Name = "GetIdeasByCollection.[area]")]
        [Authorize(Scopes.IdeasRead, Roles = "Administrator,User")]
        public virtual async Task<IEnumerable<T>> List(Guid? collection) =>
            (await this.Store.GetIdeasAsync(collection ?? this.User.GetOid()).ToEnumerable()).Select(Representer.ToView);


        [HttpPost]
        [Route("api/[area]/ideas", Name = "CreateIdea.[area]")]
        [Route("api/[area]/collection/{collection:Guid}/ideas", Name = "CreateIdeaByCollection.[area]")]
        [Authorize(Scopes.IdeasWrite, Roles = "Administrator,User")]
        public virtual async Task<ActionResult<T>> Add(T idea, Guid? collection)
        {
            var model = Representer.ToModel(idea);
            if (collection.HasValue)
            {
                model.CollectionId = collection.Value;
            }

            if (model.CollectionId == Guid.Empty)
            {
                model.CollectionId = this.User.GetOid();
            }

            var addedIdea = await this.Store.StoreIdeaAsync(model);

            var area = this.RouteData.Values["area"];

            if (model.CollectionId == this.User.GetOid())
                return this.CreatedAtRoute($"GetIdea.{area}", new { id = addedIdea.Id.ToString("N") }, Representer.ToView(addedIdea));
            else
                return this.CreatedAtRoute($"GetIdeaByCollection.{area}", new { collection = addedIdea.CollectionId.ToString("N"), id = addedIdea.Id.ToString("N") }, Representer.ToView(addedIdea));

        }

        [HttpDelete]
        [Route("api/[area]/idea/{id:Guid}", Name = "RemoveIdea.[area]")]
        [Route("api/[area]/collection/{collection:Guid}/idea/{id:Guid}", Name = "RemoveIdeaByCollection.[area]")]
        [Authorize(Scopes.IdeasWrite, Roles = "Administrator")]
        public virtual async Task<ActionResult> Delete(Guid id, Guid? collection)
        {
            if (!await this.Store.RemoveIdeaAsync(collection ?? this.User.GetOid(), id))
            {
                return this.NotFound();
            }

            return this.NoContent();
        }
    }
}
