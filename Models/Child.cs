using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivateChildcareCalendarApi.Models;

public class Child
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Barnets navn skal udfyldes")]
    [Display(Name = "Barnets navn")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Fødselsdato")]
    public DateTime BirthDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Pasningsstart")]
    public DateTime StartDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Slutdato")]
    public DateTime EndDate { get; set; }

    [Display(Name = "Bemærkning")]
    public string? Note { get; set; }

    [NotMapped]
    [Display(Name = "Standard ledig dato")]
    public DateTime FreeDate => new DateTime(BirthDate.Year + 3, BirthDate.Month, 1);

    [NotMapped]
    public bool IsFuture => StartDate.Date > DateTime.Today;
}
