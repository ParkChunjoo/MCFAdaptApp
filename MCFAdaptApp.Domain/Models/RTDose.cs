using System;

namespace MCFAdaptApp.Domain.Models
{
    /// <summary>
    /// Represents a radiotherapy dose distribution
    /// </summary>
    public class RTDose
    {
        /// <summary>
        /// Unique identifier for the dose distribution
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Name of the dose distribution
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// File system path to the dose distribution
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Creation date of the dose distribution
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Path to the DICOM file containing the dose distribution
        /// </summary>
        public string DicomFile { get; set; } = string.Empty;

        /// <summary>
        /// Patient ID associated with this dose distribution
        /// </summary>
        public string PatientId { get; set; } = string.Empty;

        /// <summary>
        /// Units of dose (e.g., "GY")
        /// </summary>
        public string DoseUnits { get; set; } = string.Empty;

        /// <summary>
        /// Type of dose (e.g., "PHYSICAL")
        /// </summary>
        public string DoseType { get; set; } = string.Empty;

        /// <summary>
        /// Maximum dose value in the distribution
        /// </summary>
        public double DoseMax { get; set; }

        /// <summary>
        /// Size of the dose grid [X, Y, Z]
        /// </summary>
        public int[] DoseGridSize { get; set; } = new int[3];

        /// <summary>
        /// ID of the reference CT dataset this dose is associated with
        /// </summary>
        public string ReferenceCTId { get; set; } = string.Empty;

        /// <summary>
        /// ID of the RT plan this dose is associated with
        /// </summary>
        public string RTPlanId { get; set; } = string.Empty;
    }
}
