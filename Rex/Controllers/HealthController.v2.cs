using Microsoft.AspNetCore.Mvc;
using Rex.Models;
using Rex.Stores;

namespace Rex.Controllers
{
    [Area("v2")]
    [Route("api/[area]/health")]
    [ApiController]
    public class HealthV2Controller : HealthController<Health.Version2>
    {
        public HealthV2Controller(IHealthStore store, IRepresenter<Health, Health.Version2> representer) : base(store, representer)
        {
        }
    }
}
