using System.Collections.Generic;
using System.Threading.Tasks;
using MCFAdaptApp.Domain.Models;

namespace MCFAdaptApp.Domain.Services
{
    /// <summary>
    /// DICOM 파일 처리를 위한 서비스 인터페이스
    /// </summary>
    public interface IDicomService
    {
        /// <summary>
        /// Loads CBCT DICOM files from the specified path
        /// </summary>
        /// <param name="patientId">Patient ID</param>
        /// <returns>Loaded CBCT object</returns>
        Task<RTCT> LoadCBCTAsync(string patientId);

        /// <summary>
        /// Loads reference CT DICOM files from the specified path
        /// </summary>
        /// <param name="patientId">Patient ID</param>
        /// <param name="isocenterZ">Optional isocenter Z coordinate</param>
        /// <returns>Loaded reference CT object</returns>
        Task<RTCT> LoadReferenceCTAsync(string patientId, double? isocenterZ = null);

        /// <summary>
        /// </summary>
        /// <param name="patientId">환자 ID</param>
        Task<RTStructure> LoadRTStructureAsync(string patientId);

        /// <summary>
        /// </summary>
        /// <param name="patientId">환자 ID</param>
        Task<RTPlan> LoadRTPlanAsync(string patientId);

        /// <summary>
        /// </summary>
        /// <param name="patientId">환자 ID</param>
        Task<RTDose> LoadRTDoseAsync(string patientId);

        /// <summary>
        /// Loads DICOM files from the specified path
        /// </summary>
        /// <param name="directoryPath">Directory path containing DICOM files</param>
        /// <param name="patientId">Patient ID</param>
        /// <param name="type">CT type (CBCT or ReferenceCT)</param>
        /// <param name="isocenterZ">Optional isocenter Z coordinate</param>
        /// <returns>Loaded RTCT object</returns>
        Task<RTCT> LoadDicomFilesAsync(string directoryPath, string patientId, string type, double? isocenterZ = null);
    }
}
