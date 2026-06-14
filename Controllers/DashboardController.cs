using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateChildcareCalendarApi.Dtos;
using PrivateChildcareCalendarApi.Data;

namespace PrivateChildcareCalendarApi.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> Get()
    {
        var today = DateTime.Today;

        var children = await _db.Children.AsNoTracking().ToListAsync();
        var settings = await _db.SystemSettings.AsNoTracking().FirstOrDefaultAsync();
        var waitingCount = await _db.WaitingList.CountAsync();

        var nextFree = children
            .Where(c => c.EndDate.Date >= today)
            .OrderBy(c => c.EndDate)
            .FirstOrDefault();

        var response = new DashboardResponse
        {
            TotalPlaces = settings?.MaxChildren ?? 5,
            ActiveNow = children.Count(c => c.StartDate.Date <= today && c.EndDate.Date > today),
            FutureChildren = children.Count(c => c.StartDate.Date > today),
            WaitingCount = waitingCount,
            NextFree = nextFree == null ? null : new NextFreeDto
            {
                Name = nextFree.Name,
                EndDate = nextFree.EndDate
            }
        };

        return Ok(response);
    }
}