using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rex.Models;

namespace Rex.Controllers
{
    public abstract class HealthController<T> : ControllerBase
        where T : IView<Health>
    {
        public HealthController(Stores.IHealthStore store, IRepresenter<Health, T> representer)
        {
            Store = store;
            Representer = representer;
        }

        protected IRepresenter<Health, T> Representer { get; }

        protected Stores.IHealthStore Store { get; }


        [HttpGet]
        [AllowAnonymous]
        public virtual async Task<T> Get() => Representer.ToViewSafe(await this.Store.GetHealthStateAsync());
    }
}
