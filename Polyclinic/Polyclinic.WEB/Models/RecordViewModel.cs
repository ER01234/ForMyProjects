using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Polyclinic.WEB.Models
{
    public class RecordViewModel
    {
        public int Id { get; set; }
        [Display(Name="Дата приёма")]
        public DateTime Date { get; set; }
        public int PatientId { get; set; }
        [Display(Name="ФИО пациента")]
        public string PatientName { get; set; }
        public int DoctorId { get; set; }
        [Display(Name = "ФИО врача")]
        public string DoctorName { get; set; }
        [Display(Name = "Время приёма")]
        public string Time { get; set; }
        [Display(Name = "Кабинет")]
        public string Cabinet { get; set; }
    }
}