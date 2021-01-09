using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rex.Models;
using SierraLib.API.Views;

namespace Rex.Controllers
{
    public abstract class UserController<T> : ControllerBase
        where T : class, IView<User>
    {
        public UserController(Stores.IUserStore userStore, IRepresenter<User, T> representer)
        {
            UserStore = userStore;
            Representer = representer;
        }

        protected Stores.IUserStore UserStore { get; }

        protected IRepresenter<User, T> Representer { get; }

        [HttpGet]
        [Route("api/[area]/user/{emailHash}", Name = "GetUser.[area]")]
        [Authorize(Scopes.UsersRead, Roles = "Administrator,User")]
        public virtual async Task<ActionResult<T>> GetUserInfo(string? emailHash)
        {
            if (string.IsNullOrWhiteSpace(emailHash))
            {
                return this.NotFound();
            }

            return Representer.ToViewOrDefault(await UserStore.GetUserAsync(emailHash).ConfigureAwait(false)).ToActionResult() ?? this.NotFound();
        }

    }
}