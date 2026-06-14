using System.ComponentModel.DataAnnotations;

namespace PrivateChildcareCalendarApi.Models;

public class SystemSetting
{
    public int Id { get; set; }

    [Range(1, 20, ErrorMessage = "Maks antal børn skal være mellem 1 og 20")]
    [Display(Name = "Maks antal børn på samme tid")]
    public int MaxChildren { get; set; } = 5;
}
