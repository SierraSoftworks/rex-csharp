namespace Rex.Models;

public partial class Collection
{
    [XmlType("Collection")]
    public class Version3 : IView<Collection>
    {
        [XmlAttribute("id")]
        public string? Id { get; set; }

        [XmlAttribute("user-id")]
        public string? UserId { get; set; }

        [XmlElement("Name")]
        public string? Name { get; set; }

        public class Representer : IRepresenter<Collection, Version3>
        {
            public Collection ToModel(Version3 view)
            {
                if (view is null)
                {
                    throw new ArgumentNullException(nameof(view));
                }

                return new Collection
                {
                    CollectionId = view.Id != null ? Guid.ParseExact(view.Id, "N") : Guid.NewGuid(),
                    PrincipalId = view.UserId != null ? Guid.ParseExact(view.UserId, "N") : Guid.Empty,
                    Name = view.Name ?? throw new RequiredFieldException(nameof(Collection), nameof(Collection.Name)),
                };
            }

            public Version3 ToView(Collection model)
            {
                if (model is null)
                {
                    throw new ArgumentNullException(nameof(model));
                }

                return new Version3
                {
                    Id = model.CollectionId.ToString("N", CultureInfo.InvariantCulture),
                    UserId = model.PrincipalId.ToString("N", CultureInfo.InvariantCulture),
                    Name = model.Name
                };
            }
        }
    }
}