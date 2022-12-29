namespace Rex.Tests.Controllers;

public class HealthControllerV1Tests
    : HealthControllerTests<Health.Version1>
{
    public HealthControllerV1Tests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    protected override string Version => "v1";
}
