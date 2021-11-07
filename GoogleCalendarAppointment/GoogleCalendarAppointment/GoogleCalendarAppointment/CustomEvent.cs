using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCalendarAppointment
{
    class CustomEvent
    {
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public string name { get; set; }

        public CustomEvent(DateTime _start, DateTime _end, string _name)
        {
            start = _start;
            end = _end;
            name = _name;
        }



    }
}
