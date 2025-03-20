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
        /// 환자 ID로 해당 환자의 AnatomyModel 목록을 가져옵니다.
        /// </summary>
        Task<List<AnatomyModel>> GetAnatomyModelsAsync(string patientId);
    }
} 