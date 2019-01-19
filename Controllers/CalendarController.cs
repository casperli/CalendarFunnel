using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NodaTime;

namespace CalendarFunnel.Controllers
{
    [Route("api/[controller]")]
    public class CalendarController : Controller
    {
        private const string DefaultIANATimeZone = "Europe/Berlin";

        private readonly ILogger<CalendarController> _logger;

        private readonly GoogleCalendarSettings _settings;

        private readonly CalendarService _service;

        public CalendarController(ILogger<CalendarController> logger)
        {
            _logger = logger;
            _settings = new GoogleCalendarSettings(Environment.GetEnvironmentVariable("GoogleCalendarId"))
            {
                ApplicationName = Environment.GetEnvironmentVariable("GoogleApplicationName"),
                PrivateKey = Environment.GetEnvironmentVariable("GooglePrivateKey"),
                ServiceAccountEmail = Environment.GetEnvironmentVariable("GoogleServiceAccountMail"),
            };

            _service = CreateService();
        }

        [HttpGet]
        public JsonResult Get()
        {
            _logger.LogInformation("Getting all events from calendars (in parallel).");

            var eventz = _settings.Calendars.AsParallel()
                .SelectMany(GetCalendarEvents)
                .Select(e => new
                {
                    id = e.Id,
                    description = e.Description ?? string.Empty,
                    text = e.Summary,
                    start_date = GetLocalDateTime(e.Start),
                    end_date = GetLocalDateTime(e.End),

                    location = e.Location,
                    googleeventid = e.Id
                });

            return Json(eventz);
        }

        public string GetLocalDateTime(EventDateTime eventDate)
        {
            const string dateFormat = "yyyy-MM-dd HH:mm";

            try
            {
                var eventUtcDate = eventDate.DateTime.GetValueOrDefault().ToUniversalTime();
                var utcDate = Instant.FromUtc(eventUtcDate.Year, eventUtcDate.Month, eventUtcDate.Day,
                    eventUtcDate.Hour, eventUtcDate.Minute);
                var timeZone = DateTimeZoneProviders.Tzdb[eventDate.TimeZone ?? DefaultIANATimeZone];
                var result = utcDate.InZone(timeZone);

                return result.ToString(dateFormat, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex,
                    $"Could not parse date to Noda Time: {eventDate.Date}. Exception: {ex.Message}");
                return string.Empty;
            }
        }

        [HttpPut]
        [Route("events")]
        public ActionResult UpsertCalendarEvents([FromBody] IEnumerable<NewEvent> eventList)
        {
            // Very primitive permission check
            var validKey = Environment.GetEnvironmentVariable("UpsertKey");

            if (string.IsNullOrEmpty(validKey))
            {
                _logger.LogWarning($"Upsert key received from environment");
                return this.StatusCode((int) HttpStatusCode.ServiceUnavailable);
            }

            var keyHeader = this.Request.Headers["UpsertKey"];

            if (keyHeader == StringValues.Empty || keyHeader.Contains(validKey))
            {
                return this.Unauthorized();
            }

            return this.Ok(Upsert(eventList));
        }

        private async Task<IEnumerable<NewEvent>> Upsert(IEnumerable<NewEvent> eventList)
        {
            var service = CreateService();
            var newEvents = eventList.ToList();

            var newCalendars = newEvents.Select(e => e.Calendar).Distinct().ToList();

            var gcalendars = await service.CalendarList.List().ExecuteAsync();
            var calendarsToUpdate = gcalendars.Items.Where(c => newCalendars.Contains(c.Summary)).ToList();

            foreach (var calendar in calendarsToUpdate)
            {
                Console.WriteLine($"Upserting Calendar: {calendar.Summary} ID: {calendar.Id}");

                var toDelete = await service.Events.List(calendar.Id).ExecuteAsync();

                foreach (var deleteEvent in toDelete.Items.Where(i => i.Start.DateTime?.Year == DateTime.Now.Year))
                {
                    Console.WriteLine("Deleting event " + deleteEvent.Summary);
                    await service.Events.Delete(calendar.Id, deleteEvent.Id).ExecuteAsync();
                }

                var events = newEvents.Where(e => e.Calendar == calendar.Summary);
                foreach (var newEvent in events)
                {
                    var gevent = CreateGoogleEvent(newEvent);

                    Console.WriteLine($"Adding event {gevent.Summary} to calendar {calendar.Summary}");
                    await service.Events.Insert(gevent, calendar.Id).ExecuteAsync();
                }
            }

            return eventList;
        }

        private static Event CreateGoogleEvent(NewEvent newEvent)
        {
            Event gevent = new Event
            {
                Summary = newEvent.Title,
                Description = newEvent.Description,
                Location = newEvent.Location,
            };

            if (newEvent.Start.Hour == 0 && newEvent.End.Hour == 0)
            {
                gevent.End = gevent.Start = new EventDateTime
                    {TimeZone = DefaultIANATimeZone, Date = newEvent.Start.ToString("yyyy-MM-dd")};
            }
            else
            {
                gevent.Start = new EventDateTime {DateTime = newEvent.Start, TimeZone = DefaultIANATimeZone};
                gevent.End = new EventDateTime {DateTime = newEvent.End, TimeZone = DefaultIANATimeZone};
            }


            return gevent;
        }

        private CalendarService CreateService()
        {
            var credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(_settings.ServiceAccountEmail)
                {
                    Scopes = new[] {CalendarService.Scope.Calendar}
                }.FromPrivateKey(_settings.PrivateKey));

            // Create the service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _settings.ApplicationName,
            });

            return service;
        }

        private IEnumerable<Event> GetCalendarEvents(string calendarId)
        {
            var calendar = _service.Calendars.Get(calendarId).Execute();

            EventsResource.ListRequest request =
                _service.Events.List(calendarId);
            request.TimeMin = new DateTime(DateTime.Now.Year - 1, 1, 1);
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 100;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();

            return events.Items.Select(e =>
            {
                e.Summary = calendar.Summary + "#*" + e.Summary;
                return e;
            });
        }
    }
}