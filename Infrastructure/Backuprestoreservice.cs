using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PrivateChildcareCalendarApi.Data;

namespace PrivateChildcareCalendarApi.Infrastructure;

public static class BackupRestoreService
{
    public static void RestoreFromFile(AppDbContext db, string backupFilePath, string activeDbPath)
    {
        if (!File.Exists(backupFilePath))
            throw new FileNotFoundException("Backupfilen blev ikke fundet.", backupFilePath);

        db.Database.GetDbConnection().Close();
        SqliteConnection.ClearAllPools();

        using (var source = new SqliteConnection($"Data Source={backupFilePath}"))
        using (var destination = new SqliteConnection($"Data Source={activeDbPath}"))
        {
            source.Open();
            destination.Open();
            source.BackupDatabase(destination);
        }

        SqliteConnection.ClearAllPools();
    }
}