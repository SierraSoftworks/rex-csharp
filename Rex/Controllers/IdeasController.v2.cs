using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Rex.Controllers
{
    [Area("v2")]
    [ApiController]
    public class IdeaV2Controller : IdeaController<Views.IdeaV2>
    {
        public IdeaV2Controller(Stores.IIdeaStore store, ILogger<IdeaV2Controller> logger) : base(store, logger) { }
    }
}
