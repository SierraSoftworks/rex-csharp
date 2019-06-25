using System;
using System.Linq;
using System.Xml.Serialization;
using Rex.Models;

namespace Rex.Views
{
    [XmlType("Idea")]
    public class IdeaV3 : IModelView<Models.Idea>, IModelSource<Models.Idea>
    {
        [XmlAttribute("CollectionId")]
        public string Collection { get; set; }

        [XmlAttribute("Id")]
        public string Id { get; set; }

        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlAttribute("Completed")]
        public bool Completed { get; set; }

        [XmlArray("Tags")]
        [XmlArrayItem("Tag")]
        public string[] Tags { get; set; }

        public void FromModel(Idea model)
        {
            this.Collection = model.CollectionId.ToString("N");
            this.Id = model.Id.ToString("N");
            this.Name = model.Name;
            this.Description = model.Description;
            this.Completed = model.Completed;
            this.Tags = model.Tags.ToArray();
        }

        public Idea ToModel()
        {
            return new Idea
            {
                CollectionId = this.Collection != null ? Guid.Parse(this.Collection) : Guid.NewGuid(),
                Id = this.Id != null ? Guid.Parse(this.Id) : Guid.NewGuid(),
                Name = this.Name,
                Description = this.Description,
                Completed = this.Completed,
                Tags = new System.Collections.Generic.HashSet<string>(this.Tags),
            };
        }
    }
}