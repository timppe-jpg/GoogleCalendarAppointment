using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GoogleCalendarAppointment
{
    class EventHelpper
    {
        Dictionary<string, int> WeekDayInts = new Dictionary<string, int>()
        {
            {"Ma",0 },
            {"Ti",1 },
            {"Ke",2 },
            {"To",3 },
            {"Pe",4 },
        };

        public List<CustomEvent> GetEvents(string rawString, int year, int weekNumber)
        {
            List<CustomEvent> result = new List<CustomEvent>();
            DateTime monday = FirstDateOfWeekISO8601(year, weekNumber);
            List<RowData> rowData = GetRowData(rawString);

            foreach (var item in rowData)
            {
                DateTime tempDateTime = monday;

                //Vienti
                DateTime start = (tempDateTime + new TimeSpan(int.Parse(item.vientiAika.Split(':')[0]), int.Parse(item.vientiAika.Split(':')[1]), 0)).AddDays(WeekDayInts[item.pv]);
                DateTime end = start + new TimeSpan(0, 30, 0);
                string eventName = $"{item.vieja} Vie Arsin";

                result.Add(new CustomEvent(start, end, eventName));

                //Nouto
                start = (tempDateTime + new TimeSpan(int.Parse(item.noutoAika.Split(':')[0]), int.Parse(item.noutoAika.Split(':')[1]), 0)).AddDays(WeekDayInts[item.pv]);
                end = start + new TimeSpan(0, 30, 0);
                eventName = $"{item.noutaja} Hakee Arsin";

                result.Add(new CustomEvent(start, end, eventName));
            }


            return result;
        }

        private List<RowData> GetRowData(string data)
        {
            string[] rows = data.Split(new string[] {"\n"}, StringSplitOptions.None);
            List<RowData> result = new List<RowData>();

            foreach (var row in rows)
            {
                if (string.IsNullOrEmpty(row) || string.IsNullOrWhiteSpace(row)) continue;
                string temp = row.Replace(" ","");

                string pv = string.Empty;
                string vieja = string.Empty;
                string noutaja = string.Empty;
                string vientiAika = string.Empty;
                string noutoAika = string.Empty;
                //Ma(Timi)7:30-14:30(Milka)

                //Ma
                pv = temp.Substring(0, temp.IndexOf("("));
                //Timi
                vieja = temp.Substring(temp.IndexOf("(") + 1, temp.IndexOf(")") - 3);
                temp = temp.Substring(temp.IndexOf(")") + 1);
                //7:30
                vientiAika = temp.Substring(0, temp.IndexOf("-"));
                //14:30
                noutoAika = temp.Substring(temp.IndexOf("-") + 1, temp.Length - temp.IndexOf("(") - 2);
                //Milka
                noutaja = temp.Substring(temp.IndexOf("(") + 1, temp.Length - temp.IndexOf("(") - 2);

                result.Add(new RowData()
                {
                    pv = pv,
                    vieja = vieja,
                    vientiAika = vientiAika,
                    noutoAika = noutoAika,
                    noutaja = noutaja
                });
            }

            return result;
        }

        

        public static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek == 1)
            {
                weekNum -= 1;
            }

            var result = firstThursday.AddDays(weekNum * 7);

            return result.AddDays(-3);
        }
    }
}
