using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PrivatPasningKalender.Data;

namespace PrivatPasningKalender.Infrastructure;

public static class BackupRestoreService
{
    /// <summary>
    /// Gendanner databasen fra en backup-fil UDEN genstart,
    /// via SQLite's BackupDatabase (kopierer indhold ind i den aktive DB).
    /// </summary>
    public static void RestoreFromFile(AppDbContext db, string backupFilePath, string activeDbPath)
    {
        if (!File.Exists(backupFilePath))
            throw new FileNotFoundException("Backupfilen blev ikke fundet.", backupFilePath);

        // 1. Luk EF Core's forbindelse + ryd connection pool,
        //    så vi har eksklusiv adgang til den aktive DB-fil.
        db.Database.GetDbConnection().Close();
        SqliteConnection.ClearAllPools();

        // 2. Kopiér backup-filens indhold ind i den aktive database.
        using (var source = new SqliteConnection($"Data Source={backupFilePath}"))
        using (var destination = new SqliteConnection($"Data Source={activeDbPath}"))
        {
            source.Open();
            destination.Open();
            source.BackupDatabase(destination);
        }

        // 3. Ryd pools igen, så efterfølgende queries får friske forbindelser
        //    der ser de nye data.
        SqliteConnection.ClearAllPools();
    }
}