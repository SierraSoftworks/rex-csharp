using System;
using System.Xml.Serialization;
using Randy.Models;

namespace Randy.Views
{
    [XmlType("Health")]
    public class HealthV1 : IModelView<Models.Health>, IModelSource<Models.Health>
    {
        [XmlAttribute("ok")]
        public bool Ok { get; set; }

        public void FromModel(Health model)
        {
            this.Ok = model.Ok;
        }

        public Health ToModel()
        {
            return new Health
            {
                Ok = this.Ok,
                StartedAt = DateTime.UtcNow,
            };
        }
    }
}