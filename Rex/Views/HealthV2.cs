using System;
using System.Xml.Serialization;
using Rex.Models;

namespace Rex.Views
{
    [XmlType("Health")]
    public class HealthV2 : IModelView<Models.Health>, IModelSource<Models.Health>
    {
        [XmlAttribute("ok")]
        public bool Ok { get; set; }

        public DateTime StartedAt { get; set; }

        public void FromModel(Health model)
        {
            this.Ok = model.Ok;
            this.StartedAt = model.StartedAt;
        }

        public Health ToModel()
        {
            return new Health
            {
                Ok = this.Ok,
                StartedAt = this.StartedAt,
            };
        }
    }
}