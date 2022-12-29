namespace Rex.Models;

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
                if (view is null)
                {
                    throw new ArgumentNullException(nameof(view));
                }

                return new Health
                {
                    Ok = view.Ok,
                    StartedAt = view.StartedAt,
                };
            }

            public Version2 ToView(Health model)
            {
                if (model is null)
                {
                    throw new ArgumentNullException(nameof(model));
                }

                return new Version2
                {
                    Ok = model.Ok,
                    StartedAt = model.StartedAt,
                };
            }
        }
    }
}