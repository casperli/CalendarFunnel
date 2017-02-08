# CalendarFunnel
Small ASP.NET App to combine multiple Google Calendars into one stream and show them inside a client component.

# cURL command to send JSON File with data to import into a calender

cat events.json | curl -X PUT -H "Content-Type: application/json" -d @- http://localhost:5000/api/calendar/events