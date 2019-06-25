using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Rex.Controllers
{
    [Area("v2")]
    [Route("api/[area]/health")]
    [ApiController]
    public class HealthV2Controller : HealthController<Views.HealthV2>
    {
        public HealthV2Controller(Stores.IHealthStore store) : base(store) { }
    }
}
