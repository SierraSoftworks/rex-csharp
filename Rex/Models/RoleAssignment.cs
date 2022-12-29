namespace Rex.Models;

public partial class RoleAssignment
{
    public Guid PrincipalId { get; set; }

    public Guid CollectionId { get; set; }

    public string Role { get; set; } = Viewer;

    public const string Owner = "Owner";

    public const string Contributor = "Contributor";

    public const string Viewer = "Viewer";
}