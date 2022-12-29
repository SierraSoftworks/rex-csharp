namespace Rex.Models;

public partial class Idea
{
    [XmlType("Idea")]
    public class Version2 : IView<Idea>
    {

        [XmlAttribute("id")]
        public string? Id { get; set; }

        [XmlElement("Name")]
        public string? Name { get; set; }

        [XmlElement("Description")]
        public string? Description { get; set; }

        [XmlAttribute("completed")]
        public bool? Completed { get; set; }

        [XmlArray("Tags")]
        [XmlArrayItem("Tag")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "This models the API response.")]
        public string[] Tags { get; set; } = Array.Empty<string>();

        public class Representer : IRepresenter<Idea, Version2>
        {
            public Idea ToModel(Version2 view)
            {
                if (view is null)
                {
                    throw new ArgumentNullException(nameof(view));
                }

                return new Idea
                {
                    CollectionId = Guid.Empty,
                    Id = view.Id != null ? Guid.Parse(view.Id) : Guid.NewGuid(),
                    Name = view.Name ?? throw new RequiredFieldException(nameof(Idea), nameof(Idea.Name)),
                    Description = view.Description ?? throw new RequiredFieldException(nameof(Idea), nameof(Idea.Name)),
                    Completed = view.Completed ?? false,
                    Tags = new System.Collections.Generic.HashSet<string>(view.Tags ?? Array.Empty<string>()),
                };
            }

            public Version2 ToView(Idea model)
            {
                if (model is null)
                {
                    throw new ArgumentNullException(nameof(model));
                }

                return new Version2
                {
                    Id = model.Id.ToString("N", CultureInfo.InvariantCulture),
                    Name = model.Name,
                    Description = model.Description,
                    Completed = model.Completed,
                    Tags = model.Tags.ToArray(),
                };
            }
        }
    }
}