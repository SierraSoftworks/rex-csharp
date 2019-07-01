using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rex.Models;
using Rex.Stores;

namespace Rex.Controllers
{
    [Area("v3")]
    [ApiController]
    public class CollectionV3Controller : CollectionController<Collection.Version3>
    {
        public CollectionV3Controller(
            ICollectionStore store,
            IRoleAssignmentStore roleStore,
            IRepresenter<Collection, Collection.Version3> representer) : base(store, roleStore, representer)
        {
        }
    }
}
