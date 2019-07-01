using System;
using System.Linq;
using System.Xml.Serialization;

namespace Rex.Models
{
    public partial class Idea
    {
        [XmlType("Idea")]
        public class Version3 : IView<Idea>
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
            public bool? Completed { get; set; }

            [XmlArray("Tags")]
            [XmlArrayItem("Tag")]
            public string[] Tags { get; set; }

            public class Representer : IRepresenter<Idea, Version3>
            {
                public Idea ToModel(Version3 view)
                {
                    return new Idea
                    {
                        CollectionId = view.Collection != null ? Guid.Parse(view.Collection) : Guid.Empty,
                        Id = view.Id != null ? Guid.Parse(view.Id) : Guid.NewGuid(),
                        Name = view.Name,
                        Description = view.Description,
                        Completed = view.Completed ?? false,
                        Tags = new System.Collections.Generic.HashSet<string>(view.Tags ?? Array.Empty<string>()),
                    };
                }

                public Version3 ToView(Idea model)
                {
                    return new Version3
                    {
                        Collection = model.CollectionId.ToString("N"),
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