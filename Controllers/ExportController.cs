using Microsoft.AspNetCore.Mvc;
using PrivateChildcareCalendarApi.Services;
using System.Globalization;
using System.Text;

namespace PrivateChildcareCalendarApi.Controllers;

[ApiController]
[Route("api/export")]
public class ExportController : ControllerBase
{
    private readonly CalendarEventService _calendarEventService;

    public ExportController(CalendarEventService calendarEventService)
        => _calendarEventService = calendarEventService;

    // GET /api/export/year/{year}/excel
    [HttpGet("year/{year:int}/excel")]
    public async Task<IActionResult> YearExcel(int year)
    {
        var start = new DateTime(year, 1, 1);
        var end = start.AddYears(1);

        var events = await _calendarEventService.GetEventsAsync(start, end);
        var culture = CultureInfo.GetCultureInfo("da-DK");

        var sb = new StringBuilder();
        sb.AppendLine("<html><head><meta charset='utf-8'></head><body>");
        sb.AppendLine($"<h1>Årsoversigt {year}</h1>");
        sb.AppendLine("<table border='1'>");
        sb.AppendLine("<tr><th>Dato</th><th>Uge</th><th>Ugedag</th><th>Kalenderlinjer</th></tr>");

        for (var date = start; date < end; date = date.AddDays(1))
        {
            var dayEvents = events
                .Where(x => x.Start.Date == date.Date ||
                            (x.End.HasValue && x.Start.Date <= date.Date && x.End.Value.Date > date.Date))
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Title)
                .Select(x => System.Net.WebUtility.HtmlEncode(x.Title));

            sb.Append("<tr>");
            sb.Append($"<td>{date:dd-MM-yyyy}</td>");
            sb.Append($"<td>{ISOWeek.GetWeekOfYear(date)}</td>");
            sb.Append($"<td>{culture.DateTimeFormat.GetDayName(date.DayOfWeek)}</td>");
            sb.Append($"<td>{string.Join("<br>", dayEvents)}</td>");
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</table></body></html>");

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/html", $"Aarsoversigt_{year}.html");
    }
}