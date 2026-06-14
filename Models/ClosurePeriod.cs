using System.ComponentModel.DataAnnotations;

namespace PrivateChildcareCalendarApi.Models;

public enum ClosurePeriodType
{
    [Display(Name = "Ferie")]
    Ferie = 1,

    [Display(Name = "Lukkedag")]
    Lukkedag = 2,

    [Display(Name = "Andet")]
    Andet = 3
}

public class ClosurePeriod
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Overskrift")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Fra dato")]
    public DateTime StartDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Til dato")]
    public DateTime EndDate { get; set; }

    [Required]
    [Display(Name = "Type")]
    public ClosurePeriodType PeriodType { get; set; } = ClosurePeriodType.Ferie;

    [Display(Name = "Bemærkning")]
    public string? Note { get; set; }
}