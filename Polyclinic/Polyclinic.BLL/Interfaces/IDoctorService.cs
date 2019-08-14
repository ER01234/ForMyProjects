using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyclinic.BLL.DTO;

namespace Polyclinic.BLL.Interfaces
{
    public interface IDoctorService
    {
        void ValidateDoctor(DoctorDTO doctorDTO);
        void ValidateDoctor2(DoctorDTO doctorDTO);
        void AddDoctor(DoctorDTO doctorDTO);
        void DeleteDoctor(int id);
        void UpdateDoctor(DoctorDTO doctorDTO);
        DoctorDTO GetDoctor(int id);
        DoctorDTO GetDoctor(string login);
        IEnumerable<DoctorDTO> GetDoctors();
        void Dispose();
    }
}
