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
    public class DoctorRepository : IRepository<Doctor>
    {
        private PolyclinicContext db;

        public DoctorRepository(PolyclinicContext db)
        {
            this.db = db;
        }

        public IEnumerable<Doctor> GetAll()
        {
            return db.Doctors;
        }

        public Doctor Get(int id)
        {
            return db.Doctors.Find(id);
        }

        public void Create(Doctor doctor)
        {
            db.Doctors.Add(doctor);
        }

        public void Update(Doctor doctor)
        {
            var doctorInDB = db.Doctors.Find(doctor.Id);
            db.Entry(doctorInDB).CurrentValues.SetValues(doctor);
            db.Entry(doctorInDB).State = EntityState.Modified;
        }

        public IEnumerable<Doctor> Find(Func<Doctor, Boolean> predicate)
        {
            return db.Doctors.Where(predicate).ToList();
        }

        public void Delete(int id)
        {
            Doctor doctors = db.Doctors.Find(id);
            if (doctors != null)
                db.Doctors.Remove(doctors);
        }
    }
}
