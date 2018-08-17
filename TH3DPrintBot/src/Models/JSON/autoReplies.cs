using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TH3DPrintBot.src.Models.JSON
{
    public class autoReplies
    {
        [DefaultValue("CHANGEME")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<string> firmware { get; set; }

        [DefaultValue("CHANGEME")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<string> bootLoader { get; set; }
    }
}
