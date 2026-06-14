using PrivateChildcareCalendarApi.Models;
using System.ComponentModel.DataAnnotations;

namespace PrivateChildcareCalendarApi.Dtos;

public class ClosurePeriodRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public ClosurePeriodType PeriodType { get; set; } = ClosurePeriodType.Ferie;

    public string? Note { get; set; }
}