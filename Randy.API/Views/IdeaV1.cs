using System;
using Randy.API.Models;

namespace Randy.API.Views
{
    public class IdeaV1 : IModelView<Models.Idea>, IModelSource<Models.Idea>
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public void FromModel(Idea model)
        {
            this.Id = model.Id;
            this.Name = model.Name;
            this.Description = model.Description;
        }

        public Idea ToModel()
        {
            return new Idea
            {
                Id = this.Id ?? Guid.NewGuid(),
                Name = this.Name,
                Description = this.Description,
                Completed = false,
                Tags = new System.Collections.Generic.HashSet<string>(),
            };
        }
    }
}