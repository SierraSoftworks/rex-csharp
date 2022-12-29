namespace Rex.Models;

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
                if (view is null)
                {
                    throw new ArgumentNullException(nameof(view));
                }

                return new Health
                {
                    Ok = view.Ok,
                    StartedAt = DateTime.UtcNow,
                };
            }

            public Version1 ToView(Health model)
            {
                if (model is null)
                {
                    throw new ArgumentNullException(nameof(model));
                }

                return new Version1
                {
                    Ok = model.Ok,
                };
            }
        }
    }
}