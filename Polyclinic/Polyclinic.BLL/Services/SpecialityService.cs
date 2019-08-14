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

namespace Polyclinic.BLL.Services
{
    public class SpecialityService : ISpecialityService
    {
        IUnitOfWork Database;
        public SpecialityService(IUnitOfWork uow)
        {
            Database = uow;
        }
        public void ValidateSpeciality(SpecialityDTO specialityDTO)
        {
            if(specialityDTO.Name==null)
            {
                throw new ValidationException("Введите название специальности", "Name");
            }
            if (Database.Specialities.GetAll().Where(s => s.Name == specialityDTO.Name).FirstOrDefault() != null)
            {
                throw new ValidationException("Эта специальность уже есть в базе данных", "Name");
            }
        }
        public void ValidateSpeciality2(SpecialityDTO specialityDTO)
        {
            if (specialityDTO.Name == null)
            {
                throw new ValidationException("Введите название специальности", "Name");
            }
            if (specialityDTO.Name == Database.Specialities.GetAll().Where(s => s.Id == specialityDTO.Id).FirstOrDefault().Name)
            {

            }
            else
            {
                if (Database.Specialities.GetAll().Where(s => s.Name == specialityDTO.Name).FirstOrDefault() != null)
                {
                    throw new ValidationException("Эта специальность уже есть в базе данных", "Name");
                }
            }
        }
        public void AddSpeciality(SpecialityDTO specialityDTO)
        {
            Database.Specialities.Create(new Speciality { Name = specialityDTO.Name });
            Database.Save();
        }
        public void DeleteSpeciality(int id)
        {
            Database.Specialities.Delete(id);
            Database.Save();
        }
        public void UpdateSpeciality(SpecialityDTO specialityDTO)
        {
            Database.Specialities.Update(new Speciality { Id = specialityDTO.Id, Name = specialityDTO.Name });
            Database.Save();
        }
        public SpecialityDTO GetSpeciality(int id)
        {
            Speciality speciality = Database.Specialities.Get(id);
            return new SpecialityDTO { Name = speciality.Name };
        }
        public IEnumerable<SpecialityDTO> GetSpecialities()
        {
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<Speciality, SpecialityDTO>()).CreateMapper();
            return mapper.Map<IEnumerable<Speciality>, IEnumerable<SpecialityDTO>>(Database.Specialities.GetAll());
        }
        public void Dispose()
        {
            Database.Dispose();
        }
    }
}
