using AutoMapper;
using Polyclinic.BLL.DTO;
using Polyclinic.BLL.Infrastructure;
using Polyclinic.BLL.Interfaces;
using Polyclinic.WEB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Polyclinic.WEB.Controllers
{
    public class PatientController : Controller
    {
        IUserService userService;
        IRoleService roleService;
        IPatientService patientService;
        ISpecialityService specialityService;
        IDoctorService doctorService;
        IRecordService recordService;
        public PatientController(IUserService userService, IRoleService roleService, IPatientService patientService, ISpecialityService specialityService, IDoctorService doctorService, IRecordService recordService)
        {
            this.userService = userService;
            this.roleService = roleService;
            this.patientService = patientService;
            this.specialityService = specialityService;
            this.doctorService = doctorService;
            this.recordService = recordService;
        }
        [HttpGet]
        public ActionResult Registration()
        {
            Session["Status"] = "Гость";
            List<int> years = new List<int>();
            for (int i = DateTime.Now.Year; i >= 1900; i--)
            {
                years.Add(i);
            }
            ViewBag.Years = new SelectList(years);
            return View();
        }
        [HttpPost]
        public ActionResult Registration(PatientViewModel patient)
        {
            try
            {
                PatientDTO patientDTO = new PatientDTO { Name = patient.Name, Surname = patient.Surname, Patronymic = patient.Patronymic, YearOfBirth = patient.YearOfBirth, Address = patient.Address, Email=patient.Email, Login = patient.Login, Password = patient.Password, ConfirmPassword = patient.ConfirmPassword };
                UserDTO userDTO = new UserDTO {Email=patient.Email, Login = patient.Login, Password = patient.Password };

                patientService.ValidatePatient(patientDTO);
                userService.ValidateUser(userDTO);

                patientService.AddPatient(patientDTO);
                RoleDTO roleDTO = roleService.GetRole("Пациент");
                userDTO.RoleId = roleDTO.Id;
                userService.AddUser(userDTO);

                Session["Login"] = userDTO.Login;
                Session["Status"] = "Пациент";
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
            List<int> years = new List<int>();
            for (int i = DateTime.Now.Year; i >= 1900; i--)
            {
                years.Add(i);
            }
            ViewBag.Years = new SelectList(years);
            return View(patient);
        }
        public ActionResult PatientRecords()
        {
            PatientDTO patientDTO = patientService.GetPatient(Session["Login"].ToString());
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<RecordDTO, RecordViewModel>()).CreateMapper();
            DateTime Tommorow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1);
            return View(mapper.Map<IEnumerable<RecordDTO>, IEnumerable<RecordViewModel>>(recordService.GetRecordsForPatient(patientDTO.Id).Where(r=>r.Date.Date==DateTime.Now.Date||r.Date.Date==Tommorow.Date)));
        }
        public ActionResult ChooseSpeciality()
        {
            Session["Status"] = "Пациент";
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<DoctorDTO, DoctorViewModel>()).CreateMapper();
            ViewBag.Doctors = mapper.Map<IEnumerable<DoctorDTO>, IEnumerable<DoctorViewModel>>(doctorService.GetDoctors());
            mapper = new MapperConfiguration(cfg => cfg.CreateMap<SpecialityDTO, SpecialityViewModel>()).CreateMapper();
            return View(mapper.Map<IEnumerable<SpecialityDTO>, IEnumerable<SpecialityViewModel>>(specialityService.GetSpecialities()));
        }
        public ActionResult ChooseDoctor(int id)
        {
            Session["Status"] = "Пациент";
            ViewBag.Speciality = specialityService.GetSpeciality(id).Name;
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<DoctorDTO, DoctorViewModel>()).CreateMapper();
            return View(mapper.Map<IEnumerable<DoctorDTO>, IEnumerable<DoctorViewModel>>(doctorService.GetDoctors().Where(d=>d.SpecialityId==id)));
        }
        public ActionResult ChooseTime(int id)
        {
            Session["DoctorId"] = id;
            DoctorDTO doctorDTO = doctorService.GetDoctor(id);
            ViewBag.Doctor = new DoctorViewModel { Id = doctorDTO.Id, Name = doctorDTO.Name, Surname = doctorDTO.Surname, Patronymic = doctorDTO.Patronymic, SpecialityId = doctorDTO.SpecialityId, TheBeginingOfReception = doctorDTO.TheBeginingOfReception, TheEndOfReception = doctorDTO.TheEndOfReception, Cabinet = doctorDTO.Cabinet, Login = doctorDTO.Login };
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<RecordDTO, RecordViewModel>()).CreateMapper();
            DateTime Tommorow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1);
            return View(mapper.Map<IEnumerable<RecordDTO>, IEnumerable<RecordViewModel>>(recordService.GetRecordsForDoctor(id).Where(r=>r.Date==Tommorow.Date)));
        }
        [HttpGet]
        public ActionResult AcknowledgeRecord(string time)
        {
            DoctorDTO doctorDTO = doctorService.GetDoctor(Convert.ToInt32(Session["DoctorId"].ToString()));
            PatientDTO patientDTO = patientService.GetPatient(Session["Login"].ToString());
            DateTime Tommorow = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day+1);
            RecordViewModel recordDTO = new RecordViewModel { Date = Tommorow.Date, PatientId = patientDTO.Id, PatientName = patientDTO.Surname + " " + patientDTO.Name + " " + patientDTO.Patronymic, DoctorId = doctorDTO.Id, DoctorName = doctorDTO.Surname + " " + doctorDTO.Name + " " + doctorDTO.Patronymic, Time = time, Cabinet = doctorDTO.Cabinet };
            return View(recordDTO);
        }
        [HttpPost]
        public ActionResult AcknowledgeRecord(RecordViewModel record)
        {
            Session["DoctorId"] = null;
            DateTime Tommorow = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day + 1);
            recordService.AddRecord(new RecordDTO { Date = Tommorow, DoctorId = record.DoctorId, DoctorName = record.DoctorName, PatientId = record.PatientId, PatientName = record.PatientName, Time = record.Time, Cabinet = record.Cabinet });
            return RedirectToRoute(new { Controller = "Home", Action = "MainPage" });
        }
    }
}