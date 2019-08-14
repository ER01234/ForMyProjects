using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyclinic.DAL.Entities;
using Polyclinic.DAL.Interfaces;
using Polyclinic.DAL.EF;
using System.Data.Entity;

namespace Polyclinic.DAL.Repositories
{
    public class RecordRepository : IRepository<Record>
    {
        private PolyclinicContext db;

        public RecordRepository(PolyclinicContext db)
        {
            this.db = db;
        }

        public IEnumerable<Record> GetAll()
        {
            return db.Records;
        }

        public Record Get(int id)
        {
            return db.Records.Find(id);
        }

        public void Create(Record record)
        {
            db.Records.Add(record);
        }

        public void Update(Record record)
        {
            db.Entry(record).State = EntityState.Modified;
        }

        public IEnumerable<Record> Find(Func<Record, Boolean> predicate)
        {
            return db.Records.Where(predicate).ToList();
        }

        public void Delete(int id)
        {
            Record record = db.Records.Find(id);
            if (record != null)
                db.Records.Remove(record);
        }
    }
}
