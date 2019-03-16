using System;
using System.Linq;
using Randy.API.Models;

namespace Randy.API.Views
{
    public class IdeaV2 : IModelView<Models.Idea>, IModelSource<Models.Idea>
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool Completed { get; set; }

        public string[] Tags { get; set; }

        public void FromModel(Idea model)
        {
            this.Id = model.Id;
            this.Name = model.Name;
            this.Description = model.Description;
            this.Completed = model.Completed;
            this.Tags = model.Tags.ToArray();
        }

        public Idea ToModel()
        {
            return new Idea
            {
                Id = this.Id ?? Guid.NewGuid(),
                Name = this.Name,
                Description = this.Description,
                Completed = this.Completed,
                Tags = new System.Collections.Generic.HashSet<string>(this.Tags),
            };
        }
    }
}