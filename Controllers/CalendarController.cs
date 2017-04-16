using System;
using System.Collections.Generic;
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

namespace CalendarFunnel.Controllers
{
    [Route("api/[controller]")]
    public class CalendarController : Controller
    {
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
            //            TimeZoneInfo tz = TimeZoneInfo.Local;
            //            try
            //            {
            //                tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Zurich") ?? TimeZoneInfo.Local;
            //            }
            //            catch (Exception ex)
            //            {
            //                // Do nothing
            //            }

            _logger.LogInformation("Getting all events from calendars (in parallel).");

            var eventz = _settings.Calendars.AsParallel()
                .SelectMany(GetCalendarEvents)
                .Select(e => new
                {
                    id = e.Id,
                    description = e.Description ?? string.Empty,
                    text = e.Summary,
                    //                    start_date = TimeZoneInfo.ConvertTime(e.Start.DateTime.GetValueOrDefault(), tz)
                    //                        .ToString("yyyy-MM-dd HH:mm"),
                    //                    end_date =
                    //                    TimeZoneInfo.ConvertTime(e.End.DateTime.GetValueOrDefault(), tz).ToString("yyyy-MM-dd HH:mm"),
                    start_date = e.Start.DateTime.GetValueOrDefault().AddHours(0).ToString("yyyy-MM-dd HH:mm"),
                    end_date = e.End.DateTime.GetValueOrDefault().AddHours(0).ToString("yyyy-MM-dd HH:mm"),

                    location = e.Location,
                    googleeventid = e.Id
                });

            return Json(eventz);
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
                return this.StatusCode((int)HttpStatusCode.ServiceUnavailable);
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

                foreach (var deleteEvent in toDelete.Items)
                {
                    Console.WriteLine("Deleting event " + deleteEvent.Summary);
                    await service.Events.Delete(calendar.Id, deleteEvent.Id).ExecuteAsync();
                }

                var events = newEvents.Where(e => e.Calendar == calendar.Summary);
                foreach (var newEvent in events)
                {
                    Event gevent = new Event
                    {
                        Summary = newEvent.Title,
                        Start = new EventDateTime { DateTime = newEvent.Start, TimeZone = "Europe/Zurich" },
                        End = new EventDateTime { DateTime = newEvent.End, TimeZone = "Europe/Zurich" },
                        Location = newEvent.Location,
                    };

                    Console.WriteLine($"Adding event {gevent.Summary} to calendar {calendar.Summary}");
                    await service.Events.Insert(gevent, calendar.Id).ExecuteAsync();
                }
            }

            return eventList;
        }

        private CalendarService CreateService()
        {
            var credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(_settings.ServiceAccountEmail)
                {
                    Scopes = new[] { CalendarService.Scope.Calendar }
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
            // Calendar as array from environment; Parallelize query
            var calendar = _service.Calendars.Get(calendarId).Execute();

            EventsResource.ListRequest request =
                _service.Events.List(calendarId);
            request.TimeMin = DateTime.Now.AddDays(-20);

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