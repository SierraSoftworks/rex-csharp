namespace Rex.Controllers;

[Area("v1")]
[ApiController]
public class IdeaV1Controller : IdeaController<Idea.Version1>
{
    public IdeaV1Controller(IIdeaStore store, IRoleAssignmentStore roleStore, IRepresenter<Idea, Idea.Version1> representer, ILogger<IdeaController<Idea.Version1>> logger) : base(store, roleStore, representer, logger)
    {
    }
}
