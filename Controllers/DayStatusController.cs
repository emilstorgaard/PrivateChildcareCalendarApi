using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateChildcareCalendarApi.Dtos;
using PrivateChildcareCalendarApi.Data;
using PrivateChildcareCalendarApi.Models;

namespace PrivateChildcareCalendarApi.Controllers;

[ApiController]
[Route("api/daystatus")]
public class DayStatusController : ControllerBase
{
    private readonly AppDbContext _db;

    public DayStatusController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DayStatusResponse>>> GetAll()
    {
        var list = await _db.ChildDayStatuses
            .AsNoTracking()
            .Include(x => x.Child)
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.EndDate)
            .Select(x => new DayStatusResponse
            {
                Id = x.Id,
                ChildId = x.ChildId,
                ChildName = x.Child != null ? x.Child.Name : null,
                Date = x.Date,
                EndDate = x.EndDate,
                StatusType = x.StatusType,
                Note = x.Note
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DayStatusResponse>> GetById(int id)
    {
        var item = await _db.ChildDayStatuses
            .AsNoTracking()
            .Include(x => x.Child)
            .Where(x => x.Id == id)
            .Select(x => new DayStatusResponse
            {
                Id = x.Id,
                ChildId = x.ChildId,
                ChildName = x.Child != null ? x.Child.Name : null,
                Date = x.Date,
                EndDate = x.EndDate,
                StatusType = x.StatusType,
                Note = x.Note
            })
            .FirstOrDefaultAsync();

        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<DayStatusResponse>> Create(DayStatusRequest request)
    {
        if (request.EndDate.Date < request.Date.Date)
            return BadRequest(new { error = "Til dato må ikke være før fra dato." });

        var childExists = await _db.Children.AnyAsync(c => c.Id == request.ChildId);
        if (!childExists)
            return BadRequest(new { error = "Det valgte barn findes ikke." });

        var status = new ChildDayStatus
        {
            ChildId = request.ChildId,
            Date = request.Date,
            EndDate = request.EndDate,
            StatusType = request.StatusType,
            Note = request.Note
        };

        _db.ChildDayStatuses.Add(status);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = status.Id }, MapToResponse(status));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, DayStatusRequest request)
    {
        if (request.EndDate.Date < request.Date.Date)
            return BadRequest(new { error = "Til dato må ikke være før fra dato." });

        var status = await _db.ChildDayStatuses.FindAsync(id);
        if (status == null) return NotFound();

        var childExists = await _db.Children.AnyAsync(c => c.Id == request.ChildId);
        if (!childExists)
            return BadRequest(new { error = "Det valgte barn findes ikke." });

        status.ChildId = request.ChildId;
        status.Date = request.Date;
        status.EndDate = request.EndDate;
        status.StatusType = request.StatusType;
        status.Note = request.Note;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var status = await _db.ChildDayStatuses.FindAsync(id);
        if (status == null) return NotFound();

        _db.ChildDayStatuses.Remove(status);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static DayStatusResponse MapToResponse(ChildDayStatus status) => new()
    {
        Id = status.Id,
        ChildId = status.ChildId,
        ChildName = status.Child?.Name,
        Date = status.Date,
        EndDate = status.EndDate,
        StatusType = status.StatusType,
        Note = status.Note
    };
}