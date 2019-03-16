using System;
using Randy.Models;

namespace Randy.Views
{
    public class HealthV2 : IModelView<Models.Health>, IModelSource<Models.Health>
    {
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