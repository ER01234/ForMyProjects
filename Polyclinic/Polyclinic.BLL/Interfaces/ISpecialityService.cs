using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyclinic.BLL.DTO;

namespace Polyclinic.BLL.Interfaces
{
    public interface ISpecialityService
    {
        void ValidateSpeciality(SpecialityDTO specialityDTO);
        void ValidateSpeciality2(SpecialityDTO specialityDTO);
        void AddSpeciality(SpecialityDTO specialityDTO);
        void DeleteSpeciality(int id);
        void UpdateSpeciality(SpecialityDTO specialityDTO);
        SpecialityDTO GetSpeciality(int id);
        IEnumerable<SpecialityDTO> GetSpecialities();
        void Dispose();
    }
}
