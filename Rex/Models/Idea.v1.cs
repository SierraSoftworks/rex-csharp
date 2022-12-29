namespace Rex.Models;

public partial class Idea
{
    [XmlType("Idea")]
    public class Version1 : IView<Idea>
    {

        [XmlAttribute("id")]
        public string? Id { get; set; }

        [XmlElement("Name")]
        public string? Name { get; set; }

        [XmlElement("Description")]
        public string? Description { get; set; }

        public class Representer : IRepresenter<Idea, Version1>
        {
            public Idea ToModel(Version1 view)
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
                    Description = view.Description ?? throw new RequiredFieldException(nameof(Idea), nameof(Idea.Description)),
                    Completed = false,
                    Tags = new System.Collections.Generic.HashSet<string>(),
                };
            }

            public Version1 ToView(Idea model)
            {
                if (model is null)
                {
                    throw new ArgumentNullException(nameof(model));
                }

                return new Version1
                {
                    Id = model.Id.ToString("N", CultureInfo.InvariantCulture),
                    Name = model.Name,
                    Description = model.Description,
                };
            }
        }
    }
}