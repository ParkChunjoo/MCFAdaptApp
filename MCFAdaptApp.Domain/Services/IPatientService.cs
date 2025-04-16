using System.Collections.Generic;
using System.Threading.Tasks;
using MCFAdaptApp.Domain.Models;

namespace MCFAdaptApp.Domain.Services
{
    /// <summary>
    /// 환자 데이터 관련 서비스 인터페이스
    /// </summary>
    public interface IPatientService
    {
        /// <summary>
        /// 모든 환자 목록을 가져옵니다.
        /// </summary>
        Task<IEnumerable<Patient>> GetAllPatientsAsync();

        /// <summary>
        /// 환자 ID로 환자 정보를 가져옵니다.
        /// </summary>
        Task<Patient?> GetPatientByIdAsync(string patientId);

        /// <summary>
        /// Gets RT structure sets for a patient by patient ID
        /// </summary>
        Task<List<RTStructure>> GetStructuresAsync(string patientId);

        /// <summary>
        /// Gets CT datasets for a patient by patient ID
        /// </summary>
        Task<List<RTCT>> GetCTDatasetsAsync(string patientId);

        /// <summary>
        /// Gets treatment plans for a patient by patient ID
        /// </summary>
        Task<List<RTPlan>> GetPlansAsync(string patientId);
    }
}