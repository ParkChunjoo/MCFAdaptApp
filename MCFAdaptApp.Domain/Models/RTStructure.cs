using System;
using System.Collections.Generic;

namespace MCFAdaptApp.Domain.Models
{
    public class RTStructure
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public DateTime CreatedDate { get; set; }

        public string DicomFile { get; set; }

        public string PatientId { get; set; }

        public List<string> StructureNames { get; set; } = new List<string>();

        public string ReferenceCTId { get; set; }
    }
}
