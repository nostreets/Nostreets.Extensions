using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace NostreetsExtensions.Utilities.Managers
{
    public static class ConfigManager
    {
        static Configuration _config = WebConfigurationManager.OpenWebConfiguration(null);


    }
}
