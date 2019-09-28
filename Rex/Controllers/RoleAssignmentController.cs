using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rex.Models;
using SierraLib.API.Views;

namespace Rex.Controllers
{
    public abstract class RoleAssignmentController<T> : ControllerBase
        where T : class, IView<RoleAssignment>
    {
        public RoleAssignmentController(Stores.IRoleAssignmentStore store, Stores.ICollectionStore collectionStore, IRepresenter<RoleAssignment, T> representer)
        {
            Store = store;
            CollectionStore = collectionStore;
            Representer = representer;
        }

        protected IRepresenter<RoleAssignment, T> Representer { get; }

        protected Stores.IRoleAssignmentStore Store { get; }

        protected Stores.ICollectionStore CollectionStore { get; }

        [HttpGet]
        [Route("api/[area]/collection/{id:Guid}/users", Name = "GetCollectionRoleAssignments.[area]")]
        [Authorize(Scopes.RoleAssignmentsWrite, Roles = "Administrator,User")]
        public virtual async Task<IEnumerable<T>> GetUsers(Guid id)
            => (await Store.GetRoleAssignments(id).ToEnumerable().ConfigureAwait(false)).Select(Representer.ToView);

        [HttpGet]
        [Route("api/[area]/collection/{collection:Guid}/user/{user:Guid}")]
        [Authorize(Scopes.RoleAssignmentsWrite, Roles = "Administrator,User")]
        public virtual async Task<ActionResult<T>> GetRoleAssignment(Guid collection, Guid user)
            => Representer.ToViewOrDefault(await Store.GetRoleAssignment(user, collection).ConfigureAwait(false)).ToActionResult() ?? this.NotFound();

        [HttpPut]
        [Route("api/[area]/collection/{collection:Guid}/user/{user:Guid}")]
        [Authorize(Scopes.RoleAssignmentsWrite, Roles = "Administrator,User")]
        public virtual async Task<ActionResult<T>> Replace(Guid collection, Guid user, [FromBody] T roleAssignment)
        {
            var role = await Store.GetRoleAssignment(collection, User.GetOid()).ConfigureAwait(false);
            if (role?.Role != RoleAssignment.Owner)
            {
                return this.Forbid();
            }

            var model = Representer.ToModel(roleAssignment);
            model.CollectionId = collection;
            model.PrincipalId = user;

            var added = await Store.StoreRoleAssignmentAsync(model).ConfigureAwait(false);

            return Representer.ToView(added);
        }

        [HttpDelete]
        [Route("api/[area]/collection/{collection:Guid}/user/{user:Guid}")]
        [Authorize(Scopes.RoleAssignmentsWrite, Roles = "Administrator,User")]
        public virtual async Task<ActionResult> Remove(Guid collection, Guid user)
        {
            var role = await Store.GetRoleAssignment(collection, User.GetOid()).ConfigureAwait(false);
            if (role?.Role != RoleAssignment.Owner)
            {
                return this.Forbid();
            }

            if (!await Store.RemoveRoleAssignmentAsync(user, collection).ConfigureAwait(false))
            {
                return this.NotFound();
            }

            return this.NoContent();
        }
    }
}
