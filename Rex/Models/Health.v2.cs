using System;
using System.Xml.Serialization;

namespace Rex.Models
{
    public partial class Health
    {
        [XmlType("Health")]
        public class Version2 : IView<Health>
        {
            [XmlAttribute("ok")]
            public bool Ok { get; set; }

            public DateTime StartedAt { get; set; }

            public class Representer : IRepresenter<Health, Version2>
            {
                public Health ToModel(Version2 view)
                {
                    return new Health
                    {
                        Ok = view.Ok,
                        StartedAt = view.StartedAt,
                    };
                }

                public Version2 ToView(Health model)
                {
                    return new Version2
                    {
                        Ok = model.Ok,
                        StartedAt = model.StartedAt,
                    };
                }
            }
        }
    }
}