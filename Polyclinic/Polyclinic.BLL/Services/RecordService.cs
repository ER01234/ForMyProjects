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
using AutoMapper;

namespace Polyclinic.BLL.Services
{
    public class RecordService : IRecordService
    {
        IUnitOfWork Database;
        public RecordService(IUnitOfWork uow)
        {
            Database = uow;
        }
        public void AddRecord(RecordDTO recordDTO)
        {
            Database.Records.Create(new Record { Date = recordDTO.Date, DoctorId = recordDTO.DoctorId, DoctorName=recordDTO.DoctorName, PatientId = recordDTO.PatientId, PatientName=recordDTO.PatientName, Time=recordDTO.Time, Cabinet = recordDTO.Cabinet });
            Database.Save();
        }
        public IEnumerable<RecordDTO> GetRecordsForPatient(int id)
        {
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<Record, RecordDTO>()).CreateMapper();
            return mapper.Map<IEnumerable<Record>, IEnumerable<RecordDTO>>(Database.Records.GetAll().Where(r=>r.PatientId==id));
        }
        public IEnumerable<RecordDTO> GetRecordsForDoctor(int id)
        {
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<Record, RecordDTO>()).CreateMapper();
            return mapper.Map<IEnumerable<Record>, IEnumerable<RecordDTO>>(Database.Records.GetAll().Where(r => r.DoctorId == id));
        }
        public void Dispose()
        {
            Database.Dispose();
        }
    }
}
