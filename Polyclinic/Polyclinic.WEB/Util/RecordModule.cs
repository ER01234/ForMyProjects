﻿using Ninject.Modules;
using Polyclinic.BLL.Interfaces;
using Polyclinic.BLL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Polyclinic.WEB.Util
{
    public class RecordModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IRecordService>().To<RecordService>();
        }
    }
}