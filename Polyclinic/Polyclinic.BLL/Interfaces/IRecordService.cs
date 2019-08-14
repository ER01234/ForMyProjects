using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyclinic.BLL.DTO;

namespace Polyclinic.BLL.Interfaces
{
    public interface IRecordService
    {
        void AddRecord(RecordDTO recordDTO);
        IEnumerable<RecordDTO> GetRecordsForPatient(int id);
        IEnumerable<RecordDTO> GetRecordsForDoctor(int id);
        void Dispose();
    }
}
