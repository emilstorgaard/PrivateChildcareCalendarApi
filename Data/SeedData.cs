using PrivateChildcareCalendarApi.Models;

namespace PrivateChildcareCalendarApi.Data;

// TODO: do not use seed data

public static class SeedData
{
    public static void EnsureSeedData(AppDbContext db)
    {
        if (!db.Children.Any())
        {
            db.Children.AddRange(
                new Child
                {
                    Name = "Poul",
                    BirthDate = new DateTime(2023, 1, 12),
                    StartDate = new DateTime(2026, 6, 1),
                    EndDate = new DateTime(2029, 1, 1),
                    Note = "Eksempel"
                },
                new Child
                {
                    Name = "Emma",
                    BirthDate = new DateTime(2022, 8, 22),
                    StartDate = new DateTime(2026, 8, 1),
                    EndDate = new DateTime(2028, 8, 1),
                    Note = "Eksempel"
                }
            );
        }

        if (!db.WaitingList.Any())
        {
            db.WaitingList.Add(new WaitingListEntry
            {
                ChildName = "Noah",
                ContactName = "Eksempel forælder",
                WantedStartDate = new DateTime(2026, 9, 1)
            });
        }
    }
}