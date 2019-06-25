using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Rex.Controllers
{
    [Area("v1")]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public class IdeaV1Controller : IdeaController<Views.IdeaV1>
    {
        public IdeaV1Controller(Stores.IIdeaStore store, ILogger<IdeaV1Controller> logger) : base(store, logger) { }
    }
}
