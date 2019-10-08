using System;

namespace Rex.Models
{
    public partial class User
    {
        public Guid PrincipalId { get; set; }

        public string EmailHash { get; set; } = "";

        public string FirstName { get; set; } = "";
    }
}