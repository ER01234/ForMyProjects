using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyclinic.DAL.Entities
{
    public class Doctor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public int SpecialityId { get; set; }
        public Speciality Speciality { get; set; }
        public string TheBeginingOfReception { get; set; }
        public string TheEndOfReception { get; set; }
        public string Cabinet { get; set; }
        public string Login { get; set; }
    }
}
