using PrivatPasningKalender.Models;
using System.ComponentModel.DataAnnotations;

namespace PrivateChildcareCalendarApi.Dtos;

public class DayStatusRequest
{
    [Required]
    public int ChildId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public ChildDayStatusType StatusType { get; set; }

    public string? Note { get; set; }
}

public class DayStatusResponse
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public string? ChildName { get; set; }   // fladt — undgår cirkulær reference
    public DateTime Date { get; set; }
    public DateTime EndDate { get; set; }
    public ChildDayStatusType StatusType { get; set; }
    public string? Note { get; set; }
}