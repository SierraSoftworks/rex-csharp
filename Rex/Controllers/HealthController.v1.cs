namespace Rex.Controllers;

[Area("v1")]
[Route("api/[area]/health")]
[ApiController]
public class HealthV1Controller : HealthController<Health.Version1>
{
    public HealthV1Controller(IHealthStore store, IRepresenter<Health, Health.Version1> representer) : base(store, representer)
    {
    }
}
