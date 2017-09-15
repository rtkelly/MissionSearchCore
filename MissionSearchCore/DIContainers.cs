using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch
{
    internal static class DIContainers
    {
        public static WindsorContainer GetWindsorContainer()
        {
            var container = new WindsorContainer();

            var containerConfigFile = "MissionSearch.Config";
            container.Install(Castle.Windsor.Installer.Configuration.FromXmlFile(containerConfigFile));

            return container;
        }
    }
}
