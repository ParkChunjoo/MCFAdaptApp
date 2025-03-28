using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MCFAdaptApp.Avalonia.ViewModels
{
    public class SelectPatientWindowViewModel : INotifyPropertyChanged
    {
        private string _patientName = string.Empty;
        private string _patientId = string.Empty;
        private DateTime? _patientDateOfBirth;
        private bool _showPatientInfo;
        
        public string PatientName
        {
            get => _patientName;
            set
            {
                if (_patientName != value)
                {
                    _patientName = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public string PatientId
        {
            get => _patientId;
            set
            {
                if (_patientId != value)
                {
                    _patientId = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public DateTime? PatientDateOfBirth
        {
            get => _patientDateOfBirth;
            set
            {
                if (_patientDateOfBirth != value)
                {
                    _patientDateOfBirth = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public bool ShowPatientInfo
        {
            get => _showPatientInfo;
            set
            {
                if (_showPatientInfo != value)
                {
                    _showPatientInfo = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public void SetPatientInfo(string patientId, string patientName, DateTime? dateOfBirth)
        {
            PatientId = patientId;
            PatientName = patientName;
            PatientDateOfBirth = dateOfBirth;
            ShowPatientInfo = !string.IsNullOrEmpty(patientId);
        }
        
        public void ClearPatientInfo()
        {
            PatientId = string.Empty;
            PatientName = string.Empty;
            PatientDateOfBirth = null;
            ShowPatientInfo = false;
        }
        
        #region INotifyPropertyChanged
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        #endregion
    }
}
