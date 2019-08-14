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
using AutoMapper;
using System.Text.RegularExpressions;

namespace Polyclinic.BLL.Services
{
    public class DoctorService : IDoctorService
    {
        IUnitOfWork Database;
        public DoctorService(IUnitOfWork uow)
        {
            Database = uow;
        }
        public void ValidateDoctor(DoctorDTO doctorDTO)
        {
            string Messages = "";
            string Properties = "";
            bool Errors = false;
            if (doctorDTO.Name == null)
            {
                Messages += "|Введите имя";
                Properties += "|Name";
                Errors = true;
            }
            if (doctorDTO.Surname == null)
            {
                Messages += "|Введите фамилию";
                Properties += "|Surname";
                Errors = true;
            }
            if (doctorDTO.Patronymic == null)
            {
                Messages += "|Введите отчество";
                Properties += "|Patronymic";
                Errors = true;
            }
            int cabinet = 0;
            if (doctorDTO.Cabinet == null)
            {
                Messages += "|Укажите кабинет";
                Properties += "|Cabinet";
                Errors = true;
            }
            else if (!Int32.TryParse(doctorDTO.Cabinet, out cabinet) || cabinet < 0)
            {
                Messages += "|Неверный номер кабинета";
                Properties += "|Cabinet";
                Errors = true;
            }
            if (doctorDTO.Email == null)
            {
                Messages += "|Введите e-mail";
                Properties += "|Email";
                Errors = true;
            }
            if (doctorDTO.Login == null)
            {
                Messages += "|Введите логин";
                Properties += "|Login";
                Errors = true;
            }
            if (doctorDTO.Password == null)
            {
                Messages += "|Введите пароль";
                Properties += "|Password";
                Errors = true;
            }
            if (Database.Doctors.GetAll().Where(d => d.Cabinet == doctorDTO.Cabinet).FirstOrDefault() != null)
            {
                Messages += "|Этот кабинет закреплен за другим врачом";
                Properties += "|Cabinet";
                Errors = true;
            }
            if (Errors)
            {
                throw new ValidationException(Messages, Properties);
            }
        }
        public void ValidateDoctor2(DoctorDTO doctorDTO)
        {
            string Messages = "";
            string Properties = "";
            bool Errors = false;
            if (doctorDTO.Name == null)
            {
                Messages += "|Введите имя";
                Properties += "|Name";
                Errors = true;
            }
            if (doctorDTO.Surname == null)
            {
                Messages += "|Введите фамилию";
                Properties += "|Surname";
                Errors = true;
            }
            if (doctorDTO.Patronymic == null)
            {
                Messages += "|Введите отчество";
                Properties += "|Patronymic";
                Errors = true;
            }
            if (doctorDTO.Email == null)
            {
                Messages += "|Введите e-mail";
                Properties += "|Email";
                Errors = true;
            }
            else
            {
                if (!Regex.IsMatch(doctorDTO.Email, @"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}"))
                {
                    Messages += "|Некорректный e-mail адрес";
                    Properties += "|Email";
                    Errors = true;
                }
            }
            if(doctorDTO.Email==Database.Users.GetAll().Where(u=>u.Login==doctorDTO.Login).FirstOrDefault().Email)
            {

            }
            else
            {
                if(Database.Users.GetAll().Where(u=>u.Email==doctorDTO.Email)!=null)
                {
                    Messages += "|Учётная запись с данным e-mail адресом уже зарегистрирована";
                    Properties += "|Email";
                    Errors = true;
                }
            }
            int cabinet = 0;
            if (doctorDTO.Cabinet == null)
            {
                Messages += "|Укажите кабинет";
                Properties += "|Cabinet";
                Errors = true;
            }
            else if (!Int32.TryParse(doctorDTO.Cabinet, out cabinet) || cabinet < 0)
            {
                Messages += "|Неверный номер кабинета";
                Properties += "|Cabinet";
                Errors = true;
            }
            if (doctorDTO.Cabinet == Database.Doctors.GetAll().Where(d => d.Login == doctorDTO.Login).FirstOrDefault().Cabinet)
            {

            }
            else
            {
                if (Database.Doctors.GetAll().Where(d => d.Cabinet == doctorDTO.Cabinet).FirstOrDefault() != null)
                {
                    Messages += "|Этот кабинет закреплен за другим врачом";
                    Properties += "|Cabinet";
                    Errors = true;
                }
            }
            if (Errors)
            {
                throw new ValidationException(Messages, Properties);
            }
        }
        public void AddDoctor(DoctorDTO doctorDTO)
        {
            Database.Doctors.Create(new Doctor { Name = doctorDTO.Name, Surname = doctorDTO.Surname, Patronymic = doctorDTO.Patronymic, SpecialityId = doctorDTO.SpecialityId, TheBeginingOfReception = doctorDTO.TheBeginingOfReception, TheEndOfReception = doctorDTO.TheEndOfReception, Cabinet = doctorDTO.Cabinet, Login = doctorDTO.Login });
            Database.Save();
        }
        public void DeleteDoctor(int id)
        {
            Database.Doctors.Delete(id);
            Database.Save();
        }
        public void UpdateDoctor(DoctorDTO doctorDTO)
        {
            Database.Doctors.Update(new Doctor {Id=doctorDTO.Id, Name = doctorDTO.Name, Surname = doctorDTO.Surname, Patronymic = doctorDTO.Patronymic, SpecialityId = doctorDTO.SpecialityId, TheBeginingOfReception = doctorDTO.TheBeginingOfReception, TheEndOfReception = doctorDTO.TheEndOfReception, Cabinet = doctorDTO.Cabinet, Login = doctorDTO.Login });
            Database.Save();
        }
        public DoctorDTO GetDoctor(int id)
        {
            Doctor doctor = Database.Doctors.Get(id);
            return new DoctorDTO { Id=doctor.Id ,Name = doctor.Name, Surname = doctor.Surname, Patronymic = doctor.Patronymic, SpecialityId = doctor.SpecialityId, TheBeginingOfReception = doctor.TheBeginingOfReception, TheEndOfReception = doctor.TheEndOfReception, Cabinet = doctor.Cabinet, Login = doctor.Login };
        }
        public DoctorDTO GetDoctor(string Login)
        {
            Doctor doctor = Database.Doctors.GetAll().Where(d => d.Login == Login).FirstOrDefault();
            return new DoctorDTO { Id = doctor.Id, Name = doctor.Name, Surname = doctor.Surname, Patronymic = doctor.Patronymic, SpecialityId = doctor.SpecialityId, TheBeginingOfReception = doctor.TheBeginingOfReception, TheEndOfReception = doctor.TheEndOfReception, Cabinet = doctor.Cabinet, Login = doctor.Login };
        }
        public IEnumerable<DoctorDTO> GetDoctors()
        {
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<Doctor, DoctorDTO>()).CreateMapper();
            return mapper.Map<IEnumerable<Doctor>, IEnumerable<DoctorDTO>>(Database.Doctors.GetAll());
        }
        public void Dispose()
        {
            Database.Dispose();
        }
    }
}
