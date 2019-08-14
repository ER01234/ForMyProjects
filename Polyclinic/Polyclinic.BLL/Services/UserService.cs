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
using System.Text.RegularExpressions;
using AutoMapper;

namespace Polyclinic.BLL.Services
{
    public class UserService : IUserService
    {
        IUnitOfWork Database;
        public UserService(IUnitOfWork uow)
        {
            Database = uow;
        }
        public void ValidateUser(UserDTO userDTO)
        {
            string Messages = "";
            string Properties = "";
            bool Errors = false;
            if (!Regex.IsMatch(userDTO.Email, @"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}"))
            {
                Messages += "|Некорректный e-mail адрес";
                Properties += "|Email";
                Errors = true;
            }
            if (Database.Users.GetAll().Where(u => u.Email == userDTO.Email).FirstOrDefault() != null)
            {
                Messages += "|Учётная запись с данным e-mail адресом уже зарегистрирована";
                Properties += "|Email";
                Errors = true;
            }
            if (Database.Users.GetAll().Where(u => u.Login == userDTO.Login).FirstOrDefault()!=null)
            {
                Messages += "|Учётная запись с данным логином уже зарегистрирована";
                Properties += "|Login";
                Errors = true;
            }
            if(Errors)
            {
                throw new ValidationException(Messages, Properties);
            }
        }
        public void ValidateUser(string Prop)
        {
            if(Prop==null)
            {
                throw new ValidationException("Введите логин/e-mail адрес", "Prop");
            }
            User user = Database.Users.GetAll().Where(u => u.Login == Prop).FirstOrDefault();
            if(user==null)
            {
                user = Database.Users.GetAll().Where(u => u.Email == Prop).FirstOrDefault();
                if(user==null)
                {
                    throw new ValidationException("Неверный логин/e-mail адрес", "Prop");
                }
            }
        }
        public void AddUser(UserDTO userDTO)
        {
            Database.Users.Create(new User { Email=userDTO.Email, Login = userDTO.Login, Password = userDTO.Password, RoleId = userDTO.RoleId });
            Database.Save();
        }
        public void DeleteUser(string Login)
        {
            Database.Users.Delete(Database.Users.GetAll().Where(u => u.Login == Login).FirstOrDefault().Id);
            Database.Save();
        }
        public void UpdateUser(UserDTO userDTO)
        {
            Database.Users.Update(new User { Id = userDTO.Id, Email = userDTO.Email, Login = userDTO.Login, Password = userDTO.Password, RoleId = userDTO.RoleId });
            Database.Save();
        }
        public UserDTO GetUser(UserDTO userDTO)
        {
            User user = Database.Users.GetAll().Where(u => u.Login == userDTO.Login).FirstOrDefault();
            string Messages = "";
            string Properties = "";
            bool Errors = false;
            if (userDTO.Login==null)
            {
                Messages += "|Введите логин";
                Properties += "|Login";
                Errors = true;
            }
            if(userDTO.Login!=null)
            {
                if (user == null)
                {
                    Messages += "|Учётная запись с данным логином не зарегистрирована";
                    Properties += "|Login";
                    Errors = true;
                }
            }
            if (userDTO.Password==null)
            {
                Messages += "|Введите пароль";
                Properties += "|Password";
                Errors = true;
            }
            if (user!=null&&userDTO.Password!=null)
            {
                if (user.Password != userDTO.Password)
                {
                    Messages += "|Неверный пароль";
                    Properties += "|Password";
                    Errors = true;
                }
            }
            if(Errors)
            {
                throw new ValidationException(Messages, Properties);
            }
            User User = Database.Users.GetAll().Where(u => u.Login == userDTO.Login).FirstOrDefault();
            return new UserDTO { Id = User.Id, Email = User.Email, Login = User.Login, Password = User.Password, RoleId = User.RoleId };
        }
        public UserDTO GetUser(string Login)
        {
            User user = Database.Users.GetAll().Where(u => u.Login == Login).FirstOrDefault();
            return new UserDTO { Id = user.Id, Email = user.Email, Login = user.Login, Password = user.Password, RoleId = user.RoleId };
        }
        public UserDTO GetUserForAccess(string Prop)
        {
            User user = Database.Users.GetAll().Where(u => u.Login == Prop).FirstOrDefault();
            if (user != null)
            {
                return new UserDTO { Email = user.Email, Login = user.Login, Password = user.Password, RoleId = user.RoleId };
            }
            else
            {
                user = Database.Users.GetAll().Where(u => u.Email == Prop).FirstOrDefault();
                return new UserDTO { Email = user.Email, Login = user.Login, Password = user.Password, RoleId = user.RoleId };
            }
        }
        public IEnumerable<UserDTO> GetUsers()
        {
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<User, UserDTO>()).CreateMapper();
            return mapper.Map<IEnumerable<User>, IEnumerable<UserDTO>>(Database.Users.GetAll());
        }
        public void Dispose()
        {
            Database.Dispose();
        }
    }
}
