using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TH3DPrintBot.src.Models.JSON
{
    public class program_settings
    {
        [DefaultValue("CHANGEME")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string botToken { get; set; }

        [DefaultValue("Logs")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string logChannel { get; set; }

        [DefaultValue("~")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string commandPrefix { get; set; }

        [DefaultValue("CHANGEME#1111")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string alertUser { get; set; }

        [DefaultValue(10)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int timerStartDelay { get; set; }

        [DefaultValue(60)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int timerUpdateInterval { get; set; }
    }
}
