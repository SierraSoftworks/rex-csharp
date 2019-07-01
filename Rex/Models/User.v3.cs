using System;
using System.Xml.Serialization;

namespace Rex.Models
{
    public partial class User
    {
        [XmlType("User")]
        public class Version3 : IView<User>
        {
            [XmlAttribute("id")]
            public string ID { get; set; }

            public class Representer : IRepresenter<User, Version3>
            {
                public User ToModel(Version3 view)
                {
                    return new User
                    {
                        PrincipalId = Guid.ParseExact(view.ID, "N"),
                    };
                }

                public Version3 ToView(User model)
                {
                    return new Version3
                    {
                        ID = model.PrincipalId.ToString("N"),
                    };
                }
            }
        }
    }
}