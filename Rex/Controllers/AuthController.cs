using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Rex.Controllers
{
    [Area("v1")]
    [Route("api/[area]/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public virtual ActionResult GetClaims() => Ok(this.User.Claims.GroupBy(c => c.Type).ToDictionary(g => g.Key, g => g.Count() == 1 ? (object)g.Single().Value : g.Select(c => c.Value).ToArray()));
    }
}
