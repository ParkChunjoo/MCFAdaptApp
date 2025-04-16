using System;

namespace MCFAdaptApp.Domain.Models
{
    /// <summary>
    /// Represents a reference treatment plan for a patient
    /// </summary>
    public class ReferencePlan
    {
        /// <summary>
        /// Name of the reference plan
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Status of the plan (e.g., "Complete", "In Progress")
        /// </summary>
        public string Status { get; set; } = "Complete";

        /// <summary>
        /// Last modification date of the plan
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Description of the plan
        /// </summary>
        public string Description { get; set; } = "Photon plan/3D-CRT/3 beams";
    }
}