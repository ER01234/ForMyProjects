using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Polyclinic.WEB.Models
{
    public class PatientViewModel
    {
        [Display(Name="Имя")]
        public string Name { get; set; }
        [Display(Name = "Фамилия")]
        public string Surname { get; set; }
        [Display(Name = "Отчество")]
        public string Patronymic { get; set; }
        [Display(Name = "Год рождения")]
        public int YearOfBirth { get; set; }
        [Display(Name = "Адрес")]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; }
        [Display(Name = "E-mail")]
        [DataType(DataType.Text)]
        public string Email { get; set; }
        [Display(Name = "Логин")]
        public string Login { get; set; }
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение")]
        public string ConfirmPassword { get; set; }
    }
}