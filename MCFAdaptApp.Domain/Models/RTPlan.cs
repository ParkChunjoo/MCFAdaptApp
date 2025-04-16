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
        public string PlanDescription { get; set; } = string.Empty;

        /// <summary>
        /// Isocenter position [X, Y, Z] in mm
        /// </summary>
        public double[]? IsocenterPosition { get; set; }

        /// <summary>
        /// ID of the reference CT dataset this plan is associated with
        /// </summary>
        public string ReferenceCTId { get; set; } = string.Empty;

        /// <summary>
        /// ID of the RT structure set this plan is associated with
        /// </summary>
        public string RTStructureId { get; set; } = string.Empty;
    }
}
