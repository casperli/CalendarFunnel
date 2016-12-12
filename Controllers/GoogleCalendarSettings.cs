using System;
using System.Collections.Generic;

namespace CalendarFunnel.Controllers
{
    public class GoogleCalendarSettings
    {
        public GoogleCalendarSettings(string calendars)
        {
            Calendars = calendars.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);
        }

        public IEnumerable<string> Calendars { get; set; }

        public string ServiceAccountEmail { get; set; }

        public string PrivateKey { get; set; }

        public string ApplicationName { get; set; }
    }
}