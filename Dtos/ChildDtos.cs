using System.ComponentModel.DataAnnotations;

namespace PrivateChildcareCalendarApi.Dtos;

public class ChildRequest
{
    [Required(ErrorMessage = "Barnets navn skal udfyldes")]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime BirthDate { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public string? Note { get; set; }
}

public class ChildResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Note { get; set; }
    public DateTime FreeDate { get; set; }
    public bool IsFuture { get; set; }
}
