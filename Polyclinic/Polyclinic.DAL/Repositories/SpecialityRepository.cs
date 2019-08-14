using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyclinic.DAL.Entities;
using Polyclinic.DAL.Interfaces;
using Polyclinic.DAL.EF;
using System.Data.Entity;

namespace Polyclinic.DAL.Repositories
{
    public class SpecialityRepository : IRepository<Speciality>
    {
        private PolyclinicContext db;

        public SpecialityRepository(PolyclinicContext db)
        {
            this.db = db;
        }

        public IEnumerable<Speciality> GetAll()
        {
            return db.Specialities;
        }

        public Speciality Get(int id)
        {
            return db.Specialities.Find(id);
        }

        public void Create(Speciality speciality)
        {
            db.Specialities.Add(speciality);
        }

        public void Update(Speciality speciality)
        {
            var specialityInDB = db.Specialities.Find(speciality.Id);
            db.Entry(specialityInDB).CurrentValues.SetValues(speciality);
            db.Entry(specialityInDB).State = EntityState.Modified;
        }

        public IEnumerable<Speciality> Find(Func<Speciality, Boolean> predicate)
        {
            return db.Specialities.Where(predicate).ToList();
        }

        public void Delete(int id)
        {
            Speciality speciality = db.Specialities.Find(id);
            if (speciality != null)
                db.Specialities.Remove(speciality);
        }
    }
}
