using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyclinic.BLL.DTO;
using Polyclinic.BLL.Interfaces;
using Polyclinic.BLL.Infrastructure;
using Polyclinic.DAL.Entities;
using Polyclinic.DAL.Interfaces;

namespace Polyclinic.BLL.Services
{
    public class PatientService : IPatientService
    {
        IUnitOfWork Database;
        public PatientService(IUnitOfWork uow)
        {
            Database = uow;
        }
        public void ValidatePatient(PatientDTO patientDTO)
        {
            string Messages = "";
            string Properties = "";
            bool Errors = false;
            if (patientDTO.Name == null)
            {
                Messages += "|Введите имя";
                Properties += "|Name";
                Errors = true;
            }
            if (patientDTO.Surname == null)
            {
                Messages += "|Введите фамилию";
                Properties += "|Surname";
                Errors = true;
            }
            if (patientDTO.Patronymic == null)
            {
                Messages += "|Введите отчество";
                Properties += "|Patronymic";
                Errors = true;
            }
            if (patientDTO.Address == null)
            {
                Messages += "|Введите адресс";
                Properties += "|Address";
                Errors = true;
            }
            if (patientDTO.Email == null)
            {
                Messages += "|Введите e-mail";
                Properties += "|Email";
                Errors = true;
            }
            if (patientDTO.Login == null)
            {
                Messages += "|Введите логин";
                Properties += "|Login";
                Errors = true;
            }
            if (patientDTO.Password == null)
            {
                Messages += "|Введите пароль";
                Properties += "|Password";
                Errors = true;
            }
            if (patientDTO.ConfirmPassword == null)
            {
                Messages += "|Подтвердите пароль";
                Properties += "|ConfirmPassword";
                Errors = true;
            }
            if (patientDTO.Password != patientDTO.ConfirmPassword)
            {
                Messages += "|Пароли не совпадают";
                Properties += "|ConfirmPassword";
                Errors = true;
            }
            if (Errors)
            {
                throw new ValidationException(Messages, Properties);
            }
        }
        public void AddPatient(PatientDTO patientDTO)
        {
            Database.Patients.Create(new Patient { Name = patientDTO.Name, Surname = patientDTO.Surname, Patronymic = patientDTO.Patronymic, YearOfBirth = patientDTO.YearOfBirth, Address = patientDTO.Address, Login = patientDTO.Login });
            Database.Save();
        }
        public PatientDTO GetPatient(string Login)
        {
            Patient patient = Database.Patients.GetAll().Where(p => p.Login == Login).FirstOrDefault();
            return new PatientDTO { Id = patient.Id, Name = patient.Name, Surname = patient.Surname, Patronymic = patient.Patronymic, YearOfBirth = patient.YearOfBirth, Address = patient.Address, Login = patient.Login };
        }
        public void Dispose()
        {
            Database.Dispose();
        }
    }
}
