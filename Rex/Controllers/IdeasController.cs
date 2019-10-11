using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rex.Models;
using SierraLib.API.Views;

namespace Rex.Controllers
{
    public abstract class IdeaController<T> : ControllerBase
        where T : class, IView<Idea>
    {
        private readonly ILogger<IdeaController<T>> logger;

        public IdeaController(Stores.IIdeaStore store, Stores.IRoleAssignmentStore roleStore, IRepresenter<Idea, T> representer, ILogger<IdeaController<T>> logger)
        {
            Store = store;
            RoleStore = roleStore;
            Representer = representer;
            this.logger = logger;
        }

        protected Stores.IIdeaStore Store { get; }

        protected Stores.IRoleAssignmentStore RoleStore { get; }

        protected IRepresenter<Idea, T> Representer { get; }

        [HttpGet]
        [Route("api/[area]/idea/{id:Guid}", Name = "GetIdea.[area]")]
        [Route("api/[area]/collection/{collection:Guid}/idea/{id:Guid}", Name = "GetIdeaByCollection.[area]")]
        [Authorize(Scopes.IdeasRead, Roles = "Administrator,User")]
        public virtual async Task<ActionResult<T>> GetIdea(Guid id, Guid? collection)
        {
            var role = await GetUserRoleOrCreateDefault(collection).ConfigureAwait(false);
            if (role?.Role == null)
            {
                return this.Forbid();
            }

            var model = await Store.GetIdeaAsync(collection ?? User.GetOid(), id).ConfigureAwait(false);

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
            var role = await GetUserRoleOrCreateDefault(collection).ConfigureAwait(false);
            if (role?.Role == null)
            {
                return this.Forbid();
            }

            var model = await Store.GetRandomIdeaAsync(collection ?? User.GetOid()).ConfigureAwait(false);

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

            var role = await RoleStore.GetRoleAssignment(model.CollectionId, User.GetOid()).ConfigureAwait(false);
            if ((role?.Role ?? RoleAssignment.Viewer) == RoleAssignment.Viewer)
            {
                return this.Forbid();
            }

            model = await Store.StoreIdeaAsync(model).ConfigureAwait(false);

            return Representer.ToView(model);
        }

        [HttpGet]
        [Route("api/[area]/ideas", Name = "GetIdeas.[area]")]
        [Route("api/[area]/collection/{collection:Guid}/ideas", Name = "GetIdeasByCollection.[area]")]
        [Authorize(Scopes.IdeasRead, Roles = "Administrator,User")]
        public virtual async Task<ActionResult<IEnumerable<T>>> List(Guid? collection)
        {
            var role = await GetUserRoleOrCreateDefault(collection).ConfigureAwait(false);
            if (role?.Role == null)
            {

                return this.Forbid();
            }

            return (await Store.GetIdeasAsync(collection ?? User.GetOid()).ToEnumerable().ConfigureAwait(false)).Select(Representer.ToView).ToActionResult() ?? this.NotFound();
        }


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

            var role = await GetUserRoleOrCreateDefault(collection).ConfigureAwait(false);
            if ((role?.Role ?? RoleAssignment.Viewer) == RoleAssignment.Viewer)
            {
                return this.Forbid();
            }

            var addedIdea = await Store.StoreIdeaAsync(model).ConfigureAwait(false);

            var area = this.RouteData.Values["area"];

            if (model.CollectionId == this.User.GetOid())
                return this.CreatedAtRoute($"GetIdea.{area}", new { id = addedIdea.Id.ToString("N", CultureInfo.InvariantCulture) }, Representer.ToView(addedIdea));
            else
                return this.CreatedAtRoute($"GetIdeaByCollection.{area}", new { collection = addedIdea.CollectionId.ToString("N", CultureInfo.InvariantCulture), id = addedIdea.Id.ToString("N", CultureInfo.InvariantCulture) }, Representer.ToView(addedIdea));

        }

        [HttpDelete]
        [Route("api/[area]/idea/{id:Guid}", Name = "RemoveIdea.[area]")]
        [Route("api/[area]/collection/{collection:Guid}/idea/{id:Guid}", Name = "RemoveIdeaByCollection.[area]")]
        [Authorize(Scopes.IdeasWrite, Roles = "Administrator,User")]
        public virtual async Task<ActionResult> Delete(Guid id, Guid? collection)
        {
            var role = await GetUserRoleOrCreateDefault(collection).ConfigureAwait(false);
            if ((role?.Role ?? RoleAssignment.Viewer) != RoleAssignment.Owner)
            {
                return this.Forbid();
            }

            if (!await Store.RemoveIdeaAsync(collection ?? User.GetOid(), id).ConfigureAwait(false))
            {
                return this.NotFound();
            }

            return this.NoContent();
        }

        protected async Task<RoleAssignment?> GetUserRoleOrCreateDefault(Guid? collection)
        {
            var role = await RoleStore.GetRoleAssignment(collection ?? User.GetOid(), User.GetOid()).ConfigureAwait(false);
            if (role is null && (collection is null || collection == User.GetOid()))
            {
                role = await RoleStore.StoreRoleAssignmentAsync(new RoleAssignment
                {
                    CollectionId = User.GetOid(),
                    PrincipalId = User.GetOid(),
                    Role = RoleAssignment.Owner
                }).ConfigureAwait(false);
            }

            return role;
        }
    }
}
