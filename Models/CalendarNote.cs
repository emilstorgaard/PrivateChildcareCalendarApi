using System.ComponentModel.DataAnnotations;

namespace PrivatPasningKalender.Models;

public class CalendarNote
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Overskrift skal udfyldes")]
    [Display(Name = "Overskrift")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Dato")]
    public DateTime Date { get; set; }

    [Display(Name = "Bemærkning")]
    public string? Note { get; set; }
}
