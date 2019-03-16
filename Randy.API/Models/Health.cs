using System;
using Randy.API.Views;

namespace Randy.API.Models
{
    public class Health : IModel<Health>
    {
        public bool Ok { get; set; }

        public DateTime StartedAt { get; set; }

        public T ToView<T>() where T : IModelView<Health>, new()
        {
            var view = new T();

            view.FromModel(this);

            return view;
        }
    }
}