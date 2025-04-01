using System;
using System.Collections.Generic;

namespace MCFAdaptApp.Domain.Models
{
    /// <summary>
    /// 참조 CT 이미지 정보를 나타내는 클래스
    /// </summary>
    public class ReferenceCT
    {
        /// <summary>
        /// 참조 CT의 고유 식별자
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 참조 CT의 이름
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 참조 CT의 경로
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 참조 CT의 생성 날짜
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// DICOM 파일 경로 목록
        /// </summary>
        public List<string> DicomFiles { get; set; } = new List<string>();

        /// <summary>
        /// 이미지 크기 (픽셀 단위)
        /// </summary>
        public int Width { get; set; }
        public int Height { get; set; }
        public int Depth { get; set; }

        /// <summary>
        /// 픽셀 간격 (mm 단위)
        /// </summary>
        public double PixelSpacingX { get; set; }
        public double PixelSpacingY { get; set; }
        public double SliceThickness { get; set; }

        /// <summary>
        /// 환자 ID
        /// </summary>
        public string PatientId { get; set; }

        /// <summary>
        /// CT 유형 (CBCT 또는 ReferenceCT)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Raw pixel data for the entire volume (Width * Height * Depth)
        /// </summary>
        public short[]? PixelData { get; set; }
    }
} 