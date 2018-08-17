using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TH3DPrintBot.src.Models.JSON;

namespace TH3DPrintBot.src.Models
{
    public class RootSettings
    {
        public program_settings program_settings { get; set; }
        public general general { get; set; }
        public lists lists { get; set; }
        public autoReplies autoReplies { get; set; }
    }
}
