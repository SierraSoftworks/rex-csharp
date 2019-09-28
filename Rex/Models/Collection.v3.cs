using System;
using System.Globalization;
using System.Xml.Serialization;
using SierraLib.API.Views;

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
                    if (view is null)
                    {
                        throw new ArgumentNullException(nameof(view));
                    }

                    return new Collection
                    {
                        CollectionId = view.ID != null ? Guid.ParseExact(view.ID, "N") : Guid.NewGuid(),
                        PrincipalId = view.UserID != null ? Guid.ParseExact(view.UserID, "N") : Guid.Empty,
                        Name = view.Name,
                    };
                }

                public Version3 ToView(Collection model)
                {
                    if (model is null)
                    {
                        throw new ArgumentNullException(nameof(model));
                    }

                    return new Version3
                    {
                        ID = model.CollectionId.ToString("N", CultureInfo.InvariantCulture),
                        UserID = model.PrincipalId.ToString("N", CultureInfo.InvariantCulture),
                        Name = model.Name
                    };
                }
            }
        }
    }
}