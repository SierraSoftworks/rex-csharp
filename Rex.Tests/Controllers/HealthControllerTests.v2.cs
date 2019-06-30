using Rex.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Rex.Tests.Controllers
{
    public class HealthControllerV2Tests
        : HealthControllerTests<Health.Version1, Health.Version1.Representer>
    {
        public HealthControllerV2Tests(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        protected override string Version => "v1";
    }
}
