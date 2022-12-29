namespace Rex.Controllers;

[Area("v3")]
[ApiController]
public class RoleAssignmentV3Controller : RoleAssignmentController<RoleAssignment.Version3>
{
    public RoleAssignmentV3Controller(IRoleAssignmentStore store, ICollectionStore collectionStore, IRepresenter<RoleAssignment, RoleAssignment.Version3> representer)
        : base(store, collectionStore, representer)
    {
    }
}
