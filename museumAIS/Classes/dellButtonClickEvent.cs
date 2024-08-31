using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace museumAIS.Classes
{
    public class dellButtonClickEvent : EventArgs
    {
        public string Id { get; private set; }

        public dellButtonClickEvent(string id)
        {
            this.Id = id;
        }
    }
}
