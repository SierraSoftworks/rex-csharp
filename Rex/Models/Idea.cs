using System;
using System.Collections.Generic;
using Rex.Views;

namespace Rex.Models
{
    public class Idea : IModel<Idea>
    {
        public Guid CollectionId { get; set; }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool Completed { get; set; }

        public HashSet<string> Tags { get; set; }

        public T ToView<T>() where T : IModelView<Idea>, new()
        {
            var view = new T();
            view.FromModel(this);

            return view;
        }
    }
}