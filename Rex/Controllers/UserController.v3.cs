namespace Rex.Controllers;

[Area("v3")]
[ApiController]
public class UserV3Controller : UserController<User.Version3>
{
    public UserV3Controller(
        IUserStore userStore,
        IRepresenter<User, User.Version3> representer) : base(userStore, representer)
    {
    }
}
