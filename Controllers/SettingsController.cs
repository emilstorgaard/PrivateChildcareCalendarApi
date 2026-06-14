using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateChildcareCalendarApi.Dtos;
using PrivatPasningKalender.Data;
using PrivatPasningKalender.Models;

namespace PrivateChildcareCalendarApi.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SettingsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<SystemSetting>> Get()
    {
        var settings = await GetOrCreateSettingsAsync();
        return Ok(settings);
    }

    [HttpPut]
    public async Task<ActionResult<SystemSetting>> Update(SettingsRequest request)
    {
        var settings = await GetOrCreateSettingsAsync();
        settings.MaxChildren = request.MaxChildren;
        await _db.SaveChangesAsync();
        return Ok(settings);
    }

    private async Task<SystemSetting> GetOrCreateSettingsAsync()
    {
        var settings = await _db.SystemSettings.FirstOrDefaultAsync();
        if (settings != null) return settings;

        settings = new SystemSetting { MaxChildren = 5 };
        _db.SystemSettings.Add(settings);
        await _db.SaveChangesAsync();
        return settings;
    }
}