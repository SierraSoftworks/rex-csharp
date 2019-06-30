using System;
using System.Linq;
using System.Xml.Serialization;

namespace Rex.Models
{
    public partial class Idea
    {
        [XmlType("Idea")]
        public class Version2 : IView<Idea>
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

            public class Representer : IRepresenter<Idea, Version2>
            {
                public Idea ToModel(Version2 view)
                {
                    return new Idea
                    {
                        CollectionId = Guid.Empty,
                        Id = view.Id != null ? Guid.Parse(view.Id) : Guid.NewGuid(),
                        Name = view.Name,
                        Description = view.Description,
                        Completed = view.Completed,
                        Tags = new System.Collections.Generic.HashSet<string>(view.Tags),
                    };
                }

                public Version2 ToView(Idea model)
                {
                    return new Version2
                    {
                        Id = model.Id.ToString("N"),
                        Name = model.Name,
                        Description = model.Description,
                        Completed = model.Completed,
                        Tags = model.Tags.ToArray(),
                    };
                }
            }
        }
    }
}