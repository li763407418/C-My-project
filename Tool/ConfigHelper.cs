using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Tool
{
    class ConfigHelper
    {
        #region config配置文件
        public class config
        {
            string message = ConfigurationManager.AppSettings["DBPath"];
            string a = ConfigurationManager.ConnectionStrings["AAA"].ToString();

        }

        /***
         <?xml version="1.0" encoding="utf-8" ?>
        <configuration>

         <appSettings>
          <add key="DBPath" value="data\#GeesunWindingMachinedb.mdb"/>
         </appSettings>

         <connectionStrings>
          <add name="AAA" connectionString="2fnsfbgf23" />
         </connectionStrings>

        </configuration>


         * **/
        #endregion
    }
}
