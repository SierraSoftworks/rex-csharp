using System;
using System.Xml.Serialization;
using Rex.Models;

namespace Rex.Views
{
    [XmlType("Health")]
    public class RoleAssignmentV1 : IModelView<Models.RoleAssignment>, IModelSource<Models.RoleAssignment>
    {
        [XmlAttribute("PrincipalId")]
        public string PrincipalId { get; set; }

        [XmlAttribute("CollectionId")]
        public string CollectionId { get; set; }

        [XmlAttribute("Role")]
        public string Role { get; set; }

        public void FromModel(RoleAssignment model)
        {
            this.PrincipalId = model.PrincipalId.ToString("N");
            this.CollectionId = model.CollectionId.ToString("N");
            this.Role = model.Role;
        }

        public RoleAssignment ToModel()
        {
            return new RoleAssignment
            {
                CollectionId = Guid.ParseExact(this.CollectionId, "N"),
                PrincipalId = Guid.ParseExact(this.PrincipalId, "N"),
                Role = this.Role,
            };
        }
    }
}