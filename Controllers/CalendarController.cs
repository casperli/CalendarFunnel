using System;
using System.Collections.Generic;
using System.Linq;
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

        public CalendarController()
        {
            _settings = new GoogleCalendarSettings
            {
                ApplicationName = Environment.GetEnvironmentVariable("GoogleApplicationName"),
                CalendarId = Environment.GetEnvironmentVariable("GoogleCalendarId"),
                PrivateKey = Environment.GetEnvironmentVariable("GooglePrivateKey"),
                ServiceAccountEmail = Environment.GetEnvironmentVariable("GoogleServiceAccountMail"),
            };
        }

        [HttpGet]
        public JsonResult Get()
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

            // Calendar as array from environment; Parallelize query
            var calendar = service.Calendars.Get(_settings.CalendarId).Execute();
            var test = calendar.Description;

            EventsResource.ListRequest request =
                service.Events.List(_settings.CalendarId);
            request.TimeMin = DateTime.Now.AddDays(-20);

            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 100;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();

            var list = events.Items.Select(e => new
            {
                id = e.Id,
                description = e.Description ?? string.Empty,
                text = e.Summary,
                start_date = ((DateTime) e.Start.DateTime).ToString("yyyy-MM-dd HH:mm"),
                end_date = ((DateTime) e.End.DateTime).ToString("yyyy-MM-dd HH:mm"),
                googleeventid = e.Id
            });

            return Json(list);
        }
    }
}