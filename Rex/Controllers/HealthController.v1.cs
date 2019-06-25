using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Rex.Controllers
{
    [Area("v1")]
    [Route("api/[area]/health")]
    [ApiController]
    public class HealthV1Controller : HealthController<Views.HealthV1>
    {
        public HealthV1Controller(Stores.IHealthStore store) : base(store) { }
    }
}
