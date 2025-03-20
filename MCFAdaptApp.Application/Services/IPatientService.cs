using MCFAdaptApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCFAdaptApp.Application.Services
{
    public interface IPatientService
    {
        Task<IEnumerable<Patient>> GetAllPatientsAsync();
        Task<Patient?> GetPatientByIdAsync(string patientId);
    }
}
