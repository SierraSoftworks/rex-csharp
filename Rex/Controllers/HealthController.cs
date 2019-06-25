using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Rex.Controllers
{
    public abstract class HealthController<T> : ControllerBase
        where T : Views.IModelView<Models.Health>, new()
    {
        public HealthController(Stores.IHealthStore store) => Store = store;

        protected Stores.IHealthStore Store { get; }


        [HttpGet]
        [AllowAnonymous]
        public virtual async Task<T> Get() => (await this.Store.GetHealthStateAsync()).ToView<T>();
    }
}
