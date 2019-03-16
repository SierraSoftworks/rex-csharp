using System;
using Randy.Models;

namespace Randy.Views
{
    public class HealthV1 : IModelView<Models.Health>, IModelSource<Models.Health>
    {
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