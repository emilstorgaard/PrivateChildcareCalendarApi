using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateChildcareCalendarApi.Dtos;
using PrivatPasningKalender.Data;
using PrivatPasningKalender.Models;

namespace PrivateChildcareCalendarApi.Controllers;

[ApiController]
[Route("api/closureperiods")]
public class ClosurePeriodsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ClosurePeriodsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClosurePeriod>>> GetAll()
    {
        var list = await _db.ClosurePeriods
            .OrderByDescending(x => x.StartDate)
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClosurePeriod>> GetById(int id)
    {
        var item = await _db.ClosurePeriods.FindAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ClosurePeriod>> Create(ClosurePeriodRequest request)
    {
        if (request.EndDate.Date < request.StartDate.Date)
            return BadRequest(new { error = "Til dato må ikke være før fra dato." });

        var item = new ClosurePeriod
        {
            Title = request.Title,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            PeriodType = request.PeriodType,
            Note = request.Note
        };

        _db.ClosurePeriods.Add(item);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ClosurePeriodRequest request)
    {
        if (request.EndDate.Date < request.StartDate.Date)
            return BadRequest(new { error = "Til dato må ikke være før fra dato." });

        var item = await _db.ClosurePeriods.FindAsync(id);
        if (item == null) return NotFound();

        item.Title = request.Title;
        item.StartDate = request.StartDate;
        item.EndDate = request.EndDate;
        item.PeriodType = request.PeriodType;
        item.Note = request.Note;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.ClosurePeriods.FindAsync(id);
        if (item == null) return NotFound();

        _db.ClosurePeriods.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}