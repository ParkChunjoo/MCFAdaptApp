using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MCFAdaptApp.Domain.Models;
using MCFAdaptApp.Domain.Services;
using FellowOakDicom;
using MCFAdaptApp.Infrastructure.Helpers;

namespace MCFAdaptApp.Infrastructure.Services
{
    /// <summary>
    /// DICOM 파일 처리를 위한 서비스 구현
    /// </summary>
    public class DicomService : IDicomService
    {
        private readonly string _basePath;
        private readonly string _cbctFolder = "CBCT";
        private readonly string _planDataFolder = "PlanData";

        public DicomService()
        {
            // Set the base path to the MCFAaptData directory in the project folder
            _basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "MCFAaptData", "Dcm");
            
            // fo-dicom 초기화
            new DicomSetupBuilder()
                .RegisterServices(s => s.AddFellowOakDicom())
                .Build();
        }

        /// <summary>
        /// 지정된 경로에서 CBCT DICOM 파일을 로드합니다.
        /// </summary>
        /// <param name="patientId">환자 ID</param>
        /// <returns>로드된 CBCT 객체</returns>
        public async Task<ReferenceCT> LoadCBCTAsync(string patientId)
        {
            LogHelper.Log($"Loading CBCT DICOM files for patient: {patientId}");
            
            string cbctPath = Path.Combine(_basePath, _cbctFolder);
            
            if (!Directory.Exists(cbctPath))
            {
                LogHelper.LogWarning($"CBCT directory not found: {cbctPath}");
                return null;
            }
            
            return await LoadDicomFilesAsync(cbctPath, patientId, "CBCT");
        }

        /// <summary>
        /// 지정된 경로에서 참조 CT DICOM 파일을 로드합니다.
        /// </summary>
        /// <param name="patientId">환자 ID</param>
        /// <returns>로드된 참조 CT 객체</returns>
        public async Task<ReferenceCT> LoadReferenceCTAsync(string patientId)
        {
            LogHelper.Log($"Loading Reference CT DICOM files for patient: {patientId}");
            
            string planDataPath = Path.Combine(_basePath, _planDataFolder);
            
            if (!Directory.Exists(planDataPath))
            {
                LogHelper.LogWarning($"PlanData directory not found: {planDataPath}");
                return null;
            }
            
            return await LoadDicomFilesAsync(planDataPath, patientId, "ReferenceCT");
        }

        /// <summary>
        /// 지정된 경로의 DICOM 파일들을 로드합니다.
        /// </summary>
        /// <param name="directoryPath">DICOM 파일이 있는 디렉토리 경로</param>
        /// <param name="patientId">환자 ID</param>
        /// <param name="type">CT 유형 (CBCT 또는 ReferenceCT)</param>
        /// <returns>로드된 ReferenceCT 객체</returns>
        public async Task<ReferenceCT> LoadDicomFilesAsync(string directoryPath, string patientId, string type)
        {
            try
            {
                LogHelper.Log($"Searching for DICOM files in: {directoryPath}");
                
                // 디렉토리에서 모든 DICOM 파일 찾기
                var dicomFiles = await Task.Run(() => 
                {
                    return Directory.GetFiles(directoryPath, "*.dcm", SearchOption.AllDirectories)
                        .Union(Directory.GetFiles(directoryPath, "*.DCM", SearchOption.AllDirectories))
                        .ToList();
                });
                
                if (dicomFiles.Count == 0)
                {
                    LogHelper.LogWarning($"No DICOM files found in: {directoryPath}");
                    return null;
                }
                
                LogHelper.Log($"Found {dicomFiles.Count} DICOM files");
                
                // 첫 번째 DICOM 파일을 로드하여 메타데이터 확인
                var firstDicom = await Task.Run(() => DicomFile.Open(dicomFiles[0]));
                
                // ReferenceCT 객체 생성
                var referenceCT = new ReferenceCT
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = type == "CBCT" ? $"CBCT_{patientId}" : $"ReferenceCT_{patientId}",
                    Path = directoryPath,
                    CreatedDate = DateTime.Now,
                    DicomFiles = dicomFiles,
                    PatientId = patientId,
                    Type = type
                };
                
                // DICOM 메타데이터에서 이미지 정보 추출
                if (firstDicom != null)
                {
                    try
                    {
                        var dataset = firstDicom.Dataset;
                        
                        // 이미지 크기 정보 추출
                        referenceCT.Width = dataset.GetValue<ushort>(DicomTag.Columns, 0);
                        referenceCT.Height = dataset.GetValue<ushort>(DicomTag.Rows, 0);
                        referenceCT.Depth = dicomFiles.Count; // 슬라이스 수
                        
                        // 픽셀 간격 정보 추출
                        if (dataset.Contains(DicomTag.PixelSpacing))
                        {
                            var pixelSpacing = dataset.GetValues<double>(DicomTag.PixelSpacing);
                            if (pixelSpacing.Length >= 2)
                            {
                                referenceCT.PixelSpacingX = pixelSpacing[0];
                                referenceCT.PixelSpacingY = pixelSpacing[1];
                            }
                        }
                        
                        // 슬라이스 두께 정보 추출
                        if (dataset.Contains(DicomTag.SliceThickness))
                        {
                            referenceCT.SliceThickness = dataset.GetValue<double>(DicomTag.SliceThickness, 0);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogError($"Error extracting DICOM metadata: {ex.Message}");
                    }
                }
                
                LogHelper.Log($"Successfully loaded {type} for patient: {patientId}");
                return referenceCT;
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error loading DICOM files: {ex.Message}");
                return null;
            }
        }
    }
} 