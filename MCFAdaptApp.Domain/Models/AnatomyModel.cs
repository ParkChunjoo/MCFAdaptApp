using System;
using System.Collections.ObjectModel;

namespace MCFAdaptApp.Domain.Models
{
    /// <summary>
    /// Represents a patient's anatomical model
    /// </summary>
    public class AnatomyModel
    {
        /// <summary>
        /// Name of the anatomical model
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Status of the model (e.g., "Complete", "In Progress")
        /// </summary>
        public string Status { get; set; } = "Complete";

        /// <summary>
        /// Last modification date of the model
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Collection of reference plans associated with this model
        /// </summary>
        public ObservableCollection<ReferencePlan> ReferencePlans { get; set; } = new ObservableCollection<ReferencePlan>();
    }
}