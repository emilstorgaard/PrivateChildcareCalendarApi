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

        modelBuilder.Entity<ChildDayStatus>(entity =>
        {
            entity.HasOne(x => x.Child)
                  .WithMany()
                  .HasForeignKey(x => x.ChildId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.ChildId);
            entity.HasIndex(x => new { x.Date, x.EndDate });
        });

        modelBuilder.Entity<ClosurePeriod>()
            .HasIndex(x => new { x.StartDate, x.EndDate });

        modelBuilder.Entity<CalendarNote>()
            .HasIndex(x => x.Date);

        modelBuilder.Entity<SystemSetting>()
            .HasData(new SystemSetting { Id = 1, MaxChildren = 5 });
    }
}