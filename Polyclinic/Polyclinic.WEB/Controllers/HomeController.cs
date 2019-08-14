using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Polyclinic.BLL.Interfaces;
using Polyclinic.BLL.DTO;
using Polyclinic.BLL.Infrastructure;
using Polyclinic.WEB.Models;
using System.Net.Mail;
using System.Net;

namespace Polyclinic.WEB.Controllers
{
    public class HomeController : Controller
    {
        IUserService userService;
        IRoleService roleService;
        IDoctorService doctorService;
        IPatientService patientService;
        public HomeController(IUserService userService, IRoleService roleService, IDoctorService doctorService, IPatientService patientService)
        {
            this.userService = userService;
            this.roleService = roleService;
            this.doctorService = doctorService;
            this.patientService = patientService;
        }
        public ActionResult MainPage()
        {
            if (Session["Status"] == null)
            {
                Session["Status"] = "Гость";
            }
            return View();
        }
        [HttpGet]
        public ActionResult LogIn()
        {
            Session["Status"] = "Гость";
            return View();
        }
        [HttpPost]
        public ActionResult LogIn(UserViewModel user)
        {
            try
            {
                UserDTO userDTO = new UserDTO { Login = user.Login, Password = user.Password };
                userDTO = userService.GetUser(userDTO);
                RoleDTO roleDTO = roleService.GetRole(userDTO.RoleId);

                Session["Login"] = userDTO.Login;
                Session["Status"] = roleDTO.Name;
                return Redirect("/");
            }
            catch (ValidationException ex)
            {
                if (ex.Message.Contains("|"))
                {
                    string[] Messages = ex.Message.Split('|');
                    string[] Properties = ex.Property.Split('|');
                    for (int i = 0; i < Messages.Length; i++)
                    {
                        ModelState.AddModelError(Properties[i], Messages[i]);
                    }
                }
                else
                {
                    ModelState.AddModelError(ex.Property, ex.Message);
                }
            }
            return View(user);
        }
        public ActionResult LogOut()
        {
            Session["Login"] = null;
            Session["Status"] = "Гость";
            return Redirect("/");
        }
        [HttpGet]
        public ActionResult RestoreAccess()
        {
            return View();
        }
        [HttpPost]
        public ActionResult RestoreAccess(RestoreAccessViewModel item)
        {
            try
            {
                userService.ValidateUser(item.Prop);
                UserDTO userDTO = userService.GetUserForAccess(item.Prop);
                MailAddress from = new MailAddress("RestoreAccessToPolyclinic@gmail.com", "RestoreAccess");
                // кому отправляем
                MailAddress to = new MailAddress(userDTO.Email);
                // создаем объект сообщения
                MailMessage m = new MailMessage(from, to);
                // тема письма
                m.Subject = "Восстановление доступа к аккаунту";
                // текст письма
                m.Body = "<div>Ваш логин: " + userDTO.Login + "<br/>Ваш пароль: " + userDTO.Password + "</div>";
                // письмо представляет код html
                m.IsBodyHtml = true;
                // адрес smtp-сервера и порт, с которого будем отправлять письмо
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                // логин и пароль
                smtp.Credentials = new NetworkCredential("RestoreAccessToPolyclinic@gmail.com", "RestoreAccess123");
                smtp.EnableSsl = true;
                smtp.Send(m);
                return RedirectToRoute(new { Controller = "Home", Action = "MessageSended" });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            return View(item);
        }
        public ActionResult MessageSended()
        {
            return View();
        }
    }
}