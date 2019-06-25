using System;
using Rex.Views;

namespace Rex.Models
{
    public class RoleAssignment : IModel<RoleAssignment>
    {
        public Guid PrincipalId { get; set; }

        public Guid CollectionId { get; set; }

        public string Role { get; set; }

        public T ToView<T>() where T : IModelView<RoleAssignment>, new()
        {
            var view = new T();

            view.FromModel(this);

            return view;
        }


        public const string Owner = "Owner";

        public const string Contributor = "Contributor";

        public const string Viewer = "Viewer";
    }
}