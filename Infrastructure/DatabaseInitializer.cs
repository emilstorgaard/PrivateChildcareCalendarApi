using PrivateChildcareCalendarApi.Data;

namespace PrivateChildcareCalendarApi.Infrastructure;

public static class DatabaseInitializer
{
    public static void Initialize(AppDbContext db)
    {
        db.Database.EnsureCreated();
    }
}