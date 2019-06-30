using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rex.Models;
using Rex.Stores;

namespace Rex.Controllers
{
    [Area("v1")]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public class IdeaV1Controller : IdeaController<Idea.Version1>
    {
        public IdeaV1Controller(IIdeaStore store, IRepresenter<Idea, Idea.Version1> representer, ILogger<IdeaController<Idea.Version1>> logger) : base(store, representer, logger)
        {
        }
    }
}
