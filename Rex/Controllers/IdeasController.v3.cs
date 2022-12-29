namespace Rex.Controllers;

[Area("v3")]
[ApiController]
public class IdeaV3Controller : IdeaController<Idea.Version3>
{
    public IdeaV3Controller(IIdeaStore store, IRoleAssignmentStore roleStore, IRepresenter<Idea, Idea.Version3> representer, ILogger<IdeaController<Idea.Version3>> logger) : base(store, roleStore, representer, logger)
    {
    }
}
