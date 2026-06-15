using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using PrivateChildcareCalendarApi.Services;
using System.Globalization;

namespace PrivateChildcareCalendarApi.Controllers;

[ApiController]
[Route("api/export")]
public class ExportController : ControllerBase
{
    private readonly CalendarEventService _calendarEventService;

    public ExportController(CalendarEventService calendarEventService)
        => _calendarEventService = calendarEventService;

    [HttpGet("year/{year:int}/excel")]
    public async Task<IActionResult> YearExcel(int year)
    {
        if (year < 2000 || year > 2100)
            return BadRequest(new { error = "Ugyldigt år." });

        var start = new DateTime(year, 1, 1);
        var end = start.AddYears(1);
        var events = await _calendarEventService.GetEventsAsync(start, end);
        var culture = CultureInfo.GetCultureInfo("da-DK");

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add($"Årsoversigt {year}");

        // --- Kolonneoverskrifter ---
        ws.Cell(1, 1).Value = "Dato";
        ws.Cell(1, 2).Value = "Uge";
        ws.Cell(1, 3).Value = "Ugedag";
        ws.Cell(1, 4).Value = "Kalenderlinjer";

        var headerRow = ws.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#16a34a");
        headerRow.Style.Font.FontColor = XLColor.White;

        // --- Data ---
        var row = 2;
        for (var date = start; date < end; date = date.AddDays(1))
        {
            var dayEvents = events
                .Where(x => x.Start.Date == date.Date ||
                            (x.End.HasValue &&
                             x.Start.Date <= date.Date &&
                             x.End.Value.Date > date.Date))
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Title)
                .Select(x => x.Title)
                .ToList();

            var isWeekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

            ws.Cell(row, 1).Value = date.ToString("dd-MM-yyyy");
            ws.Cell(row, 2).Value = ISOWeek.GetWeekOfYear(date);
            ws.Cell(row, 3).Value = culture.DateTimeFormat.GetDayName(date.DayOfWeek);
            ws.Cell(row, 4).Value = string.Join(", ", dayEvents);

            // Grå baggrund på weekender
            if (isWeekend)
            {
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#f4f2ee");
                ws.Row(row).Style.Font.FontColor = XLColor.FromHtml("#8f8474");
            }

            row++;
        }

        // --- Kolonnebredder ---
        ws.Column(1).Width = 14;
        ws.Column(2).Width = 6;
        ws.Column(3).Width = 12;
        ws.Column(4).Width = 60;
        ws.Column(4).Style.Alignment.WrapText = true;

        // --- Frys overskriftsrækken ---
        ws.SheetView.FreezeRows(1);

        // --- Auto-filter ---
        ws.RangeUsed()?.SetAutoFilter();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"Aarsoversigt_{year}.xlsx";
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName
        );
    }
}