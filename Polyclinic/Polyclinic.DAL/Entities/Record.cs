using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyclinic.DAL.Entities
{
    public class Record
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int PatientId { get; set; }
        public Patient Patient { get; set; }
        public string PatientName { get; set; }
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }
        public string DoctorName { get; set; }
        public string Time { get; set; }
        public string Cabinet { get; set; }
    }
}
