using Microsoft.AspNetCore.Mvc;
using PrivateChildcareCalendarApi.Services;

namespace PrivateChildcareCalendarApi.Controllers;

[ApiController]
[Route("api/calendar")]
public class CalendarController : ControllerBase
{
    private readonly CalendarEventService _calendarEventService;

    public CalendarController(CalendarEventService calendarEventService)
        => _calendarEventService = calendarEventService;

    [HttpGet("events")]
    public async Task<IActionResult> GetEvents(DateTime? start, DateTime? end)
    {
        var rangeStart = (start ?? DateTime.Today.AddMonths(-1)).Date;
        var rangeEnd = (end ?? DateTime.Today.AddMonths(2)).Date.AddDays(1);

        // Begræns til maks 2 år ad gangen
        if ((rangeEnd - rangeStart).TotalDays > 730)
            return BadRequest(new { error = "Datointerval må ikke overstige 2 år." });

        var events = await _calendarEventService.GetEventsAsync(rangeStart, rangeEnd);

        return Ok(events.Select(e => new
        {
            title = e.Title,
            start = e.Start.ToString("yyyy-MM-dd"),
            end = e.End?.ToString("yyyy-MM-dd"),
            className = e.ClassName,
            display = string.IsNullOrWhiteSpace(e.Display) ? null : e.Display,
            allDay = e.AllDay,
            sortOrder = e.SortOrder,
            extendedProps = new { note = e.Note }
        }));
    }
}