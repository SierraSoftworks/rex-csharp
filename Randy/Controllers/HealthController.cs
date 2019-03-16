using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Randy.Controllers
{
    public abstract class HealthController<T> : ControllerBase
        where T : Views.IModelView<Models.Health>, new()
    {
        public HealthController(Stores.HealthStore store) => Store = store;

        protected Stores.HealthStore Store { get; }


        [HttpGet]
        public virtual async Task<T> Get() => (await this.Store.GetHealthStateAsync()).ToView<T>();
    }

    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/health")]
    [ApiController]
    public class HealthV1Controller : HealthController<Views.HealthV1>
    {
        public HealthV1Controller(Stores.HealthStore store) : base(store) { }
    }

    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/health")]
    [ApiController]
    public class HealthV2Controller : HealthController<Views.HealthV2>
    {
        public HealthV2Controller(Stores.HealthStore store) : base(store) { }
    }
}
