using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sitecore.SharedSource.DataImporter.App_Start
{
    public class DataImportAreaRegistration : AreaRegistration
    {
        public override string AreaName => "DataImport";

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(AreaName, "DataImport/{controller}/{action}", new
            {
                area = AreaName
            });
        }
    }
}