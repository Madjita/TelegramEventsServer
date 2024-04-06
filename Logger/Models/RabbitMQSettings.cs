using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLoggerNamespace.Models
{
    public class RabbitMQSettings
    {
        public bool Start;
        public string UserName;
        public string Password;
        public string HostName;
        public int Port;
        public string Exchange;
        public string RoutingKey;
        public string RoutingKeyTimingAPI;
        public string RoutingKeyTimingProcessings;
    }
}
