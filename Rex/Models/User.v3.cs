using System;
using System.Globalization;
using System.Xml.Serialization;
using Rex.Exceptions;
using SierraLib.API.Views;

namespace Rex.Models
{
    public partial class User
    {
        [XmlType("User")]
        public class Version3 : IView<User>
        {
            [XmlAttribute("id")]
            public string? Id { get; set; }

            [XmlAttribute("email-hash")]
            public string? EmailHash { get; set; }

            [XmlElement("FirstName")]
            public string? FirstName { get; set; }

            public class Representer : IRepresenter<User, Version3>
            {
                public User ToModel(Version3 view)
                {
                    if (view is null)
                    {
                        throw new ArgumentNullException(nameof(view));
                    }

                    return new User
                    {
                        PrincipalId = Guid.ParseExact(view.Id ?? throw new RequiredFieldException(nameof(User), nameof(User.PrincipalId)), "N"),
                        EmailHash = view.EmailHash ?? throw new RequiredFieldException(nameof(User), nameof(User.EmailHash)),
                        FirstName = view.FirstName ?? throw new RequiredFieldException(nameof(User), nameof(User.FirstName)),
                    };
                }

                public Version3 ToView(User model)
                {
                    if (model is null)
                    {
                        throw new ArgumentNullException(nameof(model));
                    }

                    return new Version3
                    {
                        Id = model.PrincipalId.ToString("N", CultureInfo.InvariantCulture),
                        EmailHash = model.EmailHash,
                        FirstName = model.FirstName,
                    };
                }
            }
        }
    }
}