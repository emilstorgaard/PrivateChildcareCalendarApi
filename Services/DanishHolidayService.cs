namespace PrivatPasningKalender.Services;

public static class DanishHolidayService
{
    public static Dictionary<DateTime, string> GetHolidays(int year)
    {
        var easter = GetEasterSunday(year);
        var holidays = new Dictionary<DateTime, string>
        {
            [new DateTime(year, 1, 1)] = "Nytårsdag",
            [easter.AddDays(-7)] = "Palmesøndag",
            [easter.AddDays(-3)] = "Skærtorsdag",
            [easter.AddDays(-2)] = "Langfredag",
            [easter] = "Påskedag",
            [easter.AddDays(1)] = "2. påskedag",
            [easter.AddDays(39)] = "Kristi himmelfartsdag",
            [easter.AddDays(49)] = "Pinsedag",
            [easter.AddDays(50)] = "2. pinsedag",
            [new DateTime(year, 6, 5)] = "Grundlovsdag",
            [new DateTime(year, 12, 24)] = "Juleaftensdag",
            [new DateTime(year, 12, 25)] = "Juledag",
            [new DateTime(year, 12, 26)] = "2. juledag",
            [new DateTime(year, 12, 31)] = "Nytårsaftensdag"
        };

        if (year < 2024)
            holidays[easter.AddDays(26)] = "Store bededag";

        return holidays;
    }

    public static bool IsHoliday(DateTime date, out string name)
    {
        var holidays = GetHolidays(date.Year);
        return holidays.TryGetValue(date.Date, out name!);
    }

    private static DateTime GetEasterSunday(int year)
    {
        var a = year % 19;
        var b = year / 100;
        var c = year % 100;
        var d = b / 4;
        var e = b % 4;
        var f = (b + 8) / 25;
        var g = (b - f + 1) / 3;
        var h = (19 * a + b - d - g + 15) % 30;
        var i = c / 4;
        var k = c % 4;
        var l = (32 + 2 * e + 2 * i - h - k) % 7;
        var m = (a + 11 * h + 22 * l) / 451;
        var month = (h + l - 7 * m + 114) / 31;
        var day = ((h + l - 7 * m + 114) % 31) + 1;
        return new DateTime(year, month, day);
    }
}
