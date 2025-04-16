using System;
using System.Collections.ObjectModel;

namespace MCFAdaptApp.Domain.Models
{
    /// <summary>
    /// Represents a patient in the system
    /// </summary>
    public class Patient
    {
        /// <summary>
        /// Unique identifier for the patient
        /// </summary>
        public string PatientId { get; set; } = string.Empty;

        /// <summary>
        /// Patient's first name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Patient's last name
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Patient's date of birth
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Patient's gender
        /// </summary>
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// Last modified date/time
        /// </summary>
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// Latest CBCT scan time
        /// </summary>
        public DateTime? LastCBCTScanTime { get; set; }

        /// <summary>
        /// Associated user account for this patient
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Full display name (Last, First)
        /// </summary>
        public string DisplayName => $"{LastName}, {FirstName}";

        /// <summary>
        /// Collection of anatomical models for this patient
        /// </summary>
        public ObservableCollection<AnatomyModel> AnatomyModels { get; set; } = new ObservableCollection<AnatomyModel>();
    }
}