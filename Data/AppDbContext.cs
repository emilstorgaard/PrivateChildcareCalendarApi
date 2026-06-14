using Microsoft.EntityFrameworkCore;
using PrivateChildcareCalendarApi.Models;

namespace PrivateChildcareCalendarApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Child> Children => Set<Child>();
    public DbSet<WaitingListEntry> WaitingList => Set<WaitingListEntry>();
    public DbSet<ChildDayStatus> ChildDayStatuses => Set<ChildDayStatus>();
    public DbSet<CalendarNote> CalendarNotes => Set<CalendarNote>();
    public DbSet<ClosurePeriod> ClosurePeriods => Set<ClosurePeriod>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}