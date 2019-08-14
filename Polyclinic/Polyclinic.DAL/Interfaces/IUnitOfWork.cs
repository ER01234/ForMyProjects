using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyclinic.DAL.Entities;

namespace Polyclinic.DAL.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Role> Roles { get; }
        IRepository<Doctor> Doctors { get; }
        IRepository<Speciality> Specialities { get; }
        IRepository<Patient> Patients { get; }
        IRepository<Record> Records { get; }
        void Save();
    }
}
