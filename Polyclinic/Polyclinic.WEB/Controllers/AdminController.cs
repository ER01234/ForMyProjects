using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Polyclinic.BLL.Interfaces;
using Polyclinic.BLL.DTO;
using Polyclinic.BLL.Infrastructure;
using Polyclinic.WEB.Models;
using AutoMapper;

namespace Polyclinic.WEB.Controllers
{
    public class AdminController : Controller
    {
        IUserService userService;
        IRoleService roleService;
        ISpecialityService specialityService;
        IDoctorService doctorService;
        List<string> time;
        public AdminController(IUserService userService, IRoleService roleService, ISpecialityService specialityService, IDoctorService doctorService)
        {
            this.userService = userService;
            this.roleService = roleService;
            this.specialityService = specialityService;
            this.doctorService = doctorService;
            time = new List<string>();
            for (int i = 7; i <= 22; i++)
            {
                if (i == 22)
                {
                    time.Add(i.ToString() + ":00");
                }
                else
                {
                    time.Add(i.ToString() + ":00");
                    time.Add(i.ToString() + ":15");
                    time.Add(i.ToString() + ":30");
                    time.Add(i.ToString() + ":45");
                }
            }
        }
        public ActionResult Administrating()
        {
            Session["Status"] = "Заведующий";
            return View();
        }
        public ActionResult Specialities()
        {
            Session["Status"] = "Заведующий";
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<SpecialityDTO, SpecialityViewModel>()).CreateMapper();
            return View(mapper.Map<IEnumerable<SpecialityDTO>, IEnumerable<SpecialityViewModel>>(specialityService.GetSpecialities()));
        }
        [HttpGet]
        public ActionResult AddSpeciality()
        {
            Session["Status"] = "Заведующий";
            return View();
        }
        [HttpPost]
        public ActionResult AddSpeciality(SpecialityViewModel speciality)
        {
            try
            {
                SpecialityDTO specialityDTO = new SpecialityDTO { Name = speciality.Name };
                specialityService.ValidateSpeciality(specialityDTO);
                specialityService.AddSpeciality(specialityDTO);
                return RedirectToRoute(new { Controller = "Admin", Action = "Specialities" });
            }
            catch(ValidationException ex)
            {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            return View(speciality);
        }
        [HttpGet]
        public ActionResult EditSpeciality(int id)
        {
            Session["Status"] = "Заведующий";
            SpecialityDTO specialityDTO =  specialityService.GetSpeciality(id);
            return View(new SpecialityViewModel { Id=specialityDTO.Id, Name=specialityDTO.Name});
        }
        [HttpPost]
        public ActionResult EditSpeciality(SpecialityViewModel speciality)
        {
            try
            {
                SpecialityDTO specialityDTO = new SpecialityDTO {Id=speciality.Id, Name = speciality.Name };
                specialityService.ValidateSpeciality2(specialityDTO);
                specialityService.UpdateSpeciality(specialityDTO);
                return RedirectToRoute(new { Controller = "Admin", Action = "Specialities" });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(ex.Property, ex.Message);
            }
            return View(speciality);
        }

        public ActionResult DeleteSpeciality(int id)
        {
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<DoctorDTO, DoctorViewModel>()).CreateMapper();
            var doctors = mapper.Map<IEnumerable<DoctorDTO>, IEnumerable<DoctorViewModel>>(doctorService.GetDoctors()).Where(d => d.SpecialityId==id);
            foreach(var doctor in doctors)
            {
                doctorService.DeleteDoctor(doctor.Id);
                userService.DeleteUser(doctor.Login);
            }
            specialityService.DeleteSpeciality(id);
            return RedirectToRoute(new { Controller="Admin", Action="Specialities"});
        }
        public ActionResult Doctors()
        {
            Session["Status"] = "Заведующий";
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<SpecialityDTO, SpecialityViewModel>()).CreateMapper();
            ViewBag.Specialities = mapper.Map<IEnumerable<SpecialityDTO>, IEnumerable<SpecialityViewModel>>(specialityService.GetSpecialities());

            RoleDTO roleDTO = roleService.GetRole("Врач");

            mapper = new MapperConfiguration(cfg => cfg.CreateMap<UserDTO, UserViewModel>()).CreateMapper();
            ViewBag.Users = mapper.Map<IEnumerable<UserDTO>, IEnumerable<UserViewModel>>(userService.GetUsers()).Where(u=>u.RoleId==roleDTO.Id);

            mapper = new MapperConfiguration(cfg => cfg.CreateMap<DoctorDTO, DoctorViewModel>()).CreateMapper();
            return View(mapper.Map<IEnumerable<DoctorDTO>, IEnumerable<DoctorViewModel>>(doctorService.GetDoctors()));
        }
        [HttpGet]
        public ActionResult AddDoctor()
        {
            Session["Status"] = "Заведующий";
            ViewBag.Time = new SelectList(time);
            ViewBag.Time2 = new SelectList(time, "22:00");

            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<SpecialityDTO, SpecialityViewModel>()).CreateMapper();
            IEnumerable<SpecialityViewModel> specialities= mapper.Map<IEnumerable<SpecialityDTO>, IEnumerable<SpecialityViewModel>>(specialityService.GetSpecialities());
            ViewBag.Specialities = new SelectList(specialities, "Id", "Name");

            return View();
        }
        [HttpPost]
        public ActionResult AddDoctor(DoctorViewModel doctor)
        {
            try
            {
                DoctorDTO doctorDTO = new DoctorDTO { Name = doctor.Name, Surname = doctor.Surname, Patronymic = doctor.Patronymic, SpecialityId = doctor.SpecialityId, TheBeginingOfReception = doctor.TheBeginingOfReception, TheEndOfReception = doctor.TheEndOfReception, Cabinet = doctor.Cabinet, Email=doctor.Email, Login = doctor.Login, Password = doctor.Password };
                UserDTO userDTO = new UserDTO { Email=doctor.Email, Login = doctor.Login, Password = doctor.Password };

                doctorService.ValidateDoctor(doctorDTO);
                userService.ValidateUser(userDTO);

                doctorService.AddDoctor(doctorDTO);
                RoleDTO roleDTO = roleService.GetRole("Врач");
                userDTO.RoleId = roleDTO.Id;
                userService.AddUser(userDTO);

                return RedirectToRoute(new { Controller = "Admin", Action = "Doctors" });
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
            ViewBag.Time = new SelectList(time);
            ViewBag.Time2 = new SelectList(time, "22:00");

            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<SpecialityDTO, SpecialityViewModel>()).CreateMapper();
            IEnumerable<SpecialityViewModel> specialities = mapper.Map<IEnumerable<SpecialityDTO>, IEnumerable<SpecialityViewModel>>(specialityService.GetSpecialities());
            ViewBag.Specialities = new SelectList(specialities, "Id", "Name");

            return View(doctor);
        }
        [HttpGet]
        public ActionResult EditDoctor(int id)
        {
            Session["Status"] = "Заведующий";
            DoctorDTO doctorDTO = doctorService.GetDoctor(id);
            UserDTO userDTO = userService.GetUser(doctorDTO.Login);   

            ViewBag.Time = new SelectList(time);
            ViewBag.Time2 = new SelectList(time, "22:00");

            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<SpecialityDTO, SpecialityViewModel>()).CreateMapper();
            IEnumerable<SpecialityViewModel> specialities = mapper.Map<IEnumerable<SpecialityDTO>, IEnumerable<SpecialityViewModel>>(specialityService.GetSpecialities());
            ViewBag.Specialities = new SelectList(specialities, "Id", "Name");

            return View(new DoctorViewModel { Id=doctorDTO.Id, Name = doctorDTO.Name, Surname = doctorDTO.Surname, Patronymic = doctorDTO.Patronymic, SpecialityId = doctorDTO.SpecialityId, TheBeginingOfReception = doctorDTO.TheBeginingOfReception, TheEndOfReception = doctorDTO.TheEndOfReception, Email=userDTO.Email, Cabinet = doctorDTO.Cabinet, Login = doctorDTO.Login });
        }
        [HttpPost]
        public ActionResult EditDoctor(DoctorViewModel doctor)
        {
            try
            {
                DoctorDTO doctorDTO = new DoctorDTO { Id = doctor.Id, Name = doctor.Name, Surname = doctor.Surname, Patronymic = doctor.Patronymic, SpecialityId = doctor.SpecialityId, TheBeginingOfReception = doctor.TheBeginingOfReception, TheEndOfReception = doctor.TheEndOfReception, Email=doctor.Email, Cabinet = doctor.Cabinet, Login = doctor.Login };
                UserDTO userDTO = userService.GetUser(doctor.Login);
                userDTO.Email = doctor.Email;

                doctorService.ValidateDoctor2(doctorDTO);
                doctorService.UpdateDoctor(doctorDTO);
                userService.UpdateUser(userDTO);

                return RedirectToRoute(new { Controller = "Admin", Action = "Doctors" });
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
            ViewBag.Time = new SelectList(time);
            ViewBag.Time2 = new SelectList(time, "22:00");

            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<SpecialityDTO, SpecialityViewModel>()).CreateMapper();
            IEnumerable<SpecialityViewModel> specialities = mapper.Map<IEnumerable<SpecialityDTO>, IEnumerable<SpecialityViewModel>>(specialityService.GetSpecialities());
            ViewBag.Specialities = new SelectList(specialities, "Id", "Name");

            return View(doctor);
        }
        public ActionResult DeleteDoctor(int id)
        {
            DoctorDTO doctorDTO = doctorService.GetDoctor(id);
            userService.DeleteUser(doctorDTO.Login);
            doctorService.DeleteDoctor(id);
            return RedirectToRoute(new { Controller = "Admin", Action = "Doctors" });
        }
    }
}