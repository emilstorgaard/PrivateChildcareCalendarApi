namespace PrivateChildcareCalendarApi.Dtos;

public class DashboardResponse
{
    public int TotalPlaces { get; set; }
    public int ActiveNow { get; set; }
    public int FutureChildren { get; set; }
    public int WaitingCount { get; set; }
    public NextFreeDto? NextFree { get; set; }
}

public class NextFreeDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime EndDate { get; set; }
}