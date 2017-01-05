using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Mvc;

namespace CalendarFunnel.Controllers
{
    [Route("api/[controller]")]
    public class CalendarController : Controller
    {
        private readonly GoogleCalendarSettings _settings;

        private readonly CalendarService _service;

        public CalendarController()
        {
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
            var tz = TimeZoneInfo.FindSystemTimeZoneById("CET");

            var eventz = _settings.Calendars.AsParallel()
                .SelectMany(GetCalendarEvents)
                .Select(e => new
                {
                    id = e.Id,
                    description = e.Description ?? string.Empty,
                    text = e.Summary,
                    start_date = TimeZoneInfo.ConvertTime(e.Start.DateTime.GetValueOrDefault(), tz)
                        .ToString("yyyy-MM-dd HH:mm"),
                    end_date =
                    TimeZoneInfo.ConvertTime(e.End.DateTime.GetValueOrDefault(), tz).ToString("yyyy-MM-dd HH:mm"),
                    location = e.Location,
                    googleeventid = e.Id
                });

            return Json(eventz);
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