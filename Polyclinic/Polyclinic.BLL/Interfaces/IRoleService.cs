using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyclinic.BLL.DTO;

namespace Polyclinic.BLL.Interfaces
{
    public interface IRoleService
    {
        RoleDTO GetRole(string Name);
        RoleDTO GetRole(int Id);
        void Dispose();
    }
}
