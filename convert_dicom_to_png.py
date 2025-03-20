import os
import pydicom
import numpy as np
from PIL import Image
import glob
import sys

def convert_dicom_to_png(source_folder, destination_folder, output_prefix):
    """
    Convert DICOM files to PNG images.
    
    Args:
        source_folder (str): Path to the folder containing DICOM files
        destination_folder (str): Path to the folder where PNG files will be saved
        output_prefix (str): Prefix for the output PNG filenames
    """
    # Ensure destination folder exists
    os.makedirs(destination_folder, exist_ok=True)
    
    # Get all DICOM files that start with CT.
    dicom_files = glob.glob(os.path.join(source_folder, 'CT*.dcm'))
    
    if not dicom_files:
        print(f"No DICOM files found in {source_folder}")
        return
    
    print(f"Found {len(dicom_files)} DICOM files in {source_folder}")
    
    # Read all DICOM files and store with their slice location
    slices = []
    for dicom_file in dicom_files:
        try:
            ds = pydicom.dcmread(dicom_file)
            # Get slice location (Z coordinate)
            if hasattr(ds, 'SliceLocation'):
                slice_location = ds.SliceLocation
            elif hasattr(ds, 'ImagePositionPatient'):
                # If SliceLocation is not available, use the Z coordinate from ImagePositionPatient
                slice_location = ds.ImagePositionPatient[2]
            else:
                print(f"Warning: Cannot determine slice location for {dicom_file}, skipping")
                continue
                
            slices.append((dicom_file, slice_location, ds))
        except Exception as e:
            print(f"Error reading {dicom_file}: {e}")
    
    # Sort slices by location (from top to bottom, which is typically from highest to lowest Z value)
    slices.sort(key=lambda x: x[1], reverse=True)
    
    print(f"Successfully read {len(slices)} slices")
    
    # Convert each slice to PNG
    for i, (dicom_file, _, ds) in enumerate(slices, 1):
        try:
            # Get pixel array data
            pixel_array = ds.pixel_array
            
            # Normalize to 0-255 for 8-bit grayscale
            if pixel_array.max() > pixel_array.min():
                # Scale to 0-255 range
                scaled_array = ((pixel_array - pixel_array.min()) / 
                               (pixel_array.max() - pixel_array.min())) * 255.0
            else:
                scaled_array = pixel_array * 0
                
            # Convert to 8-bit unsigned integer
            scaled_array = scaled_array.astype(np.uint8)
            
            # Create PIL Image
            image = Image.fromarray(scaled_array)
            
            # Save as PNG with sequential numbering
            output_filename = f"{output_prefix}{i:03d}.png"
            output_path = os.path.join(destination_folder, output_filename)
            image.save(output_path)
            
            print(f"Converted {os.path.basename(dicom_file)} to {output_filename}")
            
        except Exception as e:
            print(f"Error converting {dicom_file}: {e}")
    
    print(f"Conversion complete. {len(slices)} files converted to {destination_folder}")

if __name__ == "__main__":
    # Define conversion tasks
    conversion_tasks = [
        {
            'source': os.path.join('MCFAaptData', 'Dcm', 'CBCT', '03152023'),
            'destination': os.path.join('MCFAaptData', 'ImageFiles', 'CBCT'),
            'prefix': 'CBCT'
        },
        {
            'source': os.path.join('MCFAaptData', 'Dcm', 'PlanData'),
            'destination': os.path.join('MCFAaptData', 'ImageFiles', 'PlanData'),
            'prefix': 'REFCT'
        }
    ]
    
    # Check if specific task is requested via command line
    if len(sys.argv) > 1 and sys.argv[1] in ['1', '2']:
        task_index = int(sys.argv[1]) - 1
        tasks_to_run = [conversion_tasks[task_index]]
    else:
        # Run all tasks by default
        tasks_to_run = conversion_tasks
    
    # Run the selected conversion tasks
    for task in tasks_to_run:
        print(f"\nProcessing task: {task['source']} -> {task['destination']} with prefix {task['prefix']}")
        convert_dicom_to_png(task['source'], task['destination'], task['prefix'])
