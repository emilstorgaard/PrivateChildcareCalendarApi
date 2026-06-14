using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateChildcareCalendarApi.Dtos;
using PrivatPasningKalender.Data;
using PrivatPasningKalender.Models;

namespace PrivateChildcareCalendarApi.Controllers;

[ApiController]
[Route("api/waitinglist")]
public class WaitingListController : ControllerBase
{
    private readonly AppDbContext _db;

    public WaitingListController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WaitingListEntry>>> GetAll()
    {
        var entries = await _db.WaitingList
            .OrderBy(w => w.WantedStartDate)
            .ToListAsync();

        return Ok(entries);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WaitingListEntry>> GetById(int id)
    {
        var entry = await _db.WaitingList.FindAsync(id);
        if (entry == null) return NotFound();
        return Ok(entry);
    }

    [HttpPost]
    public async Task<ActionResult<WaitingListEntry>> Create(WaitingListRequest request)
    {
        var entry = new WaitingListEntry
        {
            ChildName = request.ChildName,
            ContactName = request.ContactName,
            Phone = request.Phone,
            WantedStartDate = request.WantedStartDate,
            Note = request.Note
        };

        _db.WaitingList.Add(entry);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, entry);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, WaitingListRequest request)
    {
        var entry = await _db.WaitingList.FindAsync(id);
        if (entry == null) return NotFound();

        entry.ChildName = request.ChildName;
        entry.ContactName = request.ContactName;
        entry.Phone = request.Phone;
        entry.WantedStartDate = request.WantedStartDate;
        entry.Note = request.Note;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entry = await _db.WaitingList.FindAsync(id);
        if (entry == null) return NotFound();

        _db.WaitingList.Remove(entry);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}