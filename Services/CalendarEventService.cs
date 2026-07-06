using Microsoft.EntityFrameworkCore;
using PrivateChildcareCalendarApi.Data;
using PrivateChildcareCalendarApi.Models;

namespace PrivateChildcareCalendarApi.Services;

public class CalendarEventService
{
    private readonly AppDbContext _db;

    public CalendarEventService(AppDbContext db) => _db = db;

    public async Task<List<CalendarEventDto>> GetEventsAsync(DateTime rangeStart, DateTime rangeEnd)
    {
        rangeStart = rangeStart.Date;
        rangeEnd = rangeEnd.Date;

        var data = await LoadDataAsync(rangeStart, rangeEnd);
        var events = new List<CalendarEventDto>();
        var holidayCache = new Dictionary<int, Dictionary<DateTime, string>>();

        events.AddRange(BuildHolidayEvents(rangeStart, rangeEnd));
        events.AddRange(BuildClosureEvents(data.Closures, rangeStart, rangeEnd));
        events.AddRange(BuildChildEvents(data.Children, data.Statuses, data.Closures, rangeStart, rangeEnd, holidayCache));
        events.AddRange(BuildNoteEvents(data.Notes));

        return events
            .OrderBy(x => x.Start)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToList();
    }

    private async Task<CalendarData> LoadDataAsync(DateTime rangeStart, DateTime rangeEnd)
    {
        return new CalendarData
        {
            Children = await _db.Children.AsNoTracking().ToListAsync(),
            Statuses = await _db.ChildDayStatuses
                            .Include(x => x.Child)
                            .AsNoTracking()
                            .Where(x => x.EndDate >= rangeStart && x.Date < rangeEnd)
                            .ToListAsync(),
            Notes = await _db.CalendarNotes.AsNoTracking()
                            .Where(x => x.Date >= rangeStart && x.Date < rangeEnd)
                            .ToListAsync(),
            Closures = await _db.ClosurePeriods.AsNoTracking()
                            .Where(x => x.EndDate >= rangeStart && x.StartDate < rangeEnd)
                            .ToListAsync(),
        };
    }

    private static IEnumerable<CalendarEventDto> BuildHolidayEvents(DateTime rangeStart, DateTime rangeEnd)
    {
        for (var year = rangeStart.Year; year <= rangeEnd.Year; year++)
        {
            foreach (var holiday in DanishHolidayService.GetHolidays(year))
            {
                if (holiday.Key < rangeStart || holiday.Key >= rangeEnd) continue;

                yield return new CalendarEventDto
                {
                    Title = holiday.Value,
                    Start = holiday.Key,
                    ClassName = "event-holiday",
                    Display = "block",
                    SortOrder = SortOrders.Holiday
                };
            }
        }
    }

    private static IEnumerable<CalendarEventDto> BuildClosureEvents(
        List<ClosurePeriod> closures, DateTime rangeStart, DateTime rangeEnd)
    {
        foreach (var closure in closures)
        {
            var from = closure.StartDate.Date < rangeStart ? rangeStart : closure.StartDate.Date;
            var to = closure.EndDate.Date >= rangeEnd ? rangeEnd.AddDays(-1) : closure.EndDate.Date;

            var className = closure.PeriodType == ClosurePeriodType.Ferie
                ? "event-closure-vacation"
                : "event-closure";

            for (var date = from; date <= to; date = date.AddDays(1))
            {
                yield return new CalendarEventDto
                {
                    Id = closure.Id,
                    Title = closure.Title,
                    Start = date,
                    ClassName = className,
                    Display = "block",
                    Note = closure.Note ?? string.Empty,
                    SortOrder = SortOrders.Closure
                };
            }
        }
    }

    private static IEnumerable<CalendarEventDto> BuildChildEvents(
        List<Child> children,
        List<ChildDayStatus> statuses,
        List<ClosurePeriod> closures,
        DateTime rangeStart,
        DateTime rangeEnd,
        Dictionary<int, Dictionary<DateTime, string>> holidayCache)
    {
        foreach (var child in children)
        {
            yield return new CalendarEventDto
            {
                Id = child.Id,
                Title = $"{child.Name} starter",
                Start = child.StartDate,
                ClassName = "event-start",
                SortOrder = SortOrders.ChildStart,
            };
            yield return new CalendarEventDto
            {
                Id = child.Id,
                Title = $"{child.Name} sidste dag",
                Start = child.EndDate,
                ClassName = "event-child-last-day",
                SortOrder = SortOrders.ChildEnd,
            };
            foreach (var e in BuildBirthdayEvents(child, rangeStart, rangeEnd))
                yield return e;
            foreach (var e in BuildAttendanceEvents(child, statuses, closures, rangeStart, rangeEnd, holidayCache))
                yield return e;
        }
    }

    private static IEnumerable<CalendarEventDto> BuildBirthdayEvents(
        Child child, DateTime rangeStart, DateTime rangeEnd)
    {
        for (var year = rangeStart.Year; year <= rangeEnd.Year; year++)
        {
            DateTime birthday;
            try { birthday = new DateTime(year, child.BirthDate.Month, child.BirthDate.Day); }
            catch { birthday = new DateTime(year, 2, 28); }

            if (birthday < rangeStart || birthday >= rangeEnd) continue;
            if (birthday < child.StartDate.Date || birthday >= child.EndDate.Date) continue;

            var age = year - child.BirthDate.Year;
            yield return new CalendarEventDto
            {
                Id = child.Id,
                Title = $"🇩🇰 {child.Name} {age} år",
                Start = birthday,
                ClassName = "event-birthday",
                Display = "block",
                Note = $"{child.Name} har fødselsdag og bliver {age} år.",
                SortOrder = SortOrders.Birthday,
            };
        }
    }

    private static IEnumerable<CalendarEventDto> BuildAttendanceEvents(
        Child child,
        List<ChildDayStatus> statuses,
        List<ClosurePeriod> closures,
        DateTime rangeStart,
        DateTime rangeEnd,
        Dictionary<int, Dictionary<DateTime, string>> holidayCache)
    {
        var from = child.StartDate.Date > rangeStart ? child.StartDate.Date : rangeStart;
        var to = child.EndDate.Date < rangeEnd ? child.EndDate.Date : rangeEnd;

        var childSortOrder = SortOrders.ChildBase + int.Parse(child.BirthDate.ToString("yyyyMMdd"));

        for (var date = from; date < to; date = date.AddDays(1))
        {
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) continue;

            if (!holidayCache.TryGetValue(date.Year, out var holidays))
            {
                holidays = DanishHolidayService.GetHolidays(date.Year);
                holidayCache[date.Year] = holidays;
            }
            if (holidays.ContainsKey(date)) continue;
            if (closures.Any(x => x.StartDate.Date <= date && x.EndDate.Date >= date)) continue;

            var status = statuses.FirstOrDefault(
                x => x.ChildId == child.Id && x.Date.Date <= date && x.EndDate.Date >= date);
            if (status != null)
            {
                yield return BuildStatusEvent(child, date, status, childSortOrder);
                continue;
            }

            yield return new CalendarEventDto
            {
                Id = child.Id,
                Title = child.Name,
                Start = date,
                ClassName = "event-child",
                SortOrder = childSortOrder,
                DateOfBirth = child.BirthDate
            };
        }
    }

    private static CalendarEventDto BuildStatusEvent(
        Child child, DateTime date, ChildDayStatus status, int sortOrder)
    {
        return status.StatusType switch
        {
            ChildDayStatusType.Syg => new CalendarEventDto
            {
                Id = status.Id,
                Title = $"{child.Name} syg",
                Start = date,
                ClassName = "event-sick",
                Note = status.Note ?? string.Empty,
                SortOrder = sortOrder,
            },
            ChildDayStatusType.Fridag => new CalendarEventDto
            {
                Id = status.Id,
                Title = $"{child.Name} fridag",
                Start = date,
                ClassName = "event-dayoff",
                Note = status.Note ?? string.Empty,
                SortOrder = sortOrder,
            },
            _ => new CalendarEventDto
            {
                Id = status.Id,
                Title = child.Name,
                Start = date,
                ClassName = "event-child",
                SortOrder = sortOrder,
                DateOfBirth = child.BirthDate
            }
        };
    }

    private static IEnumerable<CalendarEventDto> BuildNoteEvents(List<CalendarNote> notes)
    {
        return notes.Select(note => new CalendarEventDto
        {
            Id = note.Id,
            Title = note.Title,
            Start = note.Date,
            ClassName = "event-note",
            Note = note.Note ?? string.Empty,
            SortOrder = SortOrders.Note
        });
    }

    private static class SortOrders
    {
        public const int Holiday = 20;
        public const int Closure = 15;
        public const int ChildBase = 100;
        public const int Birthday = 150_000_000;
        public const int ChildStart = 700_000_000;
        public const int ChildEnd = 710_000_000;
        public const int Note = 900_000_000;
    }

    private sealed class CalendarData
    {
        public List<Child> Children { get; init; } = [];
        public List<ChildDayStatus> Statuses { get; init; } = [];
        public List<CalendarNote> Notes { get; init; } = [];
        public List<ClosurePeriod> Closures { get; init; } = [];
    }
}