using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLoggerNamespace.Models
{
    internal class LogLine
    {
        [JsonProperty("machinename")]
        public string MachineName { get; set; }

        [JsonProperty("service")]
        public string Service { get; set; }

        [JsonProperty("loggername")]
        public string LoggerName { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("trace")]
        public int Trace { get; set; }

        [JsonProperty("datetime")]
        public string TimeStampISO8601 { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("originalorderid")]
        public string OriginalOrderId { get; set; }

    }
}
