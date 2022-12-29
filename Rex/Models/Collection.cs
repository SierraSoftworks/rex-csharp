namespace Rex.Models;

public partial class Collection
{
    public Guid CollectionId { get; set; }

    public Guid PrincipalId { get; set; }

    public string Name { get; set; } = "";
}