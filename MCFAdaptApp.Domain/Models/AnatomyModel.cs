using System;
using System.Collections.ObjectModel;

namespace MCFAdaptApp.Domain.Models
{
    /// <summary>
    /// 환자의 해부학적 모델을 나타냅니다.
    /// </summary>
    public class AnatomyModel
    {
        /// <summary>
        /// 해부학적 모델의 이름
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 모델의 완료 상태
        /// </summary>
        public string Status { get; set; } = "Complete";
        
        /// <summary>
        /// 모델의 마지막 수정 날짜
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 이 모델에 연결된 참조 계획 목록
        /// </summary>
        public ObservableCollection<ReferencePlan> ReferencePlans { get; set; } = new ObservableCollection<ReferencePlan>();
    }
} 