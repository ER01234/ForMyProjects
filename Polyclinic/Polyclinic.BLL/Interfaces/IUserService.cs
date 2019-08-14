using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyclinic.BLL.DTO;

namespace Polyclinic.BLL.Interfaces
{
    public interface IUserService
    {
        void ValidateUser(UserDTO userDTO);
        void ValidateUser(string Prop);
        void AddUser(UserDTO userDTO);
        void DeleteUser(string Login);
        void UpdateUser(UserDTO userDTO);
        UserDTO GetUser(UserDTO userDTO);
        UserDTO GetUser(string Login);
        UserDTO GetUserForAccess(string Prop);
        IEnumerable<UserDTO> GetUsers();
        void Dispose();
    }
}
