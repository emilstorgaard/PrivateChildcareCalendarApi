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

        var activeNowTask = _db.Children.CountAsync(c => c.StartDate <= today && c.EndDate > today);
        var futureChildrenTask = _db.Children.CountAsync(c => c.StartDate > today);
        var waitingCountTask = _db.WaitingList.CountAsync();
        var settingsTask = _db.SystemSettings.AsNoTracking().FirstOrDefaultAsync();
        var nextFreeTask = _db.Children
                                    .AsNoTracking()
                                    .Where(c => c.EndDate >= today)
                                    .OrderBy(c => c.EndDate)
                                    .Select(c => new NextFreeDto
                                    {
                                        Name = c.Name,
                                        EndDate = c.EndDate
                                    })
                                    .FirstOrDefaultAsync();

        await Task.WhenAll(activeNowTask, futureChildrenTask, waitingCountTask, settingsTask, nextFreeTask);

        var response = new DashboardResponse
        {
            TotalPlaces = (await settingsTask)?.MaxChildren ?? 5,
            ActiveNow = await activeNowTask,
            FutureChildren = await futureChildrenTask,
            WaitingCount = await waitingCountTask,
            NextFree = await nextFreeTask
        };

        return Ok(response);
    }
}