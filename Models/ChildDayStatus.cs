using System.ComponentModel.DataAnnotations;

namespace PrivatPasningKalender.Models;

public enum ChildDayStatusType
{
    [Display(Name = "Syg")]
    Syg = 1,

    [Display(Name = "Fridag")]
    Fridag = 2
}

public class ChildDayStatus
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Barn")]
    public int ChildId { get; set; }

    public Child? Child { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Fra dato")]
    public DateTime Date { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Til dato")]
    public DateTime EndDate { get; set; }

    [Required]
    [Display(Name = "Type")]
    public ChildDayStatusType StatusType { get; set; }

    [Display(Name = "Bemærkning")]
    public string? Note { get; set; }
}
