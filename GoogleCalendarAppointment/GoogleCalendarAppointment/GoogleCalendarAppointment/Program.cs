using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GoogleCalendarAppointment
{
    class Program
    {
        //static readonly string keyfilepath = @"C:\Users\Timi\source\repos\GoogleCalendarAppointment\GoogleCalendarAppointment\GoogleCalendarAppointment\GoogleCalendarAppointment\calendar-secret.json";
        
        static readonly string keyfilepath = Path.Combine(Environment.CurrentDirectory, "calendar-secret.json");

        static void Main(string[] args)
        {

            /*
             TODO's:
            - Refactor Classes
            - Remove Full filepaths
            - Cleaner data parsing
            - Null checks, mostly for data parsing
             */

            CalendarService service = GetService();
            EventHelpper eventHelpper = new EventHelpper();
            RawData rawData = GetRawData();
            var customEventList = eventHelpper.GetEvents(rawData.Data, rawData.Year, rawData.VkoNro);

            //GetEvents(service);

            foreach (var item in customEventList)
            {
                CreateEvent(service, item);
            }
            

            Console.ReadLine();
        }

        static RawData GetRawData()
        {
            string xmlString = System.IO.File.ReadAllText(@"C:\Users\Timi\source\repos\GoogleCalendarAppointment\GoogleCalendarAppointment\GoogleCalendarAppointment\GoogleCalendarAppointment\Data.xml");
            if (xmlString == null) return default;
            var serializer = new XmlSerializer(typeof(RawData));
            using (var reader = new StringReader(xmlString))
            {
                return (RawData)serializer.Deserialize(reader);
            }

        }

        private static CalendarService GetService()
        {
            CalendarService service;
            try
            {
                string[] Scopes = {
   CalendarService.Scope.Calendar,
   CalendarService.Scope.CalendarEvents,
   CalendarService.Scope.CalendarEventsReadonly
  };

                GoogleCredential credential;
                using (var stream = new FileStream(keyfilepath, FileMode.Open, FileAccess.Read))
                {
                    // As we are using admin SDK, we need to still impersonate user who has admin access    
                    //  https://developers.google.com/admin-sdk/directory/v1/guides/delegation    
                    credential = GoogleCredential.FromStream(stream)
                     .CreateScoped(Scopes).CreateWithUser("calendar-bot@ph-calendar-331310.iam.gserviceaccount.com");
                }

                // Create Calendar API service.    
                service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Calendar Sample",
                });

            }
            catch (Exception ex)
            {
                throw;
            }

            return service;
        }

        static void GetEvents(CalendarService _service)
        {
            // Define parameters of request.    
            EventsResource.ListRequest request = _service.Events.List("gist70prhv28s15o80lcsp6mr8@group.calendar.google.com");
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            string eventsValue = "";
            // List events.    
            Events events = request.Execute();
            eventsValue = "Upcoming events:\n";
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();
                    if (String.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }
                    eventsValue += string.Format("{0} ({1})", eventItem.Summary, when) + "\n";
                }
                Console.WriteLine(eventsValue);
            }
            else
            {
                Console.WriteLine("No upcoming events found.");
            }
        }

        static void CreateEvent(CalendarService _service, CustomEvent customEvent)
        {
            Google.Apis.Calendar.v3.Data.Event body = new Google.Apis.Calendar.v3.Data.Event();
            EventDateTime start = new EventDateTime();
            start.DateTime = customEvent.start;
            EventDateTime end = new EventDateTime();
            end.DateTime = customEvent.end;
            body.Start = start;
            body.End = end;
            body.Summary = customEvent.name;

            Console.WriteLine($"Creating Event: {customEvent.name} start: {customEvent.start.ToString("dd.MM.yyyy HH:mm")} end: {customEvent.end.ToString("dd.MM.yyyy HH:mm")}");

            EventsResource.InsertRequest request = new EventsResource.InsertRequest(_service, body, "gist70prhv28s15o80lcsp6mr8@group.calendar.google.com");
            Google.Apis.Calendar.v3.Data.Event response = request.Execute();
        }
    }
}
