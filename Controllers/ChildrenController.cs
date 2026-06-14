using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateChildcareCalendarApi.Dtos;
using PrivatPasningKalender.Data;
using PrivatPasningKalender.Models;

namespace PrivateChildcareCalendarApi.Controllers;

[ApiController]
[Route("api/children")]
public class ChildrenController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ChildCapacityValidator _capacityValidator;

    public ChildrenController(AppDbContext db, ChildCapacityValidator capacityValidator)
    {
        _db = db;
        _capacityValidator = capacityValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChildResponse>>> GetAll()
    {
        var children = await _db.Children.AsNoTracking().ToListAsync();

        var result = children
            .OrderBy(c => c.EndDate)
            .ThenBy(c => c.Name)
            .Select(MapToResponse)
            .ToList();

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ChildResponse>> GetById(int id)
    {
        var child = await _db.Children.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (child == null) return NotFound();
        return Ok(MapToResponse(child));
    }

    [HttpPost]
    public async Task<ActionResult<ChildResponse>> Create(ChildRequest request)
    {
        var child = new Child
        {
            Name = request.Name,
            BirthDate = request.BirthDate,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Note = request.Note
        };

        var error = await _capacityValidator.ValidateAsync(child);
        if (error != null)
            return BadRequest(new { error });

        _db.Children.Add(child);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = child.Id }, MapToResponse(child));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ChildRequest request)
    {
        var child = await _db.Children.FindAsync(id);
        if (child == null) return NotFound();

        // Anvend ændringer på et midlertidigt objekt til validering
        var candidate = new Child
        {
            Id = id,
            Name = request.Name,
            BirthDate = request.BirthDate,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Note = request.Note
        };

        var error = await _capacityValidator.ValidateAsync(candidate, id);
        if (error != null)
            return BadRequest(new { error });

        child.Name = request.Name;
        child.BirthDate = request.BirthDate;
        child.StartDate = request.StartDate;
        child.EndDate = request.EndDate;
        child.Note = request.Note;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var child = await _db.Children.FindAsync(id);
        if (child == null) return NotFound();

        _db.Children.Remove(child);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static ChildResponse MapToResponse(Child child) => new()
    {
        Id = child.Id,
        Name = child.Name,
        BirthDate = child.BirthDate,
        StartDate = child.StartDate,
        EndDate = child.EndDate,
        Note = child.Note,
        FreeDate = child.FreeDate,   // beregnet i modellen
        IsFuture = child.IsFuture     // beregnet i modellen
    };
}