using System.ComponentModel.DataAnnotations;

namespace PrivateChildcareCalendarApi.Dtos;

public class CalendarNoteRequest
{
    [Required(ErrorMessage = "Overskrift skal udfyldes")]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    public string? Note { get; set; }
}
