using System;
using System.Xml.Serialization;

namespace Rex.Models
{
    public partial class Health
    {
        [XmlType("Health")]
        public class Version1 : IView<Health>
        {
            [XmlAttribute("ok")]
            public bool Ok { get; set; }


            public class Representer : IRepresenter<Health, Version1>
            {
                public Health ToModel(Version1 view)
                {
                    return new Health
                    {
                        Ok = view.Ok,
                        StartedAt = DateTime.UtcNow,
                    };
                }

                public Version1 ToView(Health model)
                {
                    return new Version1
                    {
                        Ok = model.Ok,
                    };
                }
            }
        }
    }
}