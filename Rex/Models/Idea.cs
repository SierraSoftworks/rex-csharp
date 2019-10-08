using System;
using System.Collections.Generic;

namespace Rex.Models
{
    public partial class Idea
    {
        public Guid CollectionId { get; set; }

        public Guid Id { get; set; }

        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        public bool Completed { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This is used by the IRepresenter implementation")]
        public HashSet<string> Tags { get; set; } = new HashSet<string>();
    }
}