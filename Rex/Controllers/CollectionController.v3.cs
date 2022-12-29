﻿namespace Rex.Controllers;

[Area("v3")]
[ApiController]
public class CollectionV3Controller : CollectionController<Collection.Version3>
{
    public CollectionV3Controller(
        ICollectionStore collectionStore,
        IRoleAssignmentStore roleStore,
        IUserStore userStore,
        IRepresenter<Collection, Collection.Version3> representer) : base(collectionStore, roleStore, userStore, representer)
    {
    }
}
