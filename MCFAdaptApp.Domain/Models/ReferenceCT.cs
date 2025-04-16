using System;
using System.Collections.Generic;

namespace MCFAdaptApp.Domain.Models
{
    /// <summary>
    /// Represents CT image data (either Reference CT or CBCT)
    /// </summary>
    public class ReferenceCT
    {
        /// <summary>
        /// Unique identifier for the CT dataset
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Name of the CT dataset
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// File system path to the CT dataset
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Creation date of the CT dataset
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// List of DICOM file paths that make up this dataset
        /// </summary>
        public List<string> DicomFiles { get; set; } = new List<string>();

        /// <summary>
        /// Image width in pixels
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Image height in pixels
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Number of slices in the volume
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Pixel spacing in X direction (mm)
        /// </summary>
        public double PixelSpacingX { get; set; }

        /// <summary>
        /// Pixel spacing in Y direction (mm)
        /// </summary>
        public double PixelSpacingY { get; set; }

        /// <summary>
        /// Slice thickness (mm)
        /// </summary>
        public double SliceThickness { get; set; }

        /// <summary>
        /// Patient ID associated with this dataset
        /// </summary>
        public string PatientId { get; set; } = string.Empty;

        /// <summary>
        /// CT type ("CBCT" or "ReferenceCT")
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Raw pixel data for the entire volume (Width * Height * Depth)
        /// </summary>
        public short[]? PixelData { get; set; }

        /// <summary>
        /// The index of the slice to be displayed (e.g., center slice or isocenter slice)
        /// </summary>
        public int DisplaySliceIndex { get; set; }
    }
}