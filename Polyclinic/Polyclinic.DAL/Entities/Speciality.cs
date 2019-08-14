﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyclinic.DAL.Entities
{
    public class Speciality
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Doctor> Doctors { get; set; }
        public Speciality()
        {
            Doctors = new List<Doctor>();
        } 
    }
}
