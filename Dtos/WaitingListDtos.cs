using System.ComponentModel.DataAnnotations;

namespace PrivateChildcareCalendarApi.Dtos;

public class WaitingListRequest
{
    [Required(ErrorMessage = "Barnets navn skal udfyldes")]
    public string ChildName { get; set; } = string.Empty;

    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public DateTime? WantedStartDate { get; set; }
    public string? Note { get; set; }
}
