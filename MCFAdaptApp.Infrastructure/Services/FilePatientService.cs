using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MCFAdaptApp.Domain.Models;
using MCFAdaptApp.Domain.Services;
using MCFAdaptApp.Infrastructure.Helpers;

namespace MCFAdaptApp.Infrastructure.Services
{
    /// <summary>
    /// Implementation of IPatientService that reads patient data from a file
    /// </summary>
    public class FilePatientService : IPatientService
    {
        private readonly string _patientFilePath;
        private List<Patient> _cachedPatients;

        /// <summary>
        /// Initializes a new instance of the FilePatientService
        /// </summary>
        /// <param name="patientFilePath">Optional custom path to the patient data file</param>
        public FilePatientService(string patientFilePath = null)
        {
            LogHelper.Log("Initializing FilePatientService");

            // Use provided path or default to the specified location
            _patientFilePath = patientFilePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "MCFAaptData", "CBCTProjections", "patient_list.txt");

            // Ensure the directory exists
            var directory = Path.GetDirectoryName(_patientFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                LogHelper.Log($"Creating directory: {directory}");
                Directory.CreateDirectory(directory);
            }

            LogHelper.Log($"Using patient file: {_patientFilePath}");
        }

        /// <summary>
        /// Gets all patients asynchronously
        /// </summary>
        /// <returns>A collection of patients</returns>
        public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
        {
            LogHelper.Log("GetAllPatientsAsync called");

            // Return cached data if available
            if (_cachedPatients != null)
            {
                LogHelper.Log($"Returning {_cachedPatients.Count} cached patients");
                return _cachedPatients;
            }

            LogHelper.Log("Loading patients from file");

            // Create sample data if file doesn't exist
            if (!File.Exists(_patientFilePath))
            {
                LogHelper.LogWarning($"Patient file not found, creating sample data");
                //await CreateSampleDataAsync();
            }

            try
            {
                // Read all lines from the file
                var lines = await File.ReadAllLinesAsync(_patientFilePath);
                var patients = new List<Patient>();

                foreach (var line in lines)
                {
                    // Skip empty lines
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // Parse each line (assuming format: PatientID\tFirstName\tLastName)
                    var parts = line.Split('\t');
                    if (parts.Length >= 3)
                    {
                        // 현재 날짜를 기준으로 임의의 날짜/시간 생성 (최근 30일 이내)
                        var random = new Random();
                        var daysAgo = random.Next(0, 30); // 0-30일 전
                        var hoursAgo = random.Next(0, 24); // 0-24시간 전
                        var minutesAgo = random.Next(0, 60); // 0-60분 전

                        var lastModified = DateTime.Now.AddDays(-daysAgo).AddHours(-hoursAgo).AddMinutes(-minutesAgo);
                        var lastCBCTScanTime = lastModified.AddDays(-random.Next(0, 7)); // 수정일보다 0-7일 전

                        var patient = new Patient
                        {
                            PatientId = parts[0].Trim(),
                            FirstName = parts[1].Trim(),
                            LastName = parts[2].Trim(),
                            DateOfBirth = GetRandomDateOfBirth(),
                            LastModified = lastModified,
                            LastCBCTScanTime = lastCBCTScanTime
                        };

                        patients.Add(patient);

                        // 환자 디렉토리 생성 및 PlanInfo.txt 파일 생성
                        await CreatePatientDirectoryAndPlanInfoAsync(patient);
                    }
                }

                // Cache the results
                _cachedPatients = patients;

                LogHelper.Log($"Loaded {patients.Count} patients from file");
                return patients;
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error loading patients: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets a specific patient by ID asynchronously
        /// </summary>
        /// <param name="patientId">Patient ID to search for</param>
        /// <returns>Patient information or null if not found</returns>
        public async Task<Patient?> GetPatientByIdAsync(string patientId)
        {
            try
            {
                // Get all patients
                var patients = await GetAllPatientsAsync();

                // Find patient with matching ID
                return patients.FirstOrDefault(p => p.PatientId == patientId);
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error getting patient by ID: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates sample patient data
        /// </summary>
        private async Task CreateSampleDataAsync()
        {
            LogHelper.Log("Creating sample patient data");

            var samplePatients = new List<string>
            {
                "146125\tPatricia\tSmith",
                "168014\tRobert\tBrown",
                "187584\tRobert\tMoore",
                "172483\tRobert\tMiller",
                "167238\tJennifer\tSmith",
                "148796\tJohn\tMoore",
                "127323\tMary\tWilson",
                "102338\tJames\tJohnson",
                "136649\tRobert\tDavis",
                "144037\tPatricia\tSmith",
                "140281\tMichael\tWilson",
                "119088\tPatricia\tBrown",
                "148051\tRobert\tBrown"
            };

            await File.WriteAllLinesAsync(_patientFilePath, samplePatients);
            LogHelper.Log($"Sample data created with {samplePatients.Count} patients");
        }

        /// <summary>
        /// 임의의 생년월일을 생성합니다 (20-90세 사이)
        /// </summary>
        private DateTime? GetRandomDateOfBirth()
        {
            var random = new Random();
            var yearsAgo = random.Next(20, 90); // 20-90세
            var daysVariation = random.Next(0, 365); // 연도 내 임의의 날짜

            return DateTime.Now.AddYears(-yearsAgo).AddDays(-daysVariation);
        }

        /// <summary>
        /// 환자 디렉토리와 PlanInfo.txt 파일을 생성합니다.
        /// </summary>
        private async Task CreatePatientDirectoryAndPlanInfoAsync(Patient patient)
        {
            try
            {
                // 환자 디렉토리 경로 생성
                string patientDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "MCFAaptData", "PatientData", patient.PatientId);

                // 디렉토리가 없으면 생성
                if (!Directory.Exists(patientDirectory))
                {
                    Directory.CreateDirectory(patientDirectory);
                    LogHelper.Log($"Created directory: {patientDirectory}");
                }

                // PlanInfo.txt 파일 경로
                string planInfoPath = Path.Combine(patientDirectory, "PlanInfo.txt");

                // 파일이 이미 존재하면 생성하지 않음
                if (File.Exists(planInfoPath))
                {
                    LogHelper.Log($"PlanInfo.txt already exists for patient: {patient.PatientId}");
                    return;
                }

                // Create random RT structures and plans
                var random = new Random();
                var structureCount = random.Next(1, 3); // 1-2 structures

                var planInfoLines = new List<string>();

                for (int i = 1; i <= structureCount; i++)
                {
                    // Create structure name
                    string structureName = $"{patient.FirstName} {patient.LastName} Structure{(structureCount > 1 ? i.ToString() : "")}";

                    // Determine number of plans (1-2)
                    int planCount = random.Next(1, 3);

                    // Create plan names
                    var plans = new List<string>();
                    for (int j = 1; j <= planCount; j++)
                    {
                        plans.Add($"{patient.FirstName} {patient.LastName} TP 3DCRT{j}");
                    }

                    // Create tab-delimited line
                    string line = structureName + "\t" + string.Join("\t", plans);
                    planInfoLines.Add(line);
                }

                // Write to file
                await File.WriteAllLinesAsync(planInfoPath, planInfoLines);
                LogHelper.Log($"Created PlanInfo.txt for patient: {patient.PatientId} with {planInfoLines.Count} RT structures");
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error creating PlanInfo.txt for patient {patient.PatientId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets RT structure sets for a patient by patient ID
        /// </summary>
        public async Task<List<RTStructure>> GetStructuresAsync(string patientId)
        {
            var structures = new List<RTStructure>();

            try
            {
                // PlanInfo.txt 파일 경로
                string planInfoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "MCFAaptData", "PatientData", patientId, "PlanInfo.txt");

                // 파일이 존재하는지 확인
                if (!File.Exists(planInfoPath))
                {
                    LogHelper.LogWarning($"PlanInfo.txt not found for patient: {patientId}");
                    return structures;
                }

                LogHelper.Log($"Loading PlanInfo.txt for patient: {patientId}");

                // 파일 읽기 - 비동기 작업을 Task.Run으로 래핑하여 UI 스레드 차단 방지
                string[] lines = await Task.Run(() => File.ReadAllLines(planInfoPath));

                // 각 줄 처리
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // 탭으로 구분된 데이터 파싱
                    string[] parts = line.Split('\t');

                    if (parts.Length < 2)
                        continue;

                    // 첫 번째 부분은 AnatomyModel 이름
                    string anatomyModelName = parts[0].Trim();

                    // Check if a structure with the same name already exists
                    bool structureExists = structures.Any(s => s.Name == anatomyModelName);
                    if (structureExists)
                    {
                        LogHelper.LogWarning($"Skipping duplicate RTStructure: {anatomyModelName}");
                        continue;
                    }

                    // Create RTStructure
                    var rtStructure = new RTStructure
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = anatomyModelName,
                        Status = "Complete",
                        Type = "Original",
                        ModifiedDate = DateTime.Now.AddDays(-1),
                        CreatedDate = DateTime.Now.AddDays(-7),
                        PatientId = patientId,
                        Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "MCFAaptData", "PatientData", patientId)
                    };

                    // Second part onwards are plan names
                    for (int i = 1; i < parts.Length; i++)
                    {
                        string planName = parts[i].Trim();

                        if (!string.IsNullOrWhiteSpace(planName))
                        {
                            // Create RTPlan and add to structure
                            var rtPlan = new RTPlan
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = planName,
                                Status = "Complete",
                                Type = "Reference",
                                ModifiedDate = DateTime.Now,
                                CreatedDate = DateTime.Now.AddDays(-5),
                                PatientId = patientId,
                                RTStructureId = rtStructure.Id,
                                Description = "Photon plan/3D-CRT/3 beams"
                            };

                            rtStructure.Plans.Add(rtPlan);
                        }
                    }

                    // Add structure to the list
                    structures.Add(rtStructure);
                }

                LogHelper.Log($"Loaded {structures.Count} RT structures for patient: {patientId}");
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error loading RT structures for patient {patientId}: {ex.Message}");
            }

            return structures;
        }

        /// <summary>
        /// Gets CT datasets for a patient by patient ID
        /// </summary>
        public async Task<List<RTCT>> GetCTDatasetsAsync(string patientId)
        {
            var datasets = new List<RTCT>();

            try
            {
                // For now, we'll create a placeholder CT dataset
                // In a real implementation, this would load from DICOM files
                var referenceCT = new RTCT
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"ReferenceCT_{patientId}",
                    Type = "ReferenceCT",
                    PatientId = patientId,
                    CreatedDate = DateTime.Now.AddDays(-10),
                    AcquisitionDate = DateTime.Now.AddDays(-10),
                    Modality = "CT",
                    SeriesDescription = "Planning CT",
                    Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "MCFAaptData", "PlanData")
                };

                datasets.Add(referenceCT);

                // Add a CBCT dataset if available
                var cbctPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "MCFAaptData", "CBCT");
                if (Directory.Exists(cbctPath))
                {
                    var cbct = new RTCT
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = $"CBCT_{patientId}",
                        Type = "CBCT",
                        PatientId = patientId,
                        CreatedDate = DateTime.Now.AddDays(-1),
                        AcquisitionDate = DateTime.Now.AddDays(-1),
                        Modality = "CBCT",
                        SeriesDescription = "Cone Beam CT",
                        Path = cbctPath
                    };

                    datasets.Add(cbct);
                }

                LogHelper.Log($"Created {datasets.Count} CT datasets for patient: {patientId}");
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error creating CT datasets for patient {patientId}: {ex.Message}");
            }

            return datasets;
        }

        /// <summary>
        /// Gets treatment plans for a patient by patient ID
        /// </summary>
        public async Task<List<RTPlan>> GetPlansAsync(string patientId)
        {
            var plans = new List<RTPlan>();

            try
            {
                // Get structures first, as plans are associated with structures
                var structures = await GetStructuresAsync(patientId);

                // Extract plans from structures
                foreach (var structure in structures)
                {
                    foreach (var plan in structure.Plans)
                    {
                        plans.Add(plan);
                    }
                }

                LogHelper.Log($"Extracted {plans.Count} plans from structures for patient: {patientId}");
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error getting plans for patient {patientId}: {ex.Message}");
            }

            return plans;
        }
    }
}