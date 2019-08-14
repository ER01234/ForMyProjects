using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyclinic.DAL.Entities;

namespace Polyclinic.DAL.EF
{
    public class PolyclinicContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Speciality> Specialities { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Record> Records { get; set; }

        static PolyclinicContext()
        {
            Database.SetInitializer(new PolyclinicInitializer());
        }
        public PolyclinicContext(string connectionString) : base(connectionString) { }
    }

    public class PolyclinicInitializer : CreateDatabaseIfNotExists<PolyclinicContext>
    {
        protected override void Seed(PolyclinicContext db)
        {
            db.Roles.Add(new Role { Name = "Заведующий" });
            db.Roles.Add(new Role { Name = "Врач" });
            db.Roles.Add(new Role { Name = "Пациент" });
            db.Users.Add(new User { Login = "Admin", Password = "Admin", RoleId = 1 });
            db.SaveChanges();
        }
    }
}