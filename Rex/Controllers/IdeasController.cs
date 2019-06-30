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

        protected Guid DefaultUserCollection()
        {
            return Guid.ParseExact(this.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value ?? Guid.NewGuid().ToString("D"), "D");
        }

        [HttpGet]
        [Route("api/[area]/idea/{id:Guid}", Name = "GetIdea.[area]")]
        [Route("api/[area]/collection/{collection:Guid}/idea/{id:Guid}", Name = "GetIdeaByCollection.[area]")]
        [Authorize("Ideas.Read", Roles = "Administrator,User")]
        public virtual async Task<ActionResult<T>> Get(Guid id, Guid? collection)
        {
            var model = await this.Store.GetIdeaAsync(collection ?? this.DefaultUserCollection(), id);

            if (model == null)
                return NotFound();

            return Representer.ToView(model);
        }


        [HttpGet]
        [Route("api/[area]/idea/random", Name = "GetRandomIdea.[area]")]
        [Route("api/[area]/collection/{collection:Guid}/idea/random", Name = "GetRandomIdeaByCollection.[area]")]
        [Authorize("Ideas.Read", Roles = "Administrator,User")]
        public virtual async Task<ActionResult<T>> GetRandom(Guid? collection)
        {
            var model = await this.Store.GetRandomIdeaAsync(collection ?? this.DefaultUserCollection());

            if (model == null)
                return NotFound();

            return Representer.ToView(model);
        }


        [HttpPut]
        [Route("api/[area]/idea/{id:Guid}", Name = "UpdateIdea.[area]")]
        [Route("api/[area]/collection/{collection:Guid}/idea/{id:Guid}", Name = "UpdateIdeaByCollection.[area]")]
        [Authorize("Ideas.Write", Roles = "Administrator,User")]
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
                model.CollectionId = DefaultUserCollection();
            }

            model = await this.Store.StoreIdeaAsync(model);

            return Representer.ToView(model);
        }

        [HttpGet]
        [Route("api/[area]/ideas", Name = "GetIdeas.[area]")]
        [Route("api/[area]/collection/{collection:Guid}/ideas", Name = "GetIdeasByCollection.[area]")]
        [Authorize("Ideas.Read", Roles = "Administrator,User")]
        public virtual async Task<IEnumerable<T>> List(Guid? collection) =>
            (await this.Store.GetIdeasAsync(collection ?? this.DefaultUserCollection()).ToEnumerable()).Select(Representer.ToView);


        [HttpPost]
        [Route("api/[area]/ideas", Name = "CreateIdea.[area]")]
        [Route("api/[area]/collection/{collection:Guid}/ideas", Name = "CreateIdeaByCollection.[area]")]
        [Authorize("Ideas.Write", Roles = "Administrator,User")]
        public virtual async Task<ActionResult<T>> Add(T idea, Guid? collection)
        {
            var model = Representer.ToModel(idea);
            if (collection.HasValue)
            {
                model.CollectionId = collection.Value;
            }

            if (model.CollectionId == Guid.Empty)
            {
                model.CollectionId = this.DefaultUserCollection();
            }

            var addedIdea = await this.Store.StoreIdeaAsync(model);

            var area = this.RouteData.Values["area"];

            if (model.CollectionId == this.DefaultUserCollection())
                return this.CreatedAtRoute($"GetIdea.{area}", new { id = addedIdea.Id.ToString("N") }, Representer.ToView(addedIdea));
            else
                return this.CreatedAtRoute($"GetIdeaByCollection.{area}", new { collection = addedIdea.CollectionId.ToString("N"), id = addedIdea.Id.ToString("N") }, Representer.ToView(addedIdea));

        }

        [HttpDelete]
        [Route("api/[area]/idea/{id:Guid}", Name = "RemoveIdea.[area]")]
        [Route("api/[area]/collection/{collection:Guid}/idea/{id:Guid}", Name = "RemoveIdeaByCollection.[area]")]
        [Authorize("Ideas.Write", Roles = "Administrator")]
        public virtual async Task<ActionResult<T>> Delete(Guid id, Guid? collection)
        {
            var idea = await this.Store.GetIdeaAsync(collection ?? DefaultUserCollection(), id);
            if (idea == null)
            {
                return this.NotFound();
            }

            await this.Store.RemoveIdeaAsync(collection ?? DefaultUserCollection(), id);

            return Representer.ToView(idea);
        }
    }
}
