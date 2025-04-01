using System;

namespace MCFAdaptApp.Domain.Models
{
    public class RTPlan
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public DateTime CreatedDate { get; set; }

        public string DicomFile { get; set; }

        public string PatientId { get; set; }

        public string PlanLabel { get; set; }

        public string PlanDescription { get; set; }

        public string ReferenceCTId { get; set; }

        public string RTStructureId { get; set; }
    }
}
