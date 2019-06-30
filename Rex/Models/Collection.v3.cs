using System;
using System.Xml.Serialization;

namespace Rex.Models
{
    public partial class Collection
    {
        [XmlType("Collection")]
        public class Version3 : IView<Collection>
        {
            [XmlAttribute("id")]
            public string ID { get; set; }

            [XmlAttribute("user-id")]
            public string UserID { get; set; }

            [XmlAttribute("name")]
            public string Name { get; set; }

            public class Representer : IRepresenter<Collection, Version3>
            {
                public Collection ToModel(Version3 view)
                {
                    return new Collection
                    {
                        CollectionId = Guid.ParseExact(view.ID, "N"),
                        PrincipalId = Guid.ParseExact(view.UserID, "N"),
                        Name = view.Name,
                    };
                }

                public Version3 ToView(Collection model)
                {
                    return new Version3
                    {
                        ID = model.CollectionId.ToString("N"),
                        UserID = model.PrincipalId.ToString("N"),
                        Name = model.Name
                    };
                }
            }
        }
    }
}