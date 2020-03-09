using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.SharedSource.DataImporter.Controllers;

namespace Sitecore.SharedSource.DataImporter
{
    public class IocConfig : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient(typeof(DataImporterController));
        }
    }
}