namespace PrivatPasningKalender.Services;

public class CalendarEventDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
    public bool AllDay { get; set; } = true;
    public string Note { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
