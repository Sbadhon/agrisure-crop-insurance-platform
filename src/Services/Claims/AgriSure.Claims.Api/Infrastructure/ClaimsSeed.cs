// using AgriSure.Claims.Api.Data;
// using AgriSure.Claims.Api.Domain;
// using Microsoft.EntityFrameworkCore;

// namespace AgriSure.Claims.Api.Infrastructure;

// public static class ClaimsSeed
// {
//     public static readonly Guid ClaimId = Guid.Parse("40000000-0000-0000-0000-000000000001");

//     public static async Task SeedAsync(ClaimsDbContext db, CancellationToken cancellationToken = default)
//     {
//         if (await db.Claims.AnyAsync(cancellationToken))
//         {
//             return;
//         }

//         var claim = Claim.ReportLoss(
//             ClaimId,
//             "northstar-agency",
//             "CLM-2026-0001",
//             Guid.Parse("20000000-0000-0000-0000-000000000001"),
//             "MPCI-MN-2026-00421",
//             Guid.Parse("10000000-0000-0000-0000-000000000001"),
//             "producer-1001",
//             "Jordan Miller",
//             "Corn",
//             "Washington",
//             Guid.Parse("30000000-0000-0000-0000-000000000001"),
//             "F-102",
//             118.40m,
//             190m,
//             0.75m,
//             4.65m,
//             new DateOnly(2026, 6, 5),
//             "Hail",
//             "Severe hail damaged the northern field after emergence.",
//             "Jordan Miller",
//             "Producer",
//             new DateTimeOffset(2026, 6, 5, 15, 30, 0, TimeSpan.Zero));

//         db.Claims.Add(claim);
//         db.OutboxMessages.Add(ClaimEventFactory.CreateForSeed(
//             claim,
//             "ClaimLossReported",
//             "claims.loss-reported",
//             "Seeded Notice of Loss awaiting assignment."));
//         await db.SaveChangesAsync(cancellationToken);
//     }
// }
