using System.ComponentModel.DataAnnotations;

namespace PrivateChildcareCalendarApi.Models;

public class WaitingListEntry
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Barnets navn skal udfyldes")]
    [Display(Name = "Barnets navn")]
    public string ChildName { get; set; } = string.Empty;

    [Display(Name = "Forælder/kontakt")]
    public string? ContactName { get; set; }

    [Display(Name = "Telefon")]
    public string? Phone { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Ønsket startdato")]
    public DateTime? WantedStartDate { get; set; }

    [Display(Name = "Bemærkning")]
    public string? Note { get; set; }
}
