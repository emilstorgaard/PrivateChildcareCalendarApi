namespace PrivateChildcareCalendarApi.Dtos;

public class BackupFileResponse
{
    public string FileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
}
