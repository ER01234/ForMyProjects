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
    public class EFUnitOfWork : IUnitOfWork
    {
        private PolyclinicContext db;
        private UserRepository userRepository;
        private RoleRepository roleRepository;
        private DoctorRepository doctorRepository;
        private SpecialityRepository specialityRepository;
        private PatientRepository patientRepository;
        private RecordRepository recordRepository;

        public EFUnitOfWork(string connectionString)
        {
            db = new PolyclinicContext(connectionString);
        }
        public IRepository<User> Users
        {
            get
            {
                if (userRepository == null)
                    userRepository = new UserRepository(db);
                return userRepository;
            }
        }
        public IRepository<Role> Roles
        {
            get
            {
                if (roleRepository == null)
                    roleRepository = new RoleRepository(db);
                return roleRepository;
            }
        }
        public IRepository<Doctor> Doctors
        {
            get
            {
                if (doctorRepository == null)
                    doctorRepository = new DoctorRepository(db);
                return doctorRepository;
            }
        }
        public IRepository<Speciality> Specialities
        {
            get
            {
                if (specialityRepository == null)
                    specialityRepository = new SpecialityRepository(db);
                return specialityRepository;
            }
        }
        public IRepository<Patient> Patients
        {
            get
            {
                if (patientRepository == null)
                    patientRepository = new PatientRepository(db);
                return patientRepository;
            }
        }
        public IRepository<Record> Records
        {
            get
            {
                if (recordRepository == null)
                    recordRepository = new RecordRepository(db);
                return recordRepository;
            }
        }

        public void Save()
        {
            db.SaveChanges();
        }

        private bool disposed = false;

        public virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    db.Dispose();
                }
                this.disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
