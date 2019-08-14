using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyclinic.BLL.DTO;

namespace Polyclinic.BLL.Interfaces
{
    public interface IPatientService
    {
        void ValidatePatient(PatientDTO patientDTO);
        void AddPatient(PatientDTO patientDTO);
        PatientDTO GetPatient(string Login);
        void Dispose();
    }
}
