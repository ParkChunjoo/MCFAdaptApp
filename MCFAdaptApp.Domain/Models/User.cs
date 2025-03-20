using System;

namespace MCFAdaptApp.Domain.Models
{
    /// <summary>
    /// Represents a user in the system for authentication and authorization
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// User's password (should be stored securely in production)
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// User's role in the system (e.g., Admin, Doctor, Technician)
        /// </summary>
        public string Role { get; set; }
        
        /// <summary>
        /// Date and time when the user was created
        /// </summary>
        public DateTime CreatedDate { get; set; }
        
        /// <summary>
        /// Date and time when the user last logged in
        /// </summary>
        public DateTime? LastLoginDate { get; set; }
        
        /// <summary>
        /// Indicates whether the user account is active
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Optional reference to a patient ID if this user is associated with a patient
        /// </summary>
        public string PatientId { get; set; }
    }
} 