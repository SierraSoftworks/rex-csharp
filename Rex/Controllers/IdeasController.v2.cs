using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rex.Models;
using Rex.Stores;
using SierraLib.API.Views;

namespace Rex.Controllers
{
    [Area("v2")]
    [ApiController]
    public class IdeaV2Controller : IdeaController<Idea.Version2>
    {
        public IdeaV2Controller(IIdeaStore store, IRoleAssignmentStore roleStore, IRepresenter<Idea, Idea.Version2> representer, ILogger<IdeaController<Idea.Version2>> logger) : base(store, roleStore, representer, logger)
        {
        }
    }
}
