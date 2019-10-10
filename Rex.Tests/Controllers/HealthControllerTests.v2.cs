using Rex.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace Rex.Tests.Controllers
{
    public class HealthControllerV2Tests
        : HealthControllerTests<Health.Version1>
    {
        public HealthControllerV2Tests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        protected override string Version => "v1";
    }
}
