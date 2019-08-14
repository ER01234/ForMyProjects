using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Polyclinic.WEB.Models
{
    public class SpecialityViewModel
    {
        public int Id { get; set; }
        [Display(Name="Название")]
        public string Name { get; set; }
    }
}