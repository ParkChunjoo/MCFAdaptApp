using System;

namespace MCFAdaptApp.Domain.Models
{
    /// <summary>
    /// Represents a radiotherapy treatment plan
    /// </summary>
    public class RTPlan
    {
        /// <summary>
        /// Unique identifier for the plan
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Name of the treatment plan
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// File system path to the plan
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Creation date of the plan
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Last modification date of the plan
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Path to the DICOM file containing the plan
        /// </summary>
        public string DicomFile { get; set; } = string.Empty;

        /// <summary>
        /// Patient ID associated with this plan
        /// </summary>
        public string PatientId { get; set; } = string.Empty;

        /// <summary>
        /// Label for the treatment plan
        /// </summary>
        public string PlanLabel { get; set; } = string.Empty;

        /// <summary>
        /// Description of the treatment plan
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Status of the plan (e.g., "Complete", "In Progress")
        /// </summary>
        public string Status { get; set; } = "Complete";

        /// <summary>
        /// Type of the plan (e.g., "Reference", "Adapted")
        /// </summary>
        public string Type { get; set; } = "Reference";

        /// <summary>
        /// Isocenter position [X, Y, Z] in mm
        /// </summary>
        public double[]? IsocenterPosition { get; set; }

        /// <summary>
        /// ID of the reference CT dataset this plan is associated with
        /// </summary>
        public string RTCTId { get; set; } = string.Empty;

        /// <summary>
        /// ID of the RT structure set this plan is associated with
        /// </summary>
        public string RTStructureId { get; set; } = string.Empty;
    }
}
