# CalendarFunnel
Small ASP.NET App to combine multiple Google Calendars into one stream and show them inside a client component.

## Initialising the App

- Set the environment variables for:
    -           GoogleApplicationName:      The Name of this application, acting as a client to the Google calendar API
                GooglePrivateKey:           The private key for getting access to the Google Account
                GoogleCalendarId:           A list of calendars to be merged, separated by a ";"
                GoogleServiceAccountMail:   The Email address of the client credentials to access the Google Calendar


// cat events.json | curl -X PUT -H "Content-Type: application/json" -d @- http://localhost:5000/api/calendar/events

