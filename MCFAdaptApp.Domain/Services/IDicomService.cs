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
        /// 지정된 경로에서 CBCT DICOM 파일을 로드합니다.
        /// </summary>
        /// <param name="patientId">환자 ID</param>
        /// <returns>로드된 CBCT 객체</returns>
        Task<ReferenceCT> LoadCBCTAsync(string patientId);

        /// <summary>
        /// 지정된 경로에서 참조 CT DICOM 파일을 로드합니다.
        /// </summary>
        /// <param name="patientId">환자 ID</param>
        /// <returns>로드된 참조 CT 객체</returns>
        Task<ReferenceCT> LoadReferenceCTAsync(string patientId);

        /// <summary>
        /// 지정된 경로의 DICOM 파일들을 로드합니다.
        /// </summary>
        /// <param name="directoryPath">DICOM 파일이 있는 디렉토리 경로</param>
        /// <param name="patientId">환자 ID</param>
        /// <param name="type">CT 유형 (CBCT 또는 ReferenceCT)</param>
        /// <returns>로드된 ReferenceCT 객체</returns>
        Task<ReferenceCT> LoadDicomFilesAsync(string directoryPath, string patientId, string type);
    }
} 