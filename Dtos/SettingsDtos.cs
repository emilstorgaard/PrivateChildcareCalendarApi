using System.ComponentModel.DataAnnotations;

namespace PrivateChildcareCalendarApi.Dtos;

public class SettingsRequest
{
    [Range(1, 20, ErrorMessage = "Maks antal børn skal være mellem 1 og 20")]
    public int MaxChildren { get; set; } = 5;
}