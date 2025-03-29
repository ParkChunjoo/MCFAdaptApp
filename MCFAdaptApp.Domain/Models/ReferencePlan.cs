using System;

namespace MCFAdaptApp.Domain.Models
{
    /// <summary>
    /// 환자의 참조 계획을 나타냅니다.
    /// </summary>
    public class ReferencePlan
    {
        /// <summary>
        /// 참조 계획의 이름
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 계획의 완료 상태
        /// </summary>
        public string Status { get; set; } = "Complete";

        /// <summary>
        /// 계획의 마지막 수정 날짜
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// 계획의 설명
        /// </summary>
        public string Description { get; set; } = "Photon plan/3D-CRT/3 beams";
    }
}