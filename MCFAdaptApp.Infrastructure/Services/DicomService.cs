using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MCFAdaptApp.Domain.Models;
using MCFAdaptApp.Domain.Services;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using MCFAdaptApp.Infrastructure.Helpers;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;

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
        /// Loads CBCT DICOM files from the specified path
        /// </summary>
        /// <param name="patientId">Patient ID</param>
        /// <returns>Loaded CBCT object</returns>
        public async Task<RTCT> LoadCBCTAsync(string patientId)
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
        /// Loads reference CT DICOM files from the specified path
        /// </summary>
        /// <param name="patientId">Patient ID</param>
        /// <param name="isocenterZ">Optional isocenter Z coordinate</param>
        /// <returns>Loaded reference CT object</returns>
        public async Task<RTCT> LoadReferenceCTAsync(string patientId, double? isocenterZ = null)
        {
            LogHelper.Log($"Loading Reference CT DICOM files for patient: {patientId}" + (isocenterZ.HasValue ? $" near Z={isocenterZ.Value}" : ""));

            string planDataPath = Path.Combine(_basePath, _planDataFolder);

            if (!Directory.Exists(planDataPath))
            {
                LogHelper.LogWarning($"PlanData directory not found: {planDataPath}");
                return null;
            }

            return await LoadDicomFilesAsync(planDataPath, patientId, "ReferenceCT", isocenterZ);
        }

        /// <summary>
        /// </summary>
        /// <param name="patientId">환자 ID</param>
        public async Task<RTStructure> LoadRTStructureAsync(string patientId)
        {
            LogHelper.Log($"Loading RT Structure DICOM files for patient: {patientId}");

            string planDataPath = Path.Combine(_basePath, _planDataFolder);

            if (!Directory.Exists(planDataPath))
            {
                LogHelper.LogWarning($"PlanData directory not found: {planDataPath}");
                return null;
            }

            try
            {
                LogHelper.Log($"Searching for RT Structure files in: {planDataPath}");

                var rtStructureFiles = await Task.Run(() =>
                {
                    return Directory.GetFiles(planDataPath, "RS*.dcm", SearchOption.AllDirectories)
                        .Union(Directory.GetFiles(planDataPath, "RS*.DCM", SearchOption.AllDirectories))
                        .Union(Directory.GetFiles(planDataPath, "RS*.dicom", SearchOption.AllDirectories))
                        .Union(Directory.GetFiles(planDataPath, "RS*.DICOM", SearchOption.AllDirectories))
                        .ToList();
                });

                if (rtStructureFiles.Count == 0)
                {
                    LogHelper.LogWarning($"No RT Structure files found in: {planDataPath}");
                    return null;
                }

                LogHelper.Log($"Found {rtStructureFiles.Count} RT Structure files");

                var dicomFile = await Task.Run(() => DicomFile.Open(rtStructureFiles[0]));
                var dataset = dicomFile.Dataset;

                var rtStructure = new RTStructure
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"RTStructure_{patientId}",
                    Path = planDataPath,
                    CreatedDate = DateTime.Now,
                    DicomFile = rtStructureFiles[0],
                    PatientId = patientId
                };

                try
                {
                    if (dataset.Contains(DicomTag.StructureSetROISequence))
                    {
                        var structureSetSequence = dataset.GetSequence(DicomTag.StructureSetROISequence);
                        foreach (var structureItem in structureSetSequence)
                        {
                            if (structureItem.Contains(DicomTag.ROIName))
                            {
                                rtStructure.StructureNames.Add(structureItem.GetString(DicomTag.ROIName));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogError($"Error extracting structure names: {ex.Message}");
                }

                LogHelper.Log($"Successfully loaded RT Structure for patient: {patientId}");
                return rtStructure;
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error loading RT Structure file: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="patientId">환자 ID</param>
        public async Task<RTPlan> LoadRTPlanAsync(string patientId)
        {
            LogHelper.Log($"Loading RT Plan DICOM files for patient: {patientId}");

            string planDataPath = Path.Combine(_basePath, _planDataFolder);

            if (!Directory.Exists(planDataPath))
            {
                LogHelper.LogWarning($"PlanData directory not found: {planDataPath}");
                return null;
            }

            try
            {
                LogHelper.Log($"Searching for RT Plan files in: {planDataPath}");

                var rtPlanFiles = await Task.Run(() =>
                {
                    return Directory.GetFiles(planDataPath, "RP*.dcm", SearchOption.AllDirectories)
                        .Union(Directory.GetFiles(planDataPath, "RP*.DCM", SearchOption.AllDirectories))
                        .Union(Directory.GetFiles(planDataPath, "RP*.dicom", SearchOption.AllDirectories))
                        .Union(Directory.GetFiles(planDataPath, "RP*.DICOM", SearchOption.AllDirectories))
                        .ToList();
                });

                if (rtPlanFiles.Count == 0)
                {
                    // Extended logging - list all files in the directory to help diagnose the issue
                    var allFiles = await Task.Run(() => Directory.GetFiles(planDataPath, "*.*", SearchOption.AllDirectories));
                    LogHelper.LogWarning($"No RT Plan files found in: {planDataPath}");
                    LogHelper.Log($"Directory contains {allFiles.Length} files. First 10 files (or less):");
                    for (int i = 0; i < Math.Min(10, allFiles.Length); i++)
                    {
                        LogHelper.Log($"  - {Path.GetFileName(allFiles[i])}");
                    }
                    return null;
                }

                LogHelper.Log($"Found {rtPlanFiles.Count} RT Plan files");
                foreach (var rtPlanFile in rtPlanFiles)
                {
                    LogHelper.Log($"  - {Path.GetFileName(rtPlanFile)}");
                }

                var dicomFile = await Task.Run(() => DicomFile.Open(rtPlanFiles[0]));
                var dataset = dicomFile.Dataset;

                var rtPlan = new RTPlan
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"RTPlan_{patientId}",
                    Path = planDataPath,
                    CreatedDate = DateTime.Now,
                    DicomFile = rtPlanFiles[0],
                    PatientId = patientId
                };

                try
                {
                    if (dataset.Contains(DicomTag.RTPlanLabel))
                    {
                        rtPlan.PlanLabel = dataset.GetString(DicomTag.RTPlanLabel);
                    }

                    if (dataset.Contains(DicomTag.RTPlanDescription))
                    {
                        rtPlan.Description = dataset.GetString(DicomTag.RTPlanDescription);
                    }

                    // First try to get isocenter position directly (though this is uncommon)
                    if (dataset.Contains(DicomTag.IsocenterPosition))
                    {
                        rtPlan.IsocenterPosition = dataset.GetValues<double>(DicomTag.IsocenterPosition);
                        LogHelper.Log($"Extracted Isocenter using top-level tag (300a,012c): X={rtPlan.IsocenterPosition[0]}, Y={rtPlan.IsocenterPosition[1]}, Z={rtPlan.IsocenterPosition[2]}");
                    }
                    // If not found at top level, look in BeamSequence (more common location)
                    else if (dataset.Contains(DicomTag.BeamSequence))
                    {
                        LogHelper.Log("Looking for isocenter position in BeamSequence...");
                        var beamSequence = dataset.GetSequence(DicomTag.BeamSequence);

                        // Try to find isocenter in any beam
                        bool foundIsocenter = false;
                        foreach (var beamItem in beamSequence)
                        {
                            if (beamItem.Contains(DicomTag.IsocenterPosition))
                            {
                                rtPlan.IsocenterPosition = beamItem.GetValues<double>(DicomTag.IsocenterPosition);
                                LogHelper.Log($"Extracted Isocenter from BeamSequence: X={rtPlan.IsocenterPosition[0]}, Y={rtPlan.IsocenterPosition[1]}, Z={rtPlan.IsocenterPosition[2]}");
                                foundIsocenter = true;
                                break;
                            }
                            // Also check in ControlPointSequence if present
                            else if (beamItem.Contains(DicomTag.ControlPointSequence))
                            {
                                var controlPointSequence = beamItem.GetSequence(DicomTag.ControlPointSequence);
                                foreach (var controlPoint in controlPointSequence)
                                {
                                    if (controlPoint.Contains(DicomTag.IsocenterPosition))
                                    {
                                        rtPlan.IsocenterPosition = controlPoint.GetValues<double>(DicomTag.IsocenterPosition);
                                        LogHelper.Log($"Extracted Isocenter from ControlPointSequence: X={rtPlan.IsocenterPosition[0]}, Y={rtPlan.IsocenterPosition[1]}, Z={rtPlan.IsocenterPosition[2]}");
                                        foundIsocenter = true;
                                        break;
                                    }
                                }
                                if (foundIsocenter) break;
                            }
                        }

                        if (!foundIsocenter)
                        {
                            LogHelper.LogWarning("Could not find IsocenterPosition in BeamSequence or ControlPointSequence");
                        }
                    }
                    else
                    {
                        LogHelper.LogWarning("RT Plan file does not contain IsocenterPosition tag (300a,012c) or BeamSequence");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogError($"Error extracting plan information: {ex.Message}");
                }

                LogHelper.Log($"Successfully loaded RT Plan for patient: {patientId}");
                return rtPlan;
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error loading RT Plan file: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="patientId">환자 ID</param>
        public async Task<RTDose> LoadRTDoseAsync(string patientId)
        {
            LogHelper.Log($"Loading RT Dose DICOM files for patient: {patientId}");

            string planDataPath = Path.Combine(_basePath, _planDataFolder);

            if (!Directory.Exists(planDataPath))
            {
                LogHelper.LogWarning($"PlanData directory not found: {planDataPath}");
                return null;
            }

            try
            {
                LogHelper.Log($"Searching for RT Dose files in: {planDataPath}");

                var rtDoseFiles = await Task.Run(() =>
                {
                    return Directory.GetFiles(planDataPath, "RD*.dcm", SearchOption.AllDirectories)
                        .Union(Directory.GetFiles(planDataPath, "RD*.DCM", SearchOption.AllDirectories))
                        .Union(Directory.GetFiles(planDataPath, "RD*.dicom", SearchOption.AllDirectories))
                        .Union(Directory.GetFiles(planDataPath, "RD*.DICOM", SearchOption.AllDirectories))
                        .ToList();
                });

                if (rtDoseFiles.Count == 0)
                {
                    LogHelper.LogWarning($"No RT Dose files found in: {planDataPath}");
                    return null;
                }

                LogHelper.Log($"Found {rtDoseFiles.Count} RT Dose files");

                var dicomFile = await Task.Run(() => DicomFile.Open(rtDoseFiles[0]));
                var dataset = dicomFile.Dataset;

                var rtDose = new RTDose
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"RTDose_{patientId}",
                    Path = planDataPath,
                    CreatedDate = DateTime.Now,
                    DicomFile = rtDoseFiles[0],
                    PatientId = patientId
                };

                try
                {
                    if (dataset.Contains(DicomTag.DoseUnits))
                    {
                        rtDose.DoseUnits = dataset.GetString(DicomTag.DoseUnits);
                    }

                    if (dataset.Contains(DicomTag.DoseType))
                    {
                        rtDose.DoseType = dataset.GetString(DicomTag.DoseType);
                    }

                    if (dataset.Contains(DicomTag.DoseGridScaling))
                    {
                        rtDose.DoseMax = dataset.GetValue<double>(DicomTag.DoseGridScaling, 0);
                    }

                    if (dataset.Contains(DicomTag.Columns) && dataset.Contains(DicomTag.Rows))
                    {
                        rtDose.DoseGridSize[0] = dataset.GetValue<int>(DicomTag.Columns, 0);
                        rtDose.DoseGridSize[1] = dataset.GetValue<int>(DicomTag.Rows, 0);
                        rtDose.DoseGridSize[2] = dataset.Contains(DicomTag.NumberOfFrames) ?
                            dataset.GetValue<int>(DicomTag.NumberOfFrames, 0) : 1;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogError($"Error extracting dose information: {ex.Message}");
                }

                LogHelper.Log($"Successfully loaded RT Dose for patient: {patientId}");
                return rtDose;
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error loading RT Dose file: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Loads DICOM files from the specified path
        /// </summary>
        /// <param name="directoryPath">Directory path containing DICOM files</param>
        /// <param name="patientId">Patient ID</param>
        /// <param name="type">CT type (CBCT or ReferenceCT)</param>
        /// <param name="isocenterZ">Optional Z-coordinate of the isocenter (used for ReferenceCT)</param>
        /// <returns>Loaded RTCT object</returns>
        public async Task<RTCT> LoadDicomFilesAsync(string directoryPath, string patientId, string type, double? isocenterZ = null)
        {
            try
            {
                LogHelper.Log($"Searching for DICOM files in: {directoryPath}");

                // Find all DICOM files in the directory
                var allDcmFiles = await Task.Run(() =>
                {
                    return Directory.GetFiles(directoryPath, "*.dcm", SearchOption.AllDirectories)
                        .Union(Directory.GetFiles(directoryPath, "*.DCM", SearchOption.AllDirectories))
                        .Union(Directory.GetFiles(directoryPath, "*.dicom", SearchOption.AllDirectories))
                        .Union(Directory.GetFiles(directoryPath, "*.DICOM", SearchOption.AllDirectories))
                        .ToList();
                });

                // Filter for CT Modality only
                var dicomFilePaths = new List<string>();
                foreach (var filePath in allDcmFiles)
                {
                    try
                    {
                        var dcm = DicomFile.Open(filePath, FileReadOption.SkipLargeTags);
                        if (dcm.Dataset.GetSingleValueOrDefault(DicomTag.Modality, "") == "CT")
                        {
                            dicomFilePaths.Add(filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogWarning($"Could not read modality from {filePath}, skipping: {ex.Message}");
                    }
                }

                if (dicomFilePaths.Count == 0)
                {
                    LogHelper.LogWarning($"No DICOM files found in: {directoryPath}");
                    return null;
                }

                LogHelper.Log($"Found {dicomFilePaths.Count} DICOM files");

                // Sort files (important for correct slice order)
                // Assuming filenames contain slice number or can be sorted naturally
                dicomFilePaths.Sort(); // Basic sort, might need more robust sorting based on DICOM tags

                // Load the first DICOM file to get metadata
                var firstDicomFile = await Task.Run(() => DicomFile.Open(dicomFilePaths[0]));
                var firstDataset = firstDicomFile.Dataset;

                // Extract metadata
                int width = firstDataset.GetValue<ushort>(DicomTag.Columns, 0);
                int height = firstDataset.GetValue<ushort>(DicomTag.Rows, 0);
                int depth = dicomFilePaths.Count;
                double pixelSpacingX = 1.0;
                double pixelSpacingY = 1.0;
                double sliceThickness = 1.0;

                if (firstDataset.Contains(DicomTag.PixelSpacing))
                {
                    var pixelSpacing = firstDataset.GetValues<double>(DicomTag.PixelSpacing);
                    if (pixelSpacing.Length >= 2)
                    {
                        pixelSpacingX = pixelSpacing[0];
                        pixelSpacingY = pixelSpacing[1];
                    }
                }

                if (firstDataset.Contains(DicomTag.SliceThickness))
                {
                    sliceThickness = firstDataset.GetValue<double>(DicomTag.SliceThickness, 0);
                }

                // --- Determine Slice Index ---
                int displaySliceIndex = depth / 2; // Default to center slice

                if (type == "ReferenceCT" && isocenterZ.HasValue)
                {
                    LogHelper.Log($"Finding ReferenceCT slice closest to Isocenter Z: {isocenterZ.Value}");
                    int closestSliceIdx = -1;
                    double minDifference = double.MaxValue;

                    // Iterate through files to find the Z position of each slice
                    for (int i = 0; i < depth; i++)
                    {
                        try
                        {
                            var dcm = DicomFile.Open(dicomFilePaths[i]);
                            if (dcm.Dataset.Contains(DicomTag.ImagePositionPatient))
                            {
                                var imagePosition = dcm.Dataset.GetValues<double>(DicomTag.ImagePositionPatient);
                                if (imagePosition != null && imagePosition.Length == 3)
                                {
                                    double sliceZ = imagePosition[2];
                                    double diff = Math.Abs(sliceZ - isocenterZ.Value);
                                    if (diff < minDifference)
                                    {
                                        minDifference = diff;
                                        closestSliceIdx = i;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.LogWarning($"Could not read ImagePositionPatient from {dicomFilePaths[i]}: {ex.Message}");
                        }
                    }

                    if (closestSliceIdx != -1)
                    {
                        displaySliceIndex = closestSliceIdx;
                        LogHelper.Log($"Found closest slice at index {displaySliceIndex} (Z={DicomFile.Open(dicomFilePaths[displaySliceIndex]).Dataset.GetValues<double>(DicomTag.ImagePositionPatient)[2]}) with difference {minDifference}");
                    }
                    else
                    {
                        LogHelper.LogWarning("Could not determine closest slice based on ImagePositionPatient. Defaulting to center slice.");
                    }
                }
                else if (type == "ReferenceCT" && !isocenterZ.HasValue)
                {
                    LogHelper.Log("Isocenter Z not provided for ReferenceCT. Defaulting to center slice.");
                    LogHelper.Log($"Display slice index set to center slice: {displaySliceIndex} of {depth} total slices");

                    // Log slice position information for the center slice
                    try
                    {
                        var centerSliceDcm = DicomFile.Open(dicomFilePaths[displaySliceIndex]);
                        if (centerSliceDcm.Dataset.Contains(DicomTag.ImagePositionPatient))
                        {
                            var centerPos = centerSliceDcm.Dataset.GetValues<double>(DicomTag.ImagePositionPatient);
                            if (centerPos != null && centerPos.Length == 3)
                            {
                                LogHelper.Log($"Center slice position: X={centerPos[0]}, Y={centerPos[1]}, Z={centerPos[2]}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogWarning($"Could not read position of center slice: {ex.Message}");
                    }
                }
                // --- End Determine Slice Index ---

                // Create the RTCT object
                var rtct = new RTCT
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = type == "CBCT" ? $"CBCT_{patientId}" : $"ReferenceCT_{patientId}",
                    Path = directoryPath,
                    CreatedDate = DateTime.Now,
                    AcquisitionDate = DateTime.Now.AddDays(-1),
                    DicomFiles = dicomFilePaths,
                    PatientId = patientId,
                    Type = type,
                    Modality = type == "CBCT" ? "CBCT" : "CT",
                    SeriesDescription = type == "CBCT" ? "Cone Beam CT" : "Planning CT",
                    Width = width,
                    Height = height,
                    Depth = depth,
                    PixelSpacingX = pixelSpacingX,
                    PixelSpacingY = pixelSpacingY,
                    SliceThickness = sliceThickness,
                    DisplaySliceIndex = displaySliceIndex
                };

                // Load pixel data from all slices
                LogHelper.Log($"Loading pixel data for {depth} slices...");
                short[] allPixelData = new short[width * height * depth];
                int sliceSize = width * height;

                // Use Parallel.For for potentially faster loading on multi-core systems
                await Task.Run(() =>
                {
                    Parallel.For(0, depth, i =>
                    {
                        try
                        {
                            var dicomFile = DicomFile.Open(dicomFilePaths[i]);
                            var dataset = dicomFile.Dataset;

                            // Check if PixelData tag exists
                            if (dataset.Contains(DicomTag.PixelData))
                            {
                                var pixelDataElement = dataset.GetDicomItem<DicomElement>(DicomTag.PixelData);
                                var buffer = pixelDataElement.Buffer; // Get the buffer directly

                                // Get metadata necessary for interpretation
                                var pixelRepresentation = dataset.GetValue<ushort>(DicomTag.PixelRepresentation, 0);
                                var bitsStored = dataset.GetValue<ushort>(DicomTag.BitsStored, 0);
                                var highBit = dataset.GetValue<ushort>(DicomTag.HighBit, 0);
                                var rescaleIntercept = dataset.GetValue<double>(DicomTag.RescaleIntercept, 0);
                                var rescaleSlope = dataset.GetValue<double>(DicomTag.RescaleSlope, 0);

                                // Ensure buffer is not null and has expected length for 16-bit data
                                if (buffer != null && bitsStored == 16 && buffer.Size == sliceSize * 2)
                                {
                                    // Copy buffer segment to the correct position in the large array
                                    Buffer.BlockCopy(buffer.Data, 0, allPixelData, i * sliceSize * sizeof(short), (int)buffer.Size);
                                }
                                else if (buffer == null)
                                {
                                    LogHelper.LogWarning($"Pixel data buffer is null in file {dicomFilePaths[i]}");
                                }
                                else
                                {
                                    // Handle other bit depths or formats if necessary
                                    LogHelper.LogWarning($"Unsupported pixel format or unexpected buffer length in file {dicomFilePaths[i]}: BitsStored={bitsStored}, BufferLength={buffer.Size}, ExpectedLength={sliceSize * 2}");
                                }
                            }
                            else
                            {
                                LogHelper.LogWarning($"PixelData tag not found in file {dicomFilePaths[i]}");
                            }
                        }
                        catch (Exception sliceEx)
                        {
                            LogHelper.LogError($"Error processing slice {i} ({Path.GetFileName(dicomFilePaths[i])}): {sliceEx.Message}");
                            // Optionally fill this slice data with a default value (e.g., 0)
                        }
                    });
                });

                rtct.PixelData = allPixelData;
                LogHelper.Log($"Successfully loaded pixel data for {type} ({width}x{height}x{depth}) for patient: {patientId}");

                return rtct;
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error loading DICOM files: {ex.Message}");
                LogHelper.LogException(ex);
                return null;
            }
        }
    }
}
