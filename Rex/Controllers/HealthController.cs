using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rex.Models;
using SierraLib.API.Views;

namespace Rex.Controllers
{
    public abstract class HealthController<T> : ControllerBase
        where T : class, IView<Health>
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
        public virtual async Task<T> GetHealth() => Representer.ToView(await Store.GetHealthStateAsync().ConfigureAwait(false));
    }
}
