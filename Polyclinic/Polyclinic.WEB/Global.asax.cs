using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Polyclinic.BLL.Infrastructure;
using Polyclinic.WEB.Util;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Mvc;

namespace Polyclinic.WEB
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            NinjectModule serviceModule = new ServiceModule("DefaultConnection");
            NinjectModule roleModule = new RoleModule();
            NinjectModule userModule = new UserModule();
            NinjectModule patientModule = new PatientModule();
            NinjectModule doctorModule = new DoctorModule();
            NinjectModule specialityModule = new SpecialityModule();
            NinjectModule recordModule = new RecordModule();

            var kernel = new StandardKernel(serviceModule, roleModule, userModule, patientModule, doctorModule, specialityModule, recordModule);
            DependencyResolver.SetResolver(new NinjectDependencyResolver(kernel));
        }
    }
}
