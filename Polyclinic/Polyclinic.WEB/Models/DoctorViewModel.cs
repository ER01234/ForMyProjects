using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Polyclinic.WEB.Models
{
    public class DoctorViewModel
    {
        public int Id { get; set; }
        [Display(Name="Имя")]
        public string Name { get; set; }
        [Display(Name = "Фамилия")]
        public string Surname { get; set; }
        [Display(Name = "Отчество")]
        public string Patronymic { get; set; }
        [Display(Name = "Специальность")]
        public int SpecialityId { get; set; }
        [Display(Name = "Начало приёма")]
        public string TheBeginingOfReception { get; set; }
        [Display(Name = "Конец приёма")]
        public string TheEndOfReception { get; set; }
        [Display(Name = "Номер кабинета")]
        public string Cabinet { get; set; }
        [Display(Name = "E-mail")]
        [DataType(DataType.Text)]
        public string Email { get; set; }
        [Display(Name = "Логин")]
        public string Login { get; set; }
        [Display(Name = "Пароль")]
        public string Password { get; set; }
    }
}