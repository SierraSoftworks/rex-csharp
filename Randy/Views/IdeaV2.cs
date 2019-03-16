using System;
using System.Linq;
using System.Xml.Serialization;
using Randy.Models;

namespace Randy.Views
{
    [XmlType("Idea")]
    public class IdeaV2 : IModelView<Models.Idea>, IModelSource<Models.Idea>
    {
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
                Id = this.Id != null ? Guid.Parse(this.Id) : Guid.NewGuid(),
                Name = this.Name,
                Description = this.Description,
                Completed = this.Completed,
                Tags = new System.Collections.Generic.HashSet<string>(this.Tags),
            };
        }
    }
}