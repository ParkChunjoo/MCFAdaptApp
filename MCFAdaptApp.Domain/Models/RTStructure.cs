using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        /// Last modification date of the structure set
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Path to the DICOM file containing the structure set
        /// </summary>
        public string DicomFile { get; set; } = string.Empty;

        /// <summary>
        /// Patient ID associated with this structure set
        /// </summary>
        public string PatientId { get; set; } = string.Empty;

        /// <summary>
        /// Status of the structure set (e.g., "Complete", "In Progress")
        /// </summary>
        public string Status { get; set; } = "Complete";

        /// <summary>
        /// Type of the structure set (e.g., "Original", "Edited")
        /// </summary>
        public string Type { get; set; } = "Original";

        /// <summary>
        /// List of structure names contained in this structure set
        /// </summary>
        public List<string> StructureNames { get; set; } = new List<string>();

        /// <summary>
        /// ID of the reference CT dataset this structure set is associated with
        /// </summary>
        public string RTCTId { get; set; } = string.Empty;

        /// <summary>
        /// Collection of treatment plans associated with this structure set
        /// </summary>
        public ObservableCollection<RTPlan> Plans { get; set; } = new ObservableCollection<RTPlan>();
    }
}
