using Microsoft.EntityFrameworkCore;
using PrivateChildcareCalendarApi.Data;
using PrivateChildcareCalendarApi.Models;

namespace PrivateChildcareCalendarApi.Infrastructure;

// TODO: EF Core Migrations

public static class DatabaseInitializer
{
    public static void Initialize(AppDbContext db)
    {
        db.Database.EnsureCreated();
        EnsureSchema(db);
        EnsureDataMigrations(db);
        EnsureDefaultSettings(db);

        SeedData.EnsureSeedData(db);

        db.SaveChanges();
    }

    private static void EnsureSchema(AppDbContext db)
    {
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ChildDayStatuses (
                Id          INTEGER NOT NULL CONSTRAINT PK_ChildDayStatuses PRIMARY KEY AUTOINCREMENT,
                ChildId     INTEGER NOT NULL,
                Date        TEXT    NOT NULL,
                EndDate     TEXT    NOT NULL,
                StatusType  INTEGER NOT NULL,
                Note        TEXT    NULL,
                CONSTRAINT FK_ChildDayStatuses_Children_ChildId
                    FOREIGN KEY (ChildId) REFERENCES Children (Id) ON DELETE CASCADE
            );");

        db.Database.ExecuteSqlRaw(
            "CREATE INDEX IF NOT EXISTS IX_ChildDayStatuses_ChildId ON ChildDayStatuses (ChildId);");

        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS CalendarNotes (
                Id      INTEGER NOT NULL CONSTRAINT PK_CalendarNotes PRIMARY KEY AUTOINCREMENT,
                Title   TEXT    NOT NULL,
                Date    TEXT    NOT NULL,
                Note    TEXT    NULL
            );");

        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ClosurePeriods (
                Id          INTEGER NOT NULL CONSTRAINT PK_ClosurePeriods PRIMARY KEY AUTOINCREMENT,
                Title       TEXT    NOT NULL,
                StartDate   TEXT    NOT NULL,
                EndDate     TEXT    NOT NULL,
                PeriodType  INTEGER NOT NULL,
                Note        TEXT    NULL
            );");

        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS SystemSettings (
                Id          INTEGER NOT NULL CONSTRAINT PK_SystemSettings PRIMARY KEY AUTOINCREMENT,
                MaxChildren INTEGER NOT NULL
            );");
    }

    private static void EnsureDataMigrations(AppDbContext db)
    {
        try
        {
            db.Database.ExecuteSqlRaw(
                "ALTER TABLE ChildDayStatuses ADD COLUMN EndDate TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
        }
        catch (Exception ex)
        {

        }

        db.Database.ExecuteSqlRaw(
            "UPDATE ChildDayStatuses SET EndDate = Date WHERE EndDate = '2000-01-01 00:00:00' OR EndDate IS NULL;");

        try
        {
            db.Database.ExecuteSqlRaw(
                "ALTER TABLE Children ADD COLUMN EndDate TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
        }
        catch (Exception ex)
        {

        }

        var childrenWithoutEndDate = db.Children
            .Where(c => c.EndDate.Year <= 2000)
            .ToList();

        foreach (var child in childrenWithoutEndDate)
        {
            child.EndDate = child.FreeDate;
        }
    }

    private static void EnsureDefaultSettings(AppDbContext db)
    {
        if (!db.SystemSettings.Any())
        {
            db.SystemSettings.Add(new SystemSetting { MaxChildren = 5 });
        }
    }
}