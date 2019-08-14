using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Polyclinic.WEB.Models
{
    public class RestoreAccessViewModel
    {
        [Display(Name = "Логин/E-mail")]
        public string Prop { get; set; }
    }
}