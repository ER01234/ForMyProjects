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
    public class DoctorController : Controller
    {
        IDoctorService doctorService;
        IRecordService recordService;
        public DoctorController(IDoctorService doctorService, IRecordService recordService)
        {
            this.doctorService = doctorService;
            this.recordService = recordService;
        }
        public ActionResult DoctorRecords()
        {
            DoctorDTO doctorDTO = doctorService.GetDoctor(Session["Login"].ToString());
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<RecordDTO, RecordViewModel>()).CreateMapper();
            DateTime Tommorow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1);
            return View(mapper.Map<IEnumerable<RecordDTO>, IEnumerable<RecordViewModel>>(recordService.GetRecordsForDoctor(doctorDTO.Id).Where(r => r.Date.Date == DateTime.Now.Date || r.Date.Date == Tommorow.Date)));
        }
    }
}