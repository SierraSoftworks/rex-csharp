namespace Rex.Tests.Controllers;

public class IdeaControllerV2Tests
    : IdeaControllerTests<Idea.Version2>
{
    public IdeaControllerV2Tests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    protected override string Version => "v2";
}
