using System;
using System.Xml.Serialization;

namespace Rex.Models
{
    public partial class RoleAssignment
    {
        [XmlType("Health")]
        public class Version3 : IView<RoleAssignment>
        {
            [XmlAttribute("PrincipalId")]
            public string PrincipalId { get; set; }

            [XmlAttribute("CollectionId")]
            public string CollectionId { get; set; }

            [XmlAttribute("Role")]
            public string Role { get; set; }

            public class Representer : IRepresenter<RoleAssignment, Version3>
            {
                public RoleAssignment ToModel(Version3 view)
                {
                    return new RoleAssignment
                    {
                        PrincipalId = Guid.ParseExact(view.PrincipalId, "N"),
                        CollectionId = Guid.ParseExact(view.CollectionId, "N"),
                        Role = view.Role,
                    };
                }

                public Version3 ToView(RoleAssignment model)
                {
                    return new Version3
                    {
                        PrincipalId = model.PrincipalId.ToString("N"),
                        CollectionId = model.CollectionId.ToString("N"),
                        Role = model.Role,
                    };
                }
            }
        }
    }
}