using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Rex.Controllers
{
    [Area("v3")]
    [ApiController]
    public class IdeaV3Controller : IdeaController<Views.IdeaV3>
    {
        public IdeaV3Controller(Stores.IIdeaStore store, ILogger<IdeaV3Controller> logger) : base(store, logger) { }
    }
}
