using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace museumAIS.Classes
{
    public class rowItemClickEvent: EventArgs
    {
        public string IdEcsp { get; }
        public string Status { get; }

        public rowItemClickEvent(string idEcsp, string status)
        {
            IdEcsp = idEcsp;
            Status = status;
        }
    }
}
