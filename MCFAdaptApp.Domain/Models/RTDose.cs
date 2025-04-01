using System;

namespace MCFAdaptApp.Domain.Models
{
    public class RTDose
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public DateTime CreatedDate { get; set; }

        public string DicomFile { get; set; }

        public string PatientId { get; set; }

        public string DoseUnits { get; set; }

        public string DoseType { get; set; }

        public double DoseMax { get; set; }

        public int[] DoseGridSize { get; set; } = new int[3];

        public string ReferenceCTId { get; set; }

        public string RTPlanId { get; set; }
    }
}
