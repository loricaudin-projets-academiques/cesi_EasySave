using EasyLog.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLog.Server
{
    internal class ServerConfigurations
    {
        private static ServerConfigurations _Instance = null;
        public int Port { get; } = 5000;
        public string LogLocation { get; } = "/app/logs";
        private ServerConfigurations() {

            DotEnv env = new DotEnv();

            Port = int.Parse(env.GetValue("PORT"));
            LogLocation = env.GetValue("LOG_LOCATION");
        }

        public static ServerConfigurations GetInstance()
        {
            if (_Instance == null)
            {
                _Instance = new ServerConfigurations();
            }
            return _Instance;
        }
    }
}
