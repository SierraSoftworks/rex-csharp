namespace Rex.Models;

public partial class RoleAssignment
{
    [XmlType("Health")]
    public class Version3 : IView<RoleAssignment>
    {
        [XmlAttribute("user-id")]
        public string? PrincipalId { get; set; }

        [XmlAttribute("collection-id")]
        public string? CollectionId { get; set; }

        [XmlAttribute("role")]
        public string? Role { get; set; }

        public class Representer : IRepresenter<RoleAssignment, Version3>
        {
            public RoleAssignment ToModel(Version3 view)
            {
                if (view is null)
                {
                    throw new ArgumentNullException(nameof(view));
                }

                return new RoleAssignment
                {
                    PrincipalId = view.PrincipalId != null ? Guid.ParseExact(view.PrincipalId, "N") : Guid.Empty,
                    CollectionId = view.CollectionId != null ? Guid.ParseExact(view.CollectionId, "N") : Guid.Empty,
                    Role = view.Role ?? throw new RequiredFieldException(nameof(RoleAssignment), nameof(RoleAssignment.Role)),
                };
            }

            public Version3 ToView(RoleAssignment model)
            {
                if (model is null)
                {
                    throw new ArgumentNullException(nameof(model));
                }

                return new Version3
                {
                    PrincipalId = model.PrincipalId.ToString("N", CultureInfo.InvariantCulture),
                    CollectionId = model.CollectionId.ToString("N", CultureInfo.InvariantCulture),
                    Role = model.Role,
                };
            }
        }
    }
}