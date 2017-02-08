using System;

namespace CalendarFunnel.Controllers
{
    public class NewEvent
    {
        public string Calendar { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }
    }
}