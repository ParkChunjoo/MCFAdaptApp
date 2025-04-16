using System;
using System.Collections.Generic;

namespace MCFAdaptApp.Domain.Models
{
    /// <summary>
    /// Represents an RT Structure Set containing contours for radiotherapy planning
    /// </summary>
    public class RTStructure
    {
        /// <summary>
        /// Unique identifier for the structure set
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Name of the structure set
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// File system path to the structure set
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Creation date of the structure set
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Path to the DICOM file containing the structure set
        /// </summary>
        public string DicomFile { get; set; } = string.Empty;

        /// <summary>
        /// Patient ID associated with this structure set
        /// </summary>
        public string PatientId { get; set; } = string.Empty;

        /// <summary>
        /// List of structure names contained in this structure set
        /// </summary>
        public List<string> StructureNames { get; set; } = new List<string>();

        /// <summary>
        /// ID of the reference CT dataset this structure set is associated with
        /// </summary>
        public string ReferenceCTId { get; set; } = string.Empty;
    }
}
