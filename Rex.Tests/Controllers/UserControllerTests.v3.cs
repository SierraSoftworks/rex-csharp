namespace Rex.Tests.Controllers;

public class UserControllerV3Tests
    : UserControllerTests<User.Version3>
{
    public UserControllerV3Tests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    protected override string Version => "v3";
}
