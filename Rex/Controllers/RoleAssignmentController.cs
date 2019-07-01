using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rex.Models;

namespace Rex.Controllers
{
    public abstract class RoleAssignmentController<T> : ControllerBase
        where T : IView<RoleAssignment>
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
            => (await Store.GetRoleAssignments(id).ToEnumerable()).Select(Representer.ToView);

        [HttpGet]
        [Route("api/[area]/collection/{collection:Guid}/user/{user:Guid}")]
        [Authorize(Scopes.RoleAssignmentsWrite, Roles = "Administrator,User")]
        public virtual async Task<ActionResult<T>> Get(Guid collection, Guid user)
            => Representer.ToViewSafe(await Store.GetRoleAssignment(user, collection)).ToActionResult() ?? this.NotFound();

        [HttpPut]
        [Route("api/[area]/collection/{collection:Guid}/user/{user:Guid}")]
        [Authorize(Scopes.RoleAssignmentsWrite, Roles = "Administrator,User")]
        public virtual async Task<ActionResult<T>> Replace(Guid collection, Guid user, [FromBody] T roleAssignment)
        {
            var role = await Store.GetRoleAssignment(collection, User.GetOid());
            if (role?.Role != RoleAssignment.Owner)
            {
                return this.Forbid();
            }

            var model = Representer.ToModel(roleAssignment);
            model.CollectionId = collection;
            model.PrincipalId = user;

            var added = await Store.StoreRoleAssignmentAsync(model);

            return Representer.ToView(added);
        }

        [HttpDelete]
        [Route("api/[area]/collection/{collection:Guid}/user/{user:Guid}")]
        [Authorize(Scopes.RoleAssignmentsWrite, Roles = "Administrator,User")]
        public virtual async Task<ActionResult> Remove(Guid collection, Guid user)
        {
            var role = await Store.GetRoleAssignment(collection, User.GetOid());
            if (role?.Role != RoleAssignment.Owner)
            {
                return this.Forbid();
            }

            if (!await Store.RemoveRoleAssignmentAsync(user, collection))
            {
                return this.NotFound();
            }

            return this.NoContent();
        }
    }
}
