namespace Rex.Controllers;

public abstract class RoleAssignmentController<T> : ControllerBase
    where T : class, IView<RoleAssignment>
{
    protected RoleAssignmentController(Stores.IRoleAssignmentStore store, Stores.ICollectionStore collectionStore, IRepresenter<RoleAssignment, T> representer)
    {
        Store = store;
        CollectionStore = collectionStore;
        Representer = representer;
    }

    protected IRepresenter<RoleAssignment, T> Representer { get; }

    protected Stores.IRoleAssignmentStore Store { get; }

    protected Stores.ICollectionStore CollectionStore { get; }

    [HttpGet]
    [Route("api/[area]/collection/{collection:Guid}/users", Name = "GetCollectionRoleAssignments.[area]")]
    [Authorize(Scopes.RoleAssignmentsWrite, Roles = "Administrator,User")]
    public virtual async Task<ActionResult<IEnumerable<T>>> GetUsers(Guid collection)
    {
        var role = await Store.GetRoleAssignment(collection, User.GetOid()).ConfigureAwait(false);
        if (role?.Role != RoleAssignment.Owner)
        {
            return this.Forbid();
        }

        return (await Store.GetRoleAssignments(collection).ToEnumerable().ConfigureAwait(false)).Select(Representer.ToView).ToActionResult() ?? this.NotFound();
    }

    [HttpGet]
    [Route("api/[area]/collection/{collection:Guid}/user/{user:Guid}")]
    [Authorize(Scopes.RoleAssignmentsWrite, Roles = "Administrator,User")]
    public virtual async Task<ActionResult<T>> GetRoleAssignment(Guid collection, Guid user)
    {
        var role = await Store.GetRoleAssignment(collection, User.GetOid()).ConfigureAwait(false);
        if (role?.Role != RoleAssignment.Owner)
        {
            return this.Forbid();
        }

        return Representer.ToViewOrDefault(await Store.GetRoleAssignment(user, collection).ConfigureAwait(false)).ToActionResult() ?? this.NotFound();
    }

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
