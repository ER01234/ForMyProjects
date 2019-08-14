using Polyclinic.DAL.Interfaces;
using Polyclinic.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyclinic.BLL.DTO;
using Polyclinic.BLL.Interfaces;

namespace Polyclinic.BLL.Services
{
    public class RoleService : IRoleService
    {
        IUnitOfWork Database { get; set; }
        public RoleService(IUnitOfWork uow)
        {
            Database = uow;
        }
        public RoleDTO GetRole(string Name)
        {
            Role role = Database.Roles.GetAll().Where(r => r.Name == Name).FirstOrDefault();
            return new RoleDTO { Id = role.Id };
        }
        public RoleDTO GetRole(int Id)
        {
            Role role = Database.Roles.Get(Id);
            return new RoleDTO { Id = role.Id, Name = role.Name };
        }
        public void Dispose()
        {
            Database.Dispose();
        }
    }
}
