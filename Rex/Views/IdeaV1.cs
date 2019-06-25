using System;
using System.Xml.Serialization;
using Rex.Models;

namespace Rex.Views
{
    [XmlType("Idea")]
    public class IdeaV1 : IModelView<Models.Idea>, IModelSource<Models.Idea>
    {

        [XmlAttribute("Id")]
        public string Id { get; set; }


        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        public void FromModel(Idea model)
        {
            this.Id = model.Id.ToString("N");
            this.Name = model.Name;
            this.Description = model.Description;
        }

        public Idea ToModel()
        {
            return new Idea
            {
                CollectionId = Guid.Empty,
                Id = this.Id != null ? Guid.Parse(this.Id) : Guid.NewGuid(),
                Name = this.Name,
                Description = this.Description,
                Completed = false,
                Tags = new System.Collections.Generic.HashSet<string>(),
            };
        }
    }
}