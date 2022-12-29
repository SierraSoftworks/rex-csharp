namespace Rex.Tests.Controllers;

public class IdeaControllerV1Tests
    : IdeaControllerTests<Idea.Version1>
{
    public IdeaControllerV1Tests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    protected override string Version => "v1";
}
