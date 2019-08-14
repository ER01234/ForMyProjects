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
    public  class PatientRepository : IRepository<Patient>
    {
        private PolyclinicContext db;

        public PatientRepository(PolyclinicContext db)
        {
            this.db = db;
        }

        public IEnumerable<Patient> GetAll()
        {
            return db.Patients;
        }

        public Patient Get(int id)
        {
            return db.Patients.Find(id);
        }

        public void Create(Patient patient)
        {
            db.Patients.Add(patient);
        }

        public void Update(Patient patient)
        {
            db.Entry(patient).State = EntityState.Modified;
        }

        public IEnumerable<Patient> Find(Func<Patient, Boolean> predicate)
        {
            return db.Patients.Where(predicate).ToList();
        }

        public void Delete(int id)
        {
            Patient patient = db.Patients.Find(id);
            if (patient != null)
                db.Patients.Remove(patient);
        }
    }
}
