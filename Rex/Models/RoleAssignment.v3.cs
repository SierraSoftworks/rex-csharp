using System;
using System.Globalization;
using System.Xml.Serialization;
using SierraLib.API.Views;

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
                    if (view is null)
                    {
                        throw new ArgumentNullException(nameof(view));
                    }

                    return new RoleAssignment
                    {
                        PrincipalId = view.PrincipalId != null ? Guid.ParseExact(view.PrincipalId, "N") : Guid.Empty,
                        CollectionId = view.CollectionId != null ? Guid.ParseExact(view.CollectionId, "N") : Guid.Empty,
                        Role = view.Role,
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
}