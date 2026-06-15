using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.Sqlite;
using PrivateChildcareCalendarApi.Data;
using PrivateChildcareCalendarApi.Dtos;
using PrivateChildcareCalendarApi.Infrastructure;

namespace PrivateChildcareCalendarApi.Controllers;

[ApiController]
[Route("api/backup")]
public class BackupController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _db;

    public BackupController(IWebHostEnvironment env, IConfiguration configuration, AppDbContext db)
    {
        _env = env;
        _configuration = configuration;
        _db = db;
    }

    [HttpGet]
    public ActionResult<IEnumerable<BackupFileResponse>> GetAll()
    {
        var folder = GetBackupFolder();
        Directory.CreateDirectory(folder);

        var files = Directory.GetFiles(folder, "*.db")
            .Select(x => new FileInfo(x))
            .OrderByDescending(x => x.CreationTime)
            .Select(x => new BackupFileResponse
            {
                FileName = x.Name,
                SizeBytes = x.Length,
                CreatedAt = x.CreationTime
            })
            .ToList();

        return Ok(files);
    }

    [HttpPost]
    public ActionResult<BackupFileResponse> CreateBackup()
    {
        var backupFolder = GetBackupFolder();
        Directory.CreateDirectory(backupFolder);

        var fileName = $"privatpasning_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
        var backupFile = Path.Combine(backupFolder, fileName);
        var dbPath = GetDatabasePath();

        using (var source = new SqliteConnection($"Data Source={dbPath}"))
        using (var destination = new SqliteConnection($"Data Source={backupFile}"))
        {
            source.Open();
            destination.Open();
            source.BackupDatabase(destination);
        }

        var info = new FileInfo(backupFile);

        return Ok(new BackupFileResponse
        {
            FileName = info.Name,
            SizeBytes = info.Length,
            CreatedAt = info.CreationTime
        });
    }

    [HttpGet("{fileName}/download")]
    public IActionResult Download(string fileName)
    {
        if (!IsValidFileName(fileName))
            return BadRequest(new { error = "Ugyldigt filnavn." });

        var path = Path.Combine(GetBackupFolder(), fileName);
        if (!System.IO.File.Exists(path))
            return NotFound(new { error = "Filen blev ikke fundet." });

        var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return File(stream, "application/octet-stream", fileName);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile backupFile)
    {
        if (backupFile == null || backupFile.Length == 0)
            return BadRequest(new { error = "Vælg først en backupfil." });

        if (!backupFile.FileName.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Filen skal være en .db-fil." });

        var folder = GetBackupFolder();
        Directory.CreateDirectory(folder);

        var safeName = Path.GetFileName(backupFile.FileName);

        var targetPath = Path.Combine(folder, safeName);
        if (System.IO.File.Exists(targetPath))
        {
            var nameWithoutExt = Path.GetFileNameWithoutExtension(safeName);
            safeName = $"{nameWithoutExt}_{DateTime.Now:yyyyMMdd_HHmmss}.db";
            targetPath = Path.Combine(folder, safeName);
        }

        await using (var stream = System.IO.File.Create(targetPath))
        {
            await backupFile.CopyToAsync(stream);
        }

        var info = new FileInfo(targetPath);

        return Ok(new BackupFileResponse
        {
            FileName = info.Name,
            SizeBytes = info.Length,
            CreatedAt = info.CreationTime
        });
    }

    [HttpDelete("{fileName}")]
    public IActionResult Delete(string fileName)
    {
        if (!IsValidFileName(fileName))
            return BadRequest(new { error = "Ugyldigt filnavn." });

        var path = Path.Combine(GetBackupFolder(), fileName);
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);

        return NoContent();
    }

    [HttpPost("{fileName}/restore")]
    [EnableRateLimiting("backup")]
    public IActionResult RestoreFromBackup(string fileName)
    {
        if (!IsValidFileName(fileName))
            return BadRequest(new { error = "Ugyldigt filnavn." });

        var backupPath = Path.Combine(GetBackupFolder(), fileName);
        if (!System.IO.File.Exists(backupPath))
            return NotFound(new { error = "Backupfilen blev ikke fundet." });

        var dbPath = GetDatabasePath();

        BackupRestoreService.RestoreFromFile(_db, backupPath, dbPath);

        return Ok(new { message = "Databasen er gendannet fra backup." });
    }

    private static bool IsValidFileName(string fileName)
        => !string.IsNullOrWhiteSpace(fileName)
           && !fileName.Contains("..")
           && Path.GetFileName(fileName) == fileName;

    private string GetBackupFolder()
    {
        var configured = _configuration.GetValue<string>("BackupsPath") ?? "Backups";
        return Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(_env.ContentRootPath, configured);
    }

    private string GetDatabasePath()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=privatpasning.db";
        var builder = new SqliteConnectionStringBuilder(connectionString);
        var dataSource = builder.DataSource;
        return Path.IsPathRooted(dataSource)
            ? dataSource
            : Path.Combine(_env.ContentRootPath, dataSource);
    }
}