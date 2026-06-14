using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateChildcareCalendarApi.Dtos;
using PrivatPasningKalender.Data;
using PrivatPasningKalender.Models;
using System;

namespace PrivateChildcareCalendarApi.Controllers;

[ApiController]
[Route("api/notes")]
public class NotesController : ControllerBase
{
    private readonly AppDbContext _db;

    public NotesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CalendarNote>>> GetAll()
    {
        var notes = await _db.CalendarNotes
            .OrderByDescending(x => x.Date)
            .ToListAsync();

        return Ok(notes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CalendarNote>> GetById(int id)
    {
        var note = await _db.CalendarNotes.FindAsync(id);
        if (note == null) return NotFound();
        return Ok(note);
    }

    [HttpPost]
    public async Task<ActionResult<CalendarNote>> Create(CalendarNoteRequest request)
    {
        var note = new CalendarNote
        {
            Title = request.Title,
            Date = request.Date,
            Note = request.Note
        };

        _db.CalendarNotes.Add(note);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = note.Id }, note);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CalendarNoteRequest request)
    {
        var note = await _db.CalendarNotes.FindAsync(id);
        if (note == null) return NotFound();

        note.Title = request.Title;
        note.Date = request.Date;
        note.Note = request.Note;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var note = await _db.CalendarNotes.FindAsync(id);
        if (note == null) return NotFound();

        _db.CalendarNotes.Remove(note);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}