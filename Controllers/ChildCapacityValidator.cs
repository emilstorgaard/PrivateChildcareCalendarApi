using Microsoft.EntityFrameworkCore;
using PrivatPasningKalender.Data;
using PrivatPasningKalender.Models;

namespace PrivateChildcareCalendarApi.Controllers;

public class ChildCapacityValidator
{
    private readonly AppDbContext _db;

    public ChildCapacityValidator(AppDbContext db) => _db = db;

    /// <summary>
    /// Returnerer en fejlbesked hvis barnet ikke kan oprettes/rettes,
    /// ellers null hvis alt er ok.
    /// </summary>
    public async Task<string?> ValidateAsync(Child child, int? currentChildId = null)
    {
        if (child.EndDate.Date <= child.StartDate.Date)
            return "Slutdato skal være efter pasningsstart.";

        var settings = await _db.SystemSettings.FirstOrDefaultAsync();
        var maxChildren = settings?.MaxChildren ?? 5;

        var overlappingChildren = await _db.Children
            .AsNoTracking()
            .Where(c => !currentChildId.HasValue || c.Id != currentChildId.Value)
            .Where(c => c.StartDate.Date < child.EndDate.Date && c.EndDate.Date > child.StartDate.Date)
            .ToListAsync();

        var worstDay = GetWorstOverlapDay(child, overlappingChildren);

        if (worstDay.Count + 1 > maxChildren)
        {
            var names = string.Join(", ", worstDay.Children.Select(c => c.Name));
            return $"Barnet kan ikke oprettes/rettes, fordi perioden overlapper med maks antal børn. " +
                   $"Den {worstDay.Date:dd-MM-yyyy} vil der være {worstDay.Count + 1} børn mod maks {maxChildren}. " +
                   $"Overlapper med: {names}.";
        }

        return null;
    }

    private static (DateTime Date, int Count, List<Child> Children) GetWorstOverlapDay(
        Child newChild, List<Child> existingChildren)
    {
        var datesToCheck = new List<DateTime> { newChild.StartDate.Date, newChild.EndDate.Date };
        datesToCheck.AddRange(existingChildren.Select(c => c.StartDate.Date));
        datesToCheck.AddRange(existingChildren.Select(c => c.EndDate.Date.AddDays(-1)));

        datesToCheck = datesToCheck
            .Where(d => d >= newChild.StartDate.Date && d < newChild.EndDate.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        var worstDate = newChild.StartDate.Date;
        var worstChildren = new List<Child>();

        foreach (var date in datesToCheck)
        {
            var overlapping = existingChildren
                .Where(c => c.StartDate.Date <= date && c.EndDate.Date > date)
                .OrderBy(c => c.Name)
                .ToList();

            if (overlapping.Count > worstChildren.Count)
            {
                worstDate = date;
                worstChildren = overlapping;
            }
        }

        return (worstDate, worstChildren.Count, worstChildren);
    }
}